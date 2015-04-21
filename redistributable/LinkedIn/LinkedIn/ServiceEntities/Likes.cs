//-----------------------------------------------------------------------
// <copyright file="Likes.cs" company="Beemway">
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
  /// Represents a collection of likes.
  /// </summary>
  [XmlRoot("likes")]
  public class Likes : PagedCollection<Like>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Likes"/> class.
    /// </summary>
    public Likes()
      : base()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the collection of <see cref="Like" /> objects representing the likes.
    /// </summary>
    [XmlElement("like")]
    public Collection<Like> Items
    {
      get;
      set;
    }
    #endregion
  }
}
