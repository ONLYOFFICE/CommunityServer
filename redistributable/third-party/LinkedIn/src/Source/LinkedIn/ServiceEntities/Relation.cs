//-----------------------------------------------------------------------
// <copyright file="Relation.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a relation between two members.
  /// </summary>
  [XmlRoot("relation")]
  public class Relation
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Relation"/> class.
    /// </summary>
    public Relation()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the degree distance between two members.
    /// </summary>
    [XmlElement("distance")]
    public int Distance
    {
      get;
      set;
    }
    #endregion
  }
}
