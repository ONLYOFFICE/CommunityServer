//-----------------------------------------------------------------------
// <copyright file="NetworkSearchType.cs" company="Beemway">
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
  /// The different types of network searches.
  /// </summary>
  public enum NetworkSearchType
  {
    /// <summary>
    /// Search within a persons three degree network.
    /// </summary>
    [Description("in")]
    In = 0,

    /// <summary>
    /// Search outside a persons three degree network.
    /// </summary>
    [Description("out")]
    Out = 1
  }
}
