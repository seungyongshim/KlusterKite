﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiKit.Core.Tests.BusinessObjects
{
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Runtime.Remoting.Contexts;
    using System.Threading;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;

    using Castle.MicroKernel.Registration;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    using TaxiKit.Core.Cluster;
    using TaxiKit.Core.TestKit;
    using TaxiKit.Core.TestKit.Moq;

    using Xunit;
    using Xunit.Abstractions;

    public class ClusterBusinessObjectActorSupervisorTest : BaseActorTest
    {
        public ClusterBusinessObjectActorSupervisorTest(ITestOutputHelper output)
            : base(output, GetConfig())
        {
            this.WindsorContainer.Register(Component.For<TestSupervisorActor>().LifestyleTransient());
            this.WindsorContainer.Register(Component.For<TestObjectActor>().LifestyleTransient());
            this.WindsorContainer.Register(Component.For<ITestOutputHelper>().Instance(output));
        }

        public static Config GetConfig()
        {
            return ConfigurationFactory.Empty
                .WithFallback(
                    ConfigurationFactory.ParseString("akka.remote.helios.tcp.hostname = 127.0.0.1"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.helios.tcp.port = 0"))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            $"akka.cluster.roles = [\"test\"]"))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            $"akka.cluster.seed-nodes = []"))
                .WithFallback(
                    ConfigurationFactory.ParseString(
                        "akka.actor.provider = \"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster\""))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))
                    .WithFallback(ConfigurationFactory.ParseString("min-nr-of-members = 1"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.loglevel = INFO"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 1s"));
        }

        [Fact]
        public void SupervisorStartTest()
        {
            var redis = new ConcurrentDictionary<string, object>();
            this.WinsorBind<IConnectionMultiplexer>(() => new RedisConnectionMoq(redis));

            var superVisor = this.ActorOfAsTestActorRef<TestSupervisorActor>(this.Sys.DI().Props<TestSupervisorActor>());

            superVisor.Tell(new ClusterEvent.MemberUp(Member.Create(Cluster.Get(this.Sys).SelfUniqueAddress, MemberStatus.Up, ImmutableHashSet.Create("test"))));
            superVisor.Tell(new ClusterEvent.RoleLeaderChanged("test", Cluster.Get(this.Sys).SelfAddress));

            var echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-1" };
            superVisor.Tell(echoMessage);
            var created = this.ExpectMsg<string>();
            Assert.Equal("Created 1", created);
            var response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);

            echoMessage = new EchoMessage { Id = "1", Text = "Hello 1-2" };
            superVisor.Tell(echoMessage);
            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);

            echoMessage = new EchoMessage { Id = "2", Text = "Hello 2-1" };
            superVisor.Tell(echoMessage);
            created = this.ExpectMsg<string>();
            Assert.Equal("Created 2", created);
            response = this.ExpectMsg<EchoMessage>();
            Assert.Equal(echoMessage, response);
        }
    }

    public class EchoMessage : IMessageToBusinessObjectActor
    {
        public string Id { get; set; }
        public string Text { get; set; }

        public static bool operator !=(EchoMessage left, EchoMessage right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(EchoMessage left, EchoMessage right)
        {
            return Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((EchoMessage)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Id != null ? this.Id.GetHashCode() : 0) * 397) ^ (this.Text != null ? this.Text.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"Object {this.Id}: {this.Text}";
        }

        protected bool Equals(EchoMessage other)
        {
            return string.Equals(this.Id, other.Id) && string.Equals(this.Text, other.Text);
        }
    }

    [UsedImplicitly]
    public class TestObjectActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly ITestOutputHelper output;
        private readonly IActorRef testActor;
        private string id;

        public TestObjectActor(IActorRef testActor, ITestOutputHelper output)
        {
            this.testActor = testActor;
            this.output = output;
            this.Receive<ClusterBusinessObjectActorSupervisor<TestObjectActor>.SetObjectId>(m => this.SetCurrentId(m));
            this.Receive<object>(m => this.StashMessage(m));
        }

        public IStash Stash { get; set; }

        private void OnEchoMessage(EchoMessage message)
        {
            var echoMessage = new EchoMessage { Id = this.id, Text = message.Text };
            Context.GetLogger().Info(echoMessage.ToString());
            this.testActor.Tell(echoMessage);
        }

        private void SetCurrentId(ClusterBusinessObjectActorSupervisor<TestObjectActor>.SetObjectId message)
        {
            this.id = message.Id;
            this.Become(
                () =>
                    {
                        this.Receive<EchoMessage>(m => this.OnEchoMessage(m));
                        this.Stash.UnstashAll();
                    });
            this.testActor.Tell($"Created {this.id}");
        }

        private void StashMessage(object message)
        {
            this.Stash.Stash();
        }
    }

    [UsedImplicitly]
    public class TestSupervisorActor : ClusterBusinessObjectActorSupervisor<TestObjectActor>
    {
        public TestSupervisorActor(IConnectionMultiplexer redisConnection)
            : base(redisConnection)
        {
        }

        protected override string ClusterRole => "test";
    }
}