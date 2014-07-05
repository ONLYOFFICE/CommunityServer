//-----------------------------------------------------------------------
// <copyright file="SiteRequest.cs" company="Beemway">
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
  /// Represents a site request.
  /// </summary>
  [XmlRoot("site-standard-profile-request")]
  public class SiteRequest
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="SiteRequest"/> class.
    /// </summary>
    public SiteRequest()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the url of the request.
    /// </summary>
    [XmlElement(ElementName = "url")]
    public string Url
    {
      get;
      set;
    }
    #endregion
  }
}
