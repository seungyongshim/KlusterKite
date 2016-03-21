﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerReceiverActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Small client actor for node managing. Provides current node configuration information and executes update related commands
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client
{
    using System;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Core;
    using ClusterKit.NodeManager.Client.Messages;

    /// <summary>
    /// Small client actor for node managing. Provides current node configuration information and executes update related commands
    /// </summary>
    public class NodeManagerReceiverActor : ReceiveActor
    {
        /// <summary>
        /// Current node description
        /// </summary>
        private readonly NodeDescription description;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerReceiverActor"/> class.
        /// </summary>
        public NodeManagerReceiverActor()
        {
            var requestIdString = Context.System.Settings.Config.GetString("ClusterKit.NodeManager.RequestId");
            Guid requestId;
            if (!Guid.TryParse(requestIdString, out requestId))
            {
                requestId = Guid.NewGuid();
            }

            this.description = new NodeDescription
            {
                NodeAddress = Cluster.Get(Context.System).SelfAddress,
                NodeTemplate =
                                Context.System.Settings.Config.GetString(
                                    "ClusterKit.NodeManager.NodeTemplate"),
                ContainerType =
                                Context.System.Settings.Config.GetString(
                                    "ClusterKit.NodeManager.ContainerType"),
                NodeTemplateVersion =
                                Context.System.Settings.Config.GetInt(
                                    "ClusterKit.NodeManager.NodeTemplateVersion"),
                RequestId = requestId,
                Modules =
                                AppDomain.CurrentDomain.GetAssemblies()
                                .Where(
                                    a =>
                                    a.GetTypes()
                                        .Any(t => t.IsSubclassOf(typeof(BaseInstaller))))
                                .Select(
                                    a =>
                                    new NodeDescription.ModuleDescription
                                    {
                                        Name = a.GetName().Name,
                                        Version =
                                                a.GetName()
                                                .Version
                                                .ToString()
                                    })
                                .ToList()
            };

            this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(this.description));
            this.Receive<ShutdownMessage>(m => Context.System.Terminate());
        }
    }
}