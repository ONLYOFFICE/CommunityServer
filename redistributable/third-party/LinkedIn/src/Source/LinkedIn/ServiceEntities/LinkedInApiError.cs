//-----------------------------------------------------------------------
// <copyright file="LinkedInApiError.cs" company="Beemway">
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
  /// Represents an error returned by the LinkedIn API.
  /// </summary>
  [XmlRoot("error")]
  public class LinkedInApiError
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="LinkedInApiError"/> class.
    /// </summary>
    public LinkedInApiError()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the http status of the error.
    /// </summary>
    [XmlElement("status")]
    public int Status
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the timestamp of the error.
    /// </summary>
    [XmlElement("timestamp")]
    public long Timestamp
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the code of the error.
    /// </summary>
    [XmlElement("error-code")]
    public string ErrorCode
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the message of the error.
    /// </summary>
    [XmlElement("message")]
    public string Message
    {
      get;
      set;
    }
    #endregion
  }
}
