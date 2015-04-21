//-----------------------------------------------------------------------
// <copyright file="Network.cs" company="Beemway">
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
  /// Represents a network.
  /// </summary>
  [XmlRoot("network")]
  public class Network
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Network"/> class.
    /// </summary>
    public Network()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets a collection of network stats properties.
    /// </summary>
    [XmlArray("network-stats")]
    [XmlArrayItem("property")]
    public Collection<NetworkStatsProperty> NetworkStats
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of updates in this network.
    /// </summary>
    [XmlElement("updates")]
    public Updates Updates
    {
      get;
      set;
    }
    #endregion
  }
}
