//-----------------------------------------------------------------------
// <copyright file="ProfileType.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.ComponentModel;

namespace LinkedIn
{
  /// <summary>
  /// The different types of profiles.
  /// </summary>
  public enum ProfileType
  {
    /// <summary>
    /// The standard profile.
    /// </summary>
    [Description("standard")]
    Standard = 0,

    /// <summary>
    /// The public profile.
    /// </summary>
    [Description("public")]
    Public = 1
  }
}
