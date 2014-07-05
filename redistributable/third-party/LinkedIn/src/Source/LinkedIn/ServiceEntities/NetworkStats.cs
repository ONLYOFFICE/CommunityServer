//-----------------------------------------------------------------------
// <copyright file="NetworkStats.cs" company="Beemway">
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
  /// Represents a collection of network stats.
  /// </summary>
  [XmlRoot("network-stats")]
  public class NetworkStats : PagedCollection<NetworkStatsProperty>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkStats"/> class.
    /// </summary>
    public NetworkStats()
      : base()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the collection of <see cref="NetworkStatsProperty" /> objects representing the network stats.
    /// </summary>
    [XmlElement("property")]
    public Collection<NetworkStatsProperty> Items
    {
      get;
      set;
    }
    #endregion
  }
}
