//-----------------------------------------------------------------------
// <copyright file="Question.cs" company="Beemway">
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
  /// Represents a question.
  /// </summary>
  [XmlType("question")]  
  public class Question
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Question"/> class.
    /// </summary>
    public Question() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the question.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the name of the question.
    /// </summary>
    [XmlElement("title")]
    public string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the author of the question.
    /// </summary>
    [XmlElement("author")]
    public Person Author
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the collection of <see cref="QuestionCategory" /> objects representing the question categories.
    /// </summary>
    [XmlArray("question-categories")]
    [XmlArrayItem("question-category")]
    public Collection<QuestionCategory> QuestionCategories
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the collection of <see cref="Answer" /> objects representing the answers.
    /// </summary>
    [XmlArray("answers")]
    [XmlArrayItem("answer")]
    public Collection<Answer> Answers
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the web url of the question.
    /// </summary>
    [XmlElement("web-url")]
    public string WebUrl
    {
      get;
      set;
    }
    #endregion
  }
}
