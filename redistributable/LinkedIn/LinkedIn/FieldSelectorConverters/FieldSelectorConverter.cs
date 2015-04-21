//-----------------------------------------------------------------------
// <copyright file="FieldSelectorConverter.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using LinkedIn.ServiceEntities;
using LinkedIn.Utility;

namespace LinkedIn.FieldSelectorConverters
{
  internal abstract class FieldSelectorConverter<T> : IFieldSelectorConverter<T> where T : struct
  {
    private bool onlyChildFieldSelectors;
    private string fieldSelectorName;
    private Collection<IFieldSelectorConverter<T>> fieldSelectorConverters;

    protected Collection<IFieldSelectorConverter<T>> FieldSelectorConverters
    {
      get { return this.fieldSelectorConverters; }
      set { this.fieldSelectorConverters = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSelectorConverter{T}"/> class.
    /// </summary>
    /// <param name="fieldSelectorName">The name of the field selector.</param>
    protected FieldSelectorConverter(string fieldSelectorName)
      : this(false, fieldSelectorName)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSelectorConverter{T}"/> class.
    /// </summary>
    /// <param name="onlyChildFieldSelectors">Whether to process only the child field selectors.</param>
    /// <param name="fieldSelectorName">The name of the field selector.</param>    
    protected FieldSelectorConverter(bool onlyChildFieldSelectors, string fieldSelectorName)
    {
      this.onlyChildFieldSelectors = onlyChildFieldSelectors;
      this.fieldSelectorName = fieldSelectorName;
      this.FieldSelectorConverters = new Collection<IFieldSelectorConverter<T>>();
    }

    /// <summary>
    /// Convert the field selectors to a comma seperated string.
    /// </summary>
    /// <param name="fieldSelectors">The field selectors to convert.</param>
    /// <returns>A comma seperated <see cref="string"/>.</returns>
    public string ConvertToString(IEnumerable<T> fieldSelectors)      
    {
      StringBuilder sb = new StringBuilder();
      foreach (T fieldSelector in fieldSelectors)
      {
        if (this.IsValidField(fieldSelector))
        {
          sb.AppendFormat("{0},", EnumHelper.GetDescription(fieldSelector as Enum));
        }
      }

      foreach (IFieldSelectorConverter<T> fieldSelectorConverter in this.FieldSelectorConverters)
      {
        string childFieldSelectorsString = fieldSelectorConverter.ConvertToString(fieldSelectors);
        if (string.IsNullOrEmpty(childFieldSelectorsString) == false)
        {
          sb.AppendFormat("{0},", childFieldSelectorsString);
        }
      }

      string fieldSelectorsString = sb.ToString();
      if (string.IsNullOrEmpty(fieldSelectorsString))
      {
        return string.Empty;
      }
      
      // Remove the latest ','
      fieldSelectorsString = fieldSelectorsString.Remove(fieldSelectorsString.Length - 1);

      if (onlyChildFieldSelectors)
      {
        return fieldSelectorsString;
      }

      return string.Format("{0}:({1})", this.fieldSelectorName, fieldSelectorsString);
    }
    
    /// <summary>
    /// Whether or not a field selector is a valid field for this converter.
    /// </summary>
    /// <param name="fieldSelector">The field selector to check.</param>
    /// <returns><b>True</b> if the field selector is valid; otherwise <b>false</b>.</returns>
    internal abstract bool IsValidField(T fieldSelector);
  }
}
