﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The overall resource migration state
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.ORM;

    /// <summary>
    /// The overall resource migration state
    /// </summary>
    [ApiDescription("The overall resource migration state", Name = "ResourceState")]
    public class ResourceState
    {
        /// <summary>
        /// Gets or sets the current resource state with active migration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the current resource state with active migration")]
        public MigrationActorMigrationState MigrationState { get; set; }

        /// <summary>
        /// Gets or sets the current resource state without active migration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the current resource state without active migration")]
        public MigrationActorReleaseState ReleaseState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether some resource operation is in progress
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether some resource operation is in progress")]
        public bool OperationIsInProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new migration can be initiated
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether a new migration can be initiated")]
        public bool CanCreateMigration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resource can be migrated
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether resource can be migrated")]
        public bool CanMigrateResources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether node update can be initiated. The nodes will be updated to the destination release.
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether node update can be initiated. The nodes will be updated to the destination release.")]
        public bool CanUpdateNodesToDestination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether node update can be initiated. The nodes will be updated to the source release.
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether node update can be initiated. The nodes will be updated to the source release.")]
        public bool CanUpdateNodesToSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current migration can be canceled at this stage
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether current migration can be canceled at this stage")]
        public bool CanCancelMigration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current migration can be finished at this stage
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether current migration can be finished at this stage")]
        public bool CanFinishMigration { get; set; }

        /// <summary>
        /// Gets or sets the list of migration steps
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of migration steps")]
        public List<EnMigrationSteps> MigrationSteps { get; set; }

        /// <summary>
        /// Gets or sets the current migration step
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the current migration step")]
        public EnMigrationSteps? CurrentMigrationStep { get; set; }
    }
}
