//-----------------------------------------------------------------------
// <copyright file="HttpHeader.cs" company="Beemway">
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
  /// Represents a http header.
  /// </summary>
  [XmlType("http-header")]
  public class HttpHeader
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="HttpHeader"/> class.
    /// </summary>
    public HttpHeader()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the name of the http header.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the value of the http header.
    /// </summary>
    [XmlElement("value")]
    public string Value
    {
      get;
      set;
    }
    #endregion
  }
}
