//-----------------------------------------------------------------------
// <copyright file="NetworkStatsProperty.cs" company="Beemway">
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
  /// Represents a network stats property (part of the network updates).
  /// </summary>
  [XmlType("property")]
  public class NetworkStatsProperty
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkStatsProperty"/> class.
    /// </summary>
    public NetworkStatsProperty()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the key of the property.
    /// </summary>
    [XmlAttribute("key")]
    public string Key
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the value of the property.
    /// </summary>
    [XmlText]
    public string Value
    {
      get;
      set;
    }
    #endregion
  }
}
