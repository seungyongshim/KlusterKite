﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ClientProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Authentication
{
    using System.Threading.Tasks;

    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.NodeManager.Authentication.Clients;
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// Creates predefined client applications
    /// </summary>
    [UsedImplicitly]
    public class ClientProvider : IClientProvider
    {
        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem system;

        /// <summary>
        /// The launcher secret
        /// </summary>
        private readonly string launcherSecret;

        /// <inheritdoc />
        public ClientProvider(ActorSystem system)
        {
            this.system = system;
            this.launcherSecret = system.Settings.Config.GetString("KlusterKite.NodeManager.Authentication.LauncherSecret");
        }

        /// <inheritdoc />
        public decimal Priority => 10M;

        /// <inheritdoc />
        public Task<IClient> GetClientAsync(string clientId, string secret)
        {
            switch (clientId)
            {
                case WebApplication.WebApplicationClientId:
                    return Task.FromResult<IClient>(new WebApplication(this.system));
                case Launcher.LauncherClientId:
                    if (secret == this.launcherSecret)
                    {
                        return Task.FromResult<IClient>(new Launcher());
                    }

                    break;
            }

            return Task.FromResult<IClient>(null);
        }
    }
}
