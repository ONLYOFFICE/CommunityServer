//-----------------------------------------------------------------------
// <copyright file="FacetCode.cs" company="Beemway">
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
  /// The possible facet codes.
  /// </summary>
  public enum FacetCode
  {
    /// <summary>
    /// A geographical region. This is not necessarily a country. It could be a city or regional area.
    /// </summary>
    [Description("location")]
    Location = 0,

    /// <summary>
    /// An industry field.
    /// </summary>
    /// <remarks>For a complete list see: http://developer.linkedin.com/docs/DOC-1011</remarks>
    [Description("industry")]
    Industry = 1,

    /// <summary>
    /// A specific relationship to the member's social network.
    /// </summary>
    [Description("network")]
    Network = 2,

    /// <summary>
    /// A member locale set to a specific language.
    /// </summary>
    [Description("language")]
    Language = 3,

    /// <summary>
    /// A member's current companies.
    /// </summary>
    [Description("current-company")]
    CurrentCompany = 4,

    /// <summary>
    /// A member's past companies.
    /// </summary>
    [Description("past-company")]
    PastCompany = 5,

    /// <summary>
    /// A members current or previous school.
    /// </summary>
    [Description("school")]
    School = 6
  }
}
