//-----------------------------------------------------------------------
// <copyright file="Facets.cs" company="Beemway">
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
  /// Represents a collection of facets.
  /// </summary>
  [XmlRoot("facets")]
  public class Facets : PagedCollection<Facet>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Facets"/> class.
    /// </summary>
    public Facets()
      : base()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the collection of <see cref="Facet" /> objects representing the facets.
    /// </summary>
    [XmlElement("facet")]
    public Collection<Facet> Items
    {
      get;
      set;
    }
    #endregion
  }
}
