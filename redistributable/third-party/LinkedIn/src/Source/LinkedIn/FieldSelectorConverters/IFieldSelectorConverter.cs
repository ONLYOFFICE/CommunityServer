//-----------------------------------------------------------------------
// <copyright file="IFieldSelectorConverter.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using LinkedIn.ServiceEntities;

namespace LinkedIn.FieldSelectorConverters
{
  internal interface IFieldSelectorConverter<T>
  {
    /// <summary>
    /// Convert the field selectors to a comma seperated string.
    /// </summary>
    /// <param name="fieldSelectors">The field selectors to convert.</param>
    /// <returns>A comma seperated <see cref="string"/>.</returns>
    string ConvertToString(IEnumerable<T> fieldSelectors);
  }
}
