//-----------------------------------------------------------------------
// <copyright file="Location.cs" company="Beemway">
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
  /// Represents a location.
  /// </summary>
  [XmlRoot("location")]
  public class Location
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Location"/> class.
    /// </summary>
    public Location()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the name of the location.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Country" /> of the location.
    /// </summary>
    [XmlElement("country")]
    public Country Country
    {
      get;
      set;
    }
    #endregion
  }
}
