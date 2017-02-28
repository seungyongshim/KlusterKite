﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The node manager API provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using Akka.Actor;

    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// The node manager API provider
    /// </summary>
    [ApiDescription(Description = "The root provider", Name = "ClusterKitNodeApi")]
    public class ApiProvider : API.Provider.ApiProvider
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// The node manager data.
        /// </summary>
        private readonly NodeManagerApi nodeManagerData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiProvider"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        public ApiProvider(ActorSystem actorSystem)
        {
            this.actorSystem = actorSystem;
            this.nodeManagerData = new NodeManagerApi(actorSystem);
        }

        /// <summary>
        /// Gets the main node manager api
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "The ClusterKit node managing API")]
        public NodeManagerApi NodeManagerData
        {
            get
            {
                this.actorSystem.Log.Info("{Type}: NodeManagerData accessed", this.GetType().Name);
                return this.nodeManagerData;
            }
        }

        /// <summary>
        /// Gets the current user data API
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The current user API</returns>
        [UsedImplicitly]
        [DeclareField(Description = "The current user")]
        public CurrentUserApi Me(RequestContext context) => new CurrentUserApi(context);
    }
}
