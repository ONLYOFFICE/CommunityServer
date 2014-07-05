//-----------------------------------------------------------------------
// <copyright file="Company.cs" company="Beemway">
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
  /// Represents a company.
  /// </summary>
  [XmlRoot("company")]
  public class Company
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Company"/> class.
    /// </summary>
    public Company()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the company.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the company.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the type of the company.
    /// </summary>
    [XmlElement("company-type")]
    public CompanyType Type
    {
      get;
      set;
    }
    #endregion
  }
}
