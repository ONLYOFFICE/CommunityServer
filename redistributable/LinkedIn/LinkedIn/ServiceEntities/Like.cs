//-----------------------------------------------------------------------
// <copyright file="Like.cs" company="Beemway">
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
  /// Represents a like.
  /// </summary>
  [XmlRoot("like")]
  public class Like
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Like"/> class.
    /// </summary>
    public Like()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the person who (didn't) liked an update.
    /// </summary>
    [XmlElement("person")]
    public Person Person
    {
      get;
      set;
    }
    #endregion
  }
}
