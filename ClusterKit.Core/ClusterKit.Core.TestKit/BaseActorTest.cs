﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseActorTest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   <seealso cref="TestKit" /> extension class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.TestKit
{
    using System;
    using System.Linq.Expressions;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.CastleWindsor;
    using Akka.DI.Core;
    using Akka.TestKit;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using CommonServiceLocator.WindsorAdapter;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

    using Serilog;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// <seealso cref="TestKit"/> extension class
    /// </summary>
    /// <typeparam name="TConfigurator">
    /// Class, that describes test configuration
    /// </typeparam>
    public abstract class BaseActorTest<TConfigurator> : HackedBaseActorTest where TConfigurator : TestConfigurator, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseActorTest{TConfigurator}"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        protected BaseActorTest(ITestOutputHelper output) : base(CreateTestActorSystem(output))
        {
            this.Initialize();
        }

        /// <summary>
        /// Create a new actor as child of <see cref="ActorSystem"/> and returns it as <see cref="TestActorRef{TActor}"/>
        /// to enable access to the underlying actor instance via <see cref="TestActorRefBase{TActor}.UnderlyingActor"/>.
        /// Uses an expression that calls the constructor of <typeparamref name="TActor"/>.
        /// <example>
        /// <code>
        /// ActorOf&lt;MyActor&gt;(()=&gt;new MyActor("value", 4711), "test-actor")
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TActor">
        /// The type of the actor.
        /// </typeparam>
        /// <param name="factory">
        /// An expression that calls the constructor of <typeparamref name="TActor"/>
        /// </param>
        /// <param name="name">
        /// Optional: The name.
        /// </param>
        /// <returns>
        /// The actor ref
        /// </returns>
        public new TestActorRef<TActor> ActorOfAsTestActorRef<TActor>(Expression<Func<TActor>> factory, string name = null) where TActor : ActorBase
        {
            return new TestActorRef<TActor>(this.Sys, Props.Create(factory).WithDispatcher("akka.test.calling-thread-dispatcher"), null, name);
        }

        /// <summary>
        /// Create a new actor as child of <see cref="ActorSystem"/> and returns it as <see cref="TestActorRef{TActor}"/>
        /// to enable access to the underlying actor instance via <see cref="TestActorRefBase{TActor}.UnderlyingActor"/>.
        /// Uses an expression that calls the constructor of <typeparamref name="TActor"/>.
        /// <example>
        /// <code>
        /// ActorOf&lt;MyActor&gt;(()=&gt;new MyActor("value", 4711), "test-actor")
        /// </code>
        /// </example>
        /// </summary>
        /// <typeparam name="TActor">
        /// The type of the actor.
        /// </typeparam>
        /// <param name="props">
        /// The props to create an actor <typeparamref name="TActor"/>
        /// </param>
        /// <param name="name">
        /// Optional: The name.
        /// </param>
        /// <returns>
        /// The actor ref
        /// </returns>
        public new TestActorRef<TActor> ActorOfAsTestActorRef<TActor>(Props props, string name = null) where TActor : ActorBase
        {
            return new TestActorRef<TActor>(this.Sys, props.WithDispatcher("akka.test.calling-thread-dispatcher"), null, name);
        }

        /// <summary>
        /// Cleanup after test run
        /// </summary>
        [UsedImplicitly]
        public virtual void Cleanup()
        {
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            this.Sys.Log.Info("Test dispose launched");
            try
            {
                this.Sys.Terminate().Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException)
            {
                this.Sys.Log.Error($"Failed to stop actor system");
                throw;
            }

            this.Cleanup();
        }

        /// <summary>
        /// Waiting for message
        /// <seealso cref="TestActorForwarder"/>
        /// that was sent to the specified address
        /// </summary>
        /// <param name="path">
        /// specified address
        /// </param>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <returns>
        /// The original message
        /// </returns>
        protected T ExpectMsg<T>(string path)
        {
            var message = this.ExpectMsg<TestMessage<T>>();
            Assert.Equal(path, message.ReceiverPathRooted);
            return message.Message;
        }

        /// <summary>
        /// Waiting for message
        /// <seealso cref="TestActorForwarder"/>
        /// that was sent to the specified address on specified time
        /// </summary>
        /// <param name="path">
        /// specified address
        /// </param>
        /// <param name="timeout">
        /// Timespan when message should arrive
        /// </param>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <remarks>
        /// It is supposed that between now and timespan they will be now messages, and message should arrive exactly at specified time
        /// </remarks>
        /// <returns>
        /// The original message
        /// </returns>
        protected T ExpectMsg<T>(string path, TimeSpan timeout)
        {
            var message = this.ExpectTestMsg<T>(timeout);
            Assert.Equal(path, message.ReceiverPathRooted);
            return message.Message;
        }

        /// <summary>
        /// Expecting that there is no new messages
        /// </summary>
        protected void ExpectNoTestMsg()
        {
            // CallingThreadDispatcher.WaitForAllDone();
            if (this.HasMessages)
            {
                var message = this.ExpectMsg<object>();
                if (message == null)
                {
                    Assert.False(true, $"Expected no messages, but got null message");
                }
                else if (message.GetType().IsGenericType && message.GetType().GetGenericTypeDefinition() == typeof(TestMessage<>))
                {
                    var path = (string)message.GetType().GetProperty("ReceiverPathRooted").GetValue(message);
                    var type = message.GetType().GenericTypeArguments[0];
                    Assert.False(true, $"Expected no messages, but got message of type {type.FullName} to {path}");
                }
                else
                {
                    Assert.False(true, $"Expected no messages, but got nonforwarded message {message.GetType().FullName}");
                }
            }
        }

        /// <summary>
        /// Expects test message <seealso cref="TestActorForwarder"/>
        /// </summary>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <returns>
        /// The <see cref="TestMessage{T}"/>.
        /// </returns>
        protected TestMessage<T> ExpectTestMsg<T>()
        {
            return this.ExpectMsg<TestMessage<T>>();
        }

        /// <summary>
        /// Waiting for message
        /// <seealso cref="TestActorForwarder"/>
        /// that was sent on specified time
        /// </summary>
        /// <param name="timeout">
        /// Timespan when message should arrive
        /// </param>
        /// <typeparam name="T">
        /// The original message type
        /// </typeparam>
        /// <returns>
        /// The <see cref="TestMessage{T}"/>.
        /// </returns>
        /// <remarks>
        /// It is supposed that between now and timespan they will be now messages, and message should arrive exactly at specified time
        /// </remarks>
        protected TestMessage<T> ExpectTestMsg<T>(TimeSpan timeout)
        {
            return this.ExpectMsg<TestMessage<T>>(timeout);
        }

        /// <summary>
        /// Register dependency injection component
        /// </summary>
        /// <typeparam name="T">
        /// Type of component
        /// </typeparam>
        /// <param name="generator">
        /// Component generation factory
        /// </param>
        [UsedImplicitly]
        protected void WinsorBind<T>(Func<T> generator)
        {
            this.WindsorContainer.Register(Component.For(typeof(T)).UsingFactoryMethod(generator).LifestyleTransient());
        }

        /// <summary>
        /// Creating actor system for test
        /// </summary>
        /// <param name="output">
        /// Xunit output
        /// </param>
        /// <returns>
        /// actor system for test
        /// </returns>
        private static TestDescription CreateTestActorSystem(ITestOutputHelper output)
        {
            var loggerConfig =
                new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.TextWriter(new XunitOutputWriter(output));
            Serilog.Log.Logger = loggerConfig.CreateLogger();

            var container = new WindsorContainer();
            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            var configurator = new TConfigurator();
            foreach (var pluginInstaller in configurator.GetPluginInstallers())
            {
                container.Install(pluginInstaller);
            }

            var config = configurator.GetAkkaConfig(container);

            var testActorSystem = ActorSystem.Create("test", config);
            testActorSystem.AddDependencyResolver(new WindsorDependencyResolver(container, testActorSystem));
            container.Register(Component.For<ActorSystem>().Instance(testActorSystem).LifestyleSingleton());
            container.Register(Component.For<IWindsorContainer>().Instance(container).LifestyleSingleton());
            container.Register(Component.For<Config>().Instance(testActorSystem.Settings.Config).LifestyleSingleton());

            if (configurator.RunPostStart)
            {
                BaseInstaller.RunPostStart(container);
            }

            return new TestDescription
            {
                System = testActorSystem,
                Container = container
            };
        }

        /// <summary>
        /// Actor system initialization
        /// </summary>
        private void Initialize()
        {
        }
    }
}