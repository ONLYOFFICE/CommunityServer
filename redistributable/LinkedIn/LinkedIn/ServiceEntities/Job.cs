//-----------------------------------------------------------------------
// <copyright file="Job.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a job.
  /// </summary>
  [XmlType("job")]  
  public class Job
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Job"/> class.
    /// </summary>
    public Job() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the job.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the position of the job.
    /// </summary>
    [XmlElement("position")]
    public Position Position
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the company of the job.
    /// </summary>
    [XmlElement("company")]
    public Company Company
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the job poster of the job.
    /// </summary>
    [XmlElement("job-poster")]
    public Person JobPoster
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the site request of the job.
    /// </summary>
    [XmlElement("site-job-request")]
    public SiteRequest SiteJobRequest
    {
      get;
      set;
    }
    #endregion
  }
}
