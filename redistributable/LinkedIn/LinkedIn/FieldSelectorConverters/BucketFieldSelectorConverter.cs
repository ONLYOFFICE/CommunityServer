//-----------------------------------------------------------------------
// <copyright file="BucketFieldSelectorConverter.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Text;
using LinkedIn.ServiceEntities;
using LinkedIn.Utility;

namespace LinkedIn.FieldSelectorConverters
{
  /// <summary>
  /// Converter for facet fields.
  /// </summary>
  internal class BucketFieldSelectorConverter : FieldSelectorConverter<FacetField>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BucketFieldSelectorConverter"/> class.
    /// </summary>
    internal BucketFieldSelectorConverter()
      : base(Constants.BucketsFieldSelector)
    {
    }

    /// <summary>
    /// Whether or not a facet field is a valid field for this converter.
    /// </summary>
    /// <param name="facetField">The facet field to check.</param>
    /// <returns><b>True</b> if the facet field is valid; otherwise <b>false</b>.</returns>
    internal override bool IsValidField(FacetField facetField)
    {
      return facetField == FacetField.BucketName ||
        facetField == FacetField.BucketCode ||
        facetField == FacetField.BucketCount ||
        facetField == FacetField.BucketSelected;
    }
  }
}
