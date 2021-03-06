﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PrivilegesContainerAttribute.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Marks static classes as containers of used privileges.
//   All public string constans will be treated as defined privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Attributes
{
    using System;

    /// <summary>
    /// Marks static classes as containers of used privileges.
    /// All public string constants will be treated as defined privilege.
    /// It is recommended to place such classes in widely spread clint libraries
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PrivilegesContainerAttribute : Attribute
    {
    }
}
