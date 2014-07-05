//-----------------------------------------------------------------------
// <copyright file="MemberGroup.cs" company="Beemway">
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
  /// Represents a member-group.
  /// </summary>
  [XmlType("member-group")]  
  public class MemberGroup
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberGroup"/> class.
    /// </summary>
    public MemberGroup()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the member group.
    /// </summary>
    [XmlElement("id")]
    public int Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the member group.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the <see cref="SiteRequest" /> url of the member group.
    /// </summary>
    [XmlElement("site-group-request")]
    public SiteRequest SiteGroupRequest
    {
      get;
      set;
    }
    #endregion
  }
}
