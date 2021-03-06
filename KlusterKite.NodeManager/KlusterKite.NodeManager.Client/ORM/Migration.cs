﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Migration.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The history record describing cluster migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.Data.CRUD;

    /// <summary>
    /// The history record describing cluster migration
    /// </summary>
    [ApiDescription("The history record describing cluster migration", Name = "Migration")]
    public class Migration : IObjectWithId<int>
    {
        /// <summary>
        /// Gets or sets the migration id
        /// </summary>
        [Key]
        [UsedImplicitly]
        [DeclareField("The migration id", IsKey = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "serial")]  // TODO: check and remove that Npgsql.EntityFrameworkCore.PostgreSQL can generate serial columns on migration without this kludge
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current migration is in process
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating that current migration is in process")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the migration state
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration state")]
        public EnMigrationState State { get; set; }

        /// <summary>
        /// Gets or sets the migration start time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration start time")]
        public DateTimeOffset Started { get; set; }

        /// <summary>
        /// Gets or sets the migration finish time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration finish time")]
        public DateTimeOffset? Finished { get; set; }

        /// <summary>
        /// Gets or sets the list of migration logs
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of migration logs")]
        public List<MigrationLogRecord> Logs { get; set; }

        /// <summary>
        /// Gets or sets the previous configuration id
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(FromConfiguration))]
        [DeclareField("the previous configuration id")]
        public int FromConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the new configuration id
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(ToConfiguration))]
        [DeclareField("the new configuration id")]
        public int ToConfigurationId { get; set; }

        /// <summary>
        /// Gets or sets the previous configuration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the previous configuration")]
        public Configuration FromConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the new configuration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the new configuration")]
        public Configuration ToConfiguration { get; set; }

        /// <inheritdoc />
        public int GetId()
        {
            return this.Id;
        }
    }
}
