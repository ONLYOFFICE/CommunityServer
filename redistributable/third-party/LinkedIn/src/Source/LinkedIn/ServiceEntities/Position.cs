//-----------------------------------------------------------------------
// <copyright file="Position.cs" company="Beemway">
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
  /// Represents a position.
  /// </summary>
  [XmlType("position")]  
  public class Position
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Position"/> class.
    /// </summary>
    public Position()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the position.
    /// </summary>
    [XmlElement("id")]
    public int Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the title of the position.
    /// </summary>
    [XmlElement("title")]
    public string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the summary of the position.
    /// </summary>
    [XmlElement("summary")]
    public string Summary
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the start date of the position.
    /// </summary>
    [XmlElement("start-date")]
    public Date StartDate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the end date of the position.
    /// </summary>
    [XmlElement("end-date")]
    public Date EndDate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the person currently hold this position.
    /// </summary>
    [XmlElement("is-current")]
    public bool IsCurrent
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Company" /> of the position.
    /// </summary>
    [XmlElement("company")]
    public Company Company
    {
      get;
      set;
    }
    #endregion
  }
}
