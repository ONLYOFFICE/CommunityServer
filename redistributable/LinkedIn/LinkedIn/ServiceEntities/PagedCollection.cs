//-----------------------------------------------------------------------
// <copyright file="PagedCollection.cs" company="Beemway">
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
  /// A base class for paged collections.
  /// </summary>
  /// <typeparam name="T">Base item type for this collection.</typeparam>
  public class PagedCollection<T>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedCollection{T}"/> class.
    /// </summary>
    public PagedCollection()
      : base()
    { 
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the total number of items in the total collection.
    /// </summary>
    [XmlAttribute(AttributeName = "total")]
    public int Total
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the starting location within the total collection.
    /// </summary>
    [XmlAttribute(AttributeName = "start")]
    public int Start
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the number of results within the collection.
    /// </summary>
    [XmlAttribute(AttributeName = "count")]
    public int NumberOfResults
    {
      get;
      set;
    }
    #endregion
  }
}
