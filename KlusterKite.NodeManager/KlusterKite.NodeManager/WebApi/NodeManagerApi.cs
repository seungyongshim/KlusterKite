// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerApi.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The node manager api
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Attributes.Authorization;
    using KlusterKite.API.Client;
    using KlusterKite.NodeManager.Client;
    using KlusterKite.NodeManager.Client.Messages;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.Messages;
    using KlusterKite.Security.Attributes;

    using ConfigurationUtils = KlusterKite.Core.ConfigurationUtils;

    /// <summary>
    /// The node manager api
    /// </summary>
    [ApiDescription("The main KlusterKite node managing methods", Name = "Root")]
    public class NodeManagerApi
    {
        /// <summary>
        /// The actor system.
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// The package repository
        /// </summary>
        private readonly IPackageRepository packageRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerApi"/> class.
        /// </summary>
        /// <param name="actorSystem">
        /// The actor system.
        /// </param>
        /// <param name="packageRepository">The package repository</param>
        public NodeManagerApi(ActorSystem actorSystem, IPackageRepository packageRepository)
        {
            this.actorSystem = actorSystem;
            this.packageRepository = packageRepository;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(actorSystem);
            this.ClusterManagement = new ClusterManagement(this.actorSystem);
        }

        /// <summary>
        /// Gets the list of defined system privileges
        /// </summary>
        [UsedImplicitly]
        [RequirePrivilege(
            Privileges.GetPrivilegesList,
            Scope = EnPrivilegeScope.User,
            AddActionNameToRequiredPrivilege = true)]
        public List<PrivilegeDescription> DefinedPrivilegeDescriptions => Utils.DefinedPrivileges.ToList();

        /// <summary>
        /// Gets the cluster management
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the cluster management")]
        public ClusterManagement ClusterManagement { get; }

        /// <summary>
        /// Gets the list of packages in the nuget repository
        /// </summary>
        [UsedImplicitly]
        [DeclareConnection("The packages in the Nuget repository")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetPackages, Scope = EnPrivilegeScope.User)]
        public NugetPackagesConnection NugetPackages => new NugetPackagesConnection(this.packageRepository);

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        private TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// Gets current cluster active nodes descriptions
        /// </summary>
        /// <returns>The list of descriptions</returns>
        [UsedImplicitly]
        [DeclareField("The list of known active nodes")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetActiveNodeDescriptions, Scope = EnPrivilegeScope.User)]
        public async Task<List<NodeDescription>> GetActiveNodeDescriptions()
        {
            var activeNodeDescriptions = await this.actorSystem.ActorSelection(GetManagerActorProxyPath())
                                             .Ask<List<NodeDescription>>(
                                                 new ActiveNodeDescriptionsRequest(),
                                                 this.AkkaTimeout);

            return activeNodeDescriptions.OrderBy(n => n.NodeTemplate)
                .ThenBy(n => n.ContainerType)
                .ThenBy(n => n.NodeAddress.ToString())
                .ToList();
        }

        /// <summary>
        /// Gets current cluster node template usage for debug purposes
        /// </summary>
        /// <returns>Current cluster statistics</returns>
        [UsedImplicitly]
        [DeclareField("Current cluster node template usage for debug purposes")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.GetTemplateStatistics, Scope = EnPrivilegeScope.User)]
        public async Task<TemplatesUsageStatistics> GetTemplateStatistics()
        {
            return await this.actorSystem.ActorSelection(GetManagerActorProxyPath())
                       .Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest(), this.AkkaTimeout);
        }

        /// <summary>
        /// Gets the history of migrations
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>the history of migrations</returns>
        [UsedImplicitly]
        [DeclareConnection("the cluster migration management")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(
            Privileges.ClusterMigration,
            Scope = EnPrivilegeScope.User,
            AddActionNameToRequiredPrivilege = true)]
        public MigrationConnection Migrations(RequestContext context)
        {
            return new MigrationConnection(
                this.actorSystem,
                GetManagerActorProxyPath(),
                this.AkkaTimeout,
                context);
        }

        /// <summary>
        /// The connection to the <see cref="Role"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [RequireSession]
        [RequireUser]
        [DeclareConnection(
            CanCreate = true,
            CreateDescription = "Creates the new draft configuration",
            CanUpdate = true,
            UpdateDescription = "Updates the draft configuration",
            CanDelete = true,
            DeleteDescription = "Removes the draft configuration",
            Description = "KlusterKite managing system security roles")]
        [RequirePrivilege(Privileges.Configuration, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public ConfigurationConnection Configurations(RequestContext context)
        {
            return new ConfigurationConnection(this.actorSystem, GetManagerActorProxyPath(), this.AkkaTimeout, context);
        }

        /// <summary>
        /// The connection to the <see cref="Role"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(
            CanCreate = true,
            CreateDescription = "Creates the new managing system role",
            CanUpdate = true,
            UpdateDescription = "Updates the managing system role",
            Description = "KlusterKite managing system security roles")]
        [RequirePrivilege(Privileges.Role, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public RolesConnection Roles(RequestContext context)
        {
            return new RolesConnection(this.actorSystem, GetManagerActorProxyPath(), this.AkkaTimeout, context);
        }

        /// <summary>
        /// Manual node upgrade request
        /// </summary>
        /// <param name="address">Address of node to upgrade</param>
        /// <returns>Execution task</returns>
        [UsedImplicitly]
        [DeclareMutation("Manual node reboot request")]
        [RequireSession]
        [RequireUser]
        [RequirePrivilege(Privileges.UpgradeNode, Scope = EnPrivilegeScope.User)]
        [LogAccess]
        public async Task<MutationResult<bool>> UpgradeNode(string address)
        {
            var result = await this.actorSystem.ActorSelection(GetManagerActorProxyPath())
                             .Ask<bool>(new NodeUpgradeRequest { Address = Address.Parse(address) }, this.AkkaTimeout);
            return new MutationResult<bool> { Result = result };
        }

        /// <summary>
        /// The connection to the <see cref="User"/>
        /// </summary>
        /// <param name="context">The request context</param>
        /// <returns>The data connection</returns>
        [UsedImplicitly]
        [DeclareConnection(
            CanCreate = true,
            CreateDescription = "Creates the new user",
            CanUpdate = true,
            UpdateDescription = "Updates the user",
            Description = "KlusterKite managing system users")]
        [RequirePrivilege(Privileges.User, Scope = EnPrivilegeScope.User, AddActionNameToRequiredPrivilege = true)]
        public UsersConnection Users(RequestContext context)
        {
            return new UsersConnection(this.actorSystem, GetManagerActorProxyPath(), this.AkkaTimeout, context);
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        internal static string GetManagerActorProxyPath() => "/user/NodeManager/NodeManagerProxy";
    }
}