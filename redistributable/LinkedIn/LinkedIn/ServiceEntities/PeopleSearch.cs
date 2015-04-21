//-----------------------------------------------------------------------
// <copyright file="PeopleSearch.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents the result from a people search.
  /// </summary>
  [XmlRoot("people-search")]
  public class PeopleSearch
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="PeopleSearch"/> class.
    /// </summary>
    public PeopleSearch()
      : base()
    { 
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the found <see cref="Person" /> objects.
    /// </summary>
    [XmlElement("people")]
    public People People
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the number of results of the search query.
    /// </summary>
    [XmlElement("num-results")]
    public int NumberOfResults
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the found <see cref="Facet" /> objects.
    /// </summary>
    [XmlElement("facets")]
    public Facets Facets
    {
      get;
      set;
    }
    #endregion
  }
}
