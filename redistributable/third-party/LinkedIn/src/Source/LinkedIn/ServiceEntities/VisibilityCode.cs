//-----------------------------------------------------------------------
// <copyright file="VisibilityCode.cs" company="Beemway">
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
  /// The different visibility codes.
  /// </summary>
  [Flags]
  public enum VisibilityCode
  {
    /// <summary>
    /// A unknown visibility code.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// All members.
    /// </summary>
    [Description("anyone")]
    Anyone = 1,

    /// <summary>
    /// Only the person his connections
    /// </summary>
    [Description("connections-only")]
    ConnectionsOnly = 2
  }
}
