//-----------------------------------------------------------------------
// <copyright file="MemberUrl.cs" company="Beemway">
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
  /// Represents a url shared by a person.
  /// </summary>
  [XmlType("member-url")]
  public class MemberUrl
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberUrl"/> class.
    /// </summary>
    public MemberUrl()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the url.
    /// </summary>
    [XmlElement("url")]
    public string Url
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the url.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }
    #endregion
  }
}
