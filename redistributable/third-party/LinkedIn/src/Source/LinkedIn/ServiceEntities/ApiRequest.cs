//-----------------------------------------------------------------------
// <copyright file="ApiRequest.cs" company="Beemway">
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
  /// Represents a API request.
  /// </summary>
  [XmlRoot("api-standard-profile-request")]
  public class ApiRequest
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiRequest"/> class.
    /// </summary>
    public ApiRequest()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the url of the API request.
    /// </summary>
    [XmlElement(ElementName = "url")]
    public string Url
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of http headers.
    /// </summary>
    [XmlArray(ElementName = "headers")]
    [XmlArrayItem("http-header")]
    public Collection<HttpHeader> Headers
    {
      get;
      set;
    }
    #endregion
  }
}
