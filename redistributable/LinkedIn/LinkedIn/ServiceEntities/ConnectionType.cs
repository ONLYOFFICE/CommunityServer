//-----------------------------------------------------------------------
// <copyright file="ConnectionType.cs" company="Beemway">
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
  /// The different types of connections.
  /// </summary>
  public enum ConnectionType
  {
    /// <summary>
    /// The connection type is friend.
    /// </summary>
    [Description("friend")]
    Friend = 0
  }
}
