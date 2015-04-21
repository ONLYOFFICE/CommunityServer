//-----------------------------------------------------------------------
// <copyright file="CompanyType.cs" company="Beemway">
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
  /// Represents a company type.
  /// </summary>
  [XmlRoot("company-type")]
  public class CompanyType
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="CompanyType"/> class.
    /// </summary>
    public CompanyType()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the code of the company type.
    /// </summary>
    [XmlElement("code")]
    public string Code
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the company type.
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
