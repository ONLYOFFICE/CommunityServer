//-----------------------------------------------------------------------
// <copyright file="Scope.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// The different types of scopes.
  /// </summary>
  public enum Scope
  {
    /// <summary>
    /// The scope is the current user.
    /// </summary>
    [Description("self")]
    Self = 0,

    /// <summary>
    /// The scope is the current user his connections.
    /// </summary>
    [Description("")]
    Connections = 1
  }
}
