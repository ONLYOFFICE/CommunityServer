//-----------------------------------------------------------------------
// <copyright file="UpdateContent.cs" company="Beemway">
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
  /// Represents a update content.
  /// </summary>
  [XmlType("update-content")]  
  public class UpdateContent
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateContent"/> class.
    /// </summary>
    public UpdateContent()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the <see cref="Person" /> object of the content.
    /// </summary>
    [XmlElement("person")]
    public Person Person
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Question" /> object of the content.
    /// </summary>
    [XmlElement("question")]
    public Question Question
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Job" /> object of the content.
    /// </summary>
    [XmlElement("job")]
    public Job Job
    {
      get;
      set;
    }
    #endregion
  }
}
