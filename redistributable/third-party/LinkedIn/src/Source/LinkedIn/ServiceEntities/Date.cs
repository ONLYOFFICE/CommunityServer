//-----------------------------------------------------------------------
// <copyright file="Date.cs" company="Beemway">
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
  /// Represents a date.
  /// </summary>
  [XmlRoot("date")]
  public class Date
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Date"/> class.
    /// </summary>
    public Date()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the year of the date.
    /// </summary>
    [XmlElement("year")]
    public int Year
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the month of the date.
    /// </summary>
    [XmlElement("month")]
    public int Month
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the day of the date.
    /// </summary>
    [XmlElement("day")]
    public int Day
    {
      get;
      set;
    }
    #endregion
  }
}
