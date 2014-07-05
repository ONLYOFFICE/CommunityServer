//-----------------------------------------------------------------------
// <copyright file="Modified.cs" company="Beemway">
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
  /// The different ways a collection can be modified.
  /// </summary>
  public enum Modified
  {
    /// <summary>
    /// All the ways a collection can be modified.
    /// </summary>
    All = 0,

    /// <summary>
    /// An item in the collection is updated.
    /// </summary>
    [Description("updated")]
    Updated = 1,

    /// <summary>
    /// A new item is added to a collection.
    /// </summary>
    [Description("new")]
    New = 2
  }
}
