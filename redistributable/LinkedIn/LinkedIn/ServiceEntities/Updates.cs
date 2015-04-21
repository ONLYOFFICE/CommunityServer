//-----------------------------------------------------------------------
// <copyright file="Updates.cs" company="Beemway">
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
  /// Represents a collection of updates.
  /// </summary>
  [XmlRoot("updates")]
  public class Updates : PagedCollection<Update>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Updates"/> class.
    /// </summary>
    public Updates()
      : base()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the collection of <see cref="Update" /> objects representing the updates.
    /// </summary>
    [XmlElement("update")]
    public Collection<Update> Items
    {
      get;
      set;
    }
    #endregion
  }
}
