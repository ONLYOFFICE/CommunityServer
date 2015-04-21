//-----------------------------------------------------------------------
// <copyright file="Answer.cs" company="Beemway">
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
  /// Represents a answer.
  /// </summary>
  [XmlType("answer")]  
  public class Answer
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Answer"/> class.
    /// </summary>
    public Answer() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the answer.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the web url of the answer.
    /// </summary>
    [XmlElement("web-url")]
    public string WebUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the author of the answer.
    /// </summary>
    [XmlElement("author")]
    public Person Author
    {
      get;
      set;
    }
    #endregion
  }
}
