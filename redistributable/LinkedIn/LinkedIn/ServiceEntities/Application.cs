//-----------------------------------------------------------------------
// <copyright file="Application.cs" company="Beemway">
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
  /// Represents an application.
  /// </summary>
  [XmlType("application")]  
  public class Application
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Application"/> class.
    /// </summary>
    public Application() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the name of the application.
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
