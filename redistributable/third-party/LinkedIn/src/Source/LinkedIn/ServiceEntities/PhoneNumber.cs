//-----------------------------------------------------------------------
// <copyright file="PhoneNumber.cs" company="Beemway">
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
  /// Represents a phone number.
  /// </summary>
  [XmlType("phone-number")]  
  public class PhoneNumber
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneNumber"/> class.
    /// </summary>
    public PhoneNumber()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the type of phone number.
    /// </summary>
    /// <remarks>Possible values are: home, work, and mobile.</remarks>
    [XmlElement("phone-type")]
    public string Type
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the actual phone number.
    /// </summary>
    [XmlElement("phone-number")]
    public string Number
    {
      get;
      set;
    }
    #endregion
  }
}
