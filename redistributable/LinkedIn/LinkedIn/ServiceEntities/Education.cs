//-----------------------------------------------------------------------
// <copyright file="Education.cs" company="Beemway">
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
  /// Represents a education.
  /// </summary>
  [XmlType("education")]  
  public class Education
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Education"/> class.
    /// </summary>
    public Education()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the education.
    /// </summary>
    [XmlElement("id")]
    public int Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the school name of the education.
    /// </summary>
    [XmlElement("school-name")]
    public string SchoolName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the notes of the education.
    /// </summary>
    [XmlElement("notes")]
    public string Notes
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the activities a person participated in during his education.
    /// </summary>
    [XmlElement("activities")]
    public string Activities
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the degree of the education.
    /// </summary>
    [XmlElement("degree")]
    public string Degree
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the field of study at the school.
    /// </summary>
    [XmlElement("field-of-study")]
    public string FieldOfStudy
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the start date of the education.
    /// </summary>
    [XmlElement("start-date")]
    public Date StartDate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the end date of the education.
    /// </summary>
    [XmlElement("end-date")]
    public Date EndDate
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the person currently is educated at this school.
    /// </summary>
    [XmlElement("is-current")]
    public bool IsCurrent
    {
      get;
      set;
    }
    #endregion
  }
}
