//-----------------------------------------------------------------------
// <copyright file="ShareSource.cs" company="Beemway">
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
  /// Represents a share source.
  /// </summary>
  [XmlType("source")]  
  public class ShareSource
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ShareSource"/> class.
    /// </summary>
    public ShareSource() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the source service provider.
    /// </summary>
    [XmlElement("service-provider")]
    public ServiceProvider ServiceProvider
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the source application.
    /// </summary>
    [XmlElement("application")]
    public Application Application
    {
      get;
      set;
    }
    #endregion
  }
}
