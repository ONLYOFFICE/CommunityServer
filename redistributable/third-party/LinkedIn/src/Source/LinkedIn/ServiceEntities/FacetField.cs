//-----------------------------------------------------------------------
// <copyright file="FacetField.cs" company="Beemway">
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
  /// The possible facet fields.
  /// </summary>
  public enum FacetField
  {
    /// <summary>
    /// The name of a facet (human readable).
    /// </summary>
    [Description("name")]
    Name = 0,

    /// <summary>
    /// The code of a facet (computer processable).
    /// </summary>
    [Description("code")]
    Code = 1,

    /// <summary>
    /// The name of a bucket (human readable).
    /// </summary>
    [Description("name")]
    BucketName = 2,

    /// <summary>
    /// The code of a bucket (computer processable).
    /// </summary>
    [Description("code")]
    BucketCode = 3,

    /// <summary>
    /// The number of results inside the bucket.
    /// </summary>
    [Description("count")]
    BucketCount = 4,

    /// <summary>
    /// Whether this bucket's results are included in your search query.
    /// </summary>
    [Description("selected")]
    BucketSelected = 5
  }
}
