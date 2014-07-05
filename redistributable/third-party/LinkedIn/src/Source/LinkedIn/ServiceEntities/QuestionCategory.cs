//-----------------------------------------------------------------------
// <copyright file="QuestionCategory.cs" company="Beemway">
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
  /// Represents a question category.
  /// </summary>
  [XmlType("question-category")]  
  public class QuestionCategory
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="QuestionCategory"/> class.
    /// </summary>
    public QuestionCategory() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the code of the question category.
    /// </summary>
    [XmlElement("code")]
    public string Code
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the question category.
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
