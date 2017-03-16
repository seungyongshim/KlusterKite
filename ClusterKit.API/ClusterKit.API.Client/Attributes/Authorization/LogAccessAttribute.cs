﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogAccessAttribute.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the LogAccessAttribute type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes.Authorization
{
    using System;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The access to the marked field will be logged to security log with specified severity
    /// </summary>
    /// <remarks>
    /// In case of multiple attributes applied to the same case the one with the highest severity will be used
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class LogAccessAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the severity
        /// </summary>
        public EnSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets the log type.
        /// </summary>
        /// <remarks>
        /// If absent, the most suitable type will be applied automatically
        /// </remarks>
        public SecurityLog.EnType? Type { get; set; }

        /// <summary>
        /// Gets or sets the log message
        /// </summary>
        /// <remarks>
        /// If absent, the api field path will be logged
        /// </remarks>
        public string LogMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of connection actions to apply attribute to
        /// </summary>
        /// <returns>
        /// Usually data modification operation a logged by themselves at operation place
        /// </returns>
        public EnConnectionAction ConnectionActions { get; set; } = EnConnectionAction.All;

        /// <summary>
        /// Creates <see cref="LogAccessRule"/> from this attribute
        /// </summary>
        /// <returns>The <see cref="LogAccessRule"/></returns>
        public LogAccessRule CreateRule()
        {
            return new LogAccessRule
                       {
                           Type = this.Type,
                           ConnectionActions = this.ConnectionActions,
                           Severity = this.Severity,
                           LogMessage = this.LogMessage
                       };
        }
    }
}
