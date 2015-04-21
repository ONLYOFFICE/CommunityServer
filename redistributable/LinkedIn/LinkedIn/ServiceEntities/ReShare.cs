//-----------------------------------------------------------------------
// <copyright file="ReShare.cs" company="Beemway">
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

using LinkedIn.Utility;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a reshare.
  /// </summary>
  [XmlRoot("share")]
  public class ReShare
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ReShare"/> class.
    /// </summary>
    public ReShare()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the comment of the reshare.
    /// </summary>
    [XmlElement("comment")]
    public string Comment
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the attribution.
    /// </summary>
    [XmlElement("attribution")]
    public Attribution Attribution
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the visibility of the reshare.
    /// </summary>
    [XmlElement("visibility")]
    public Visibility Visibility
    {
      get;
      set;
    }
    #endregion
  }
}
