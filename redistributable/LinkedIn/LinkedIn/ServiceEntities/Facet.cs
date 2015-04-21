//-----------------------------------------------------------------------
// <copyright file="Facet.cs" company="Beemway">
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
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a facet.
  /// </summary>
  [XmlType("facet")]  
  public class Facet
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Facet"/> class.
    /// </summary>
    public Facet() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the name of the facet.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the code of the facet.
    /// </summary>
    [XmlElement("code")]
    public string Code
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the collection of <see cref="Bucket" /> objects representing the buckets.
    /// </summary>
    [XmlArray("buckets")]
    [XmlArrayItem("bucket")]
    public Collection<Bucket> Buckets
    {
      get;
      set;
    }
    #endregion
  }
}
