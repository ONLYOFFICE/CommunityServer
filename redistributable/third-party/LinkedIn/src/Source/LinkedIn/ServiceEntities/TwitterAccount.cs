//-----------------------------------------------------------------------
// <copyright file="TwitterAccount.cs" company="Beemway">
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
  /// Represents a im account.
  /// </summary>
  [XmlType("twitter-account")]  
  public class TwitterAccount
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="TwitterAccount"/> class.
    /// </summary>
    public TwitterAccount()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of twitter account.
    /// </summary>
    [XmlElement("provider-account-id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the twitter account.
    /// </summary>
    [XmlElement("provider-account-name")]
    public string Name
    {
      get;
      set;
    }
    #endregion
  }
}
