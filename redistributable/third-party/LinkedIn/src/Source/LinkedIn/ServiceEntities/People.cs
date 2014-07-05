//-----------------------------------------------------------------------
// <copyright file="People.cs" company="Beemway">
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
  /// Represents a collection of people.
  /// </summary>
  [XmlRoot("people")]
  public class People : PagedCollection<Person>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="People"/> class.
    /// </summary>
    public People()
      : base()
    { 
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the collection of <see cref="Person" /> objects.
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
