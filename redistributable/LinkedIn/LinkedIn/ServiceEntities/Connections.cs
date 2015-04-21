//-----------------------------------------------------------------------
// <copyright file="Connections.cs" company="Beemway">
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
  /// Represents a collection of <see cref="Person"/> objects.
  /// </summary>
  [XmlRoot("connections")]
  public class Connections : PagedCollection<Person>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Connections"/> class.
    /// </summary>
    public Connections()
      : base()
    { 
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets a collection of <see cref="Person" /> objects representing the connections.
    /// </summary>
    [XmlElement("person")]
    public Collection<Person> Items
    {
      get;
      set;
    }
    #endregion
  }
}
