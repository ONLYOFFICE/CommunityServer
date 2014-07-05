//-----------------------------------------------------------------------
// <copyright file="Recommendation.cs" company="Beemway">
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
  /// Represents a recommendation.
  /// </summary>
  [XmlType("recommendation")]  
  public class Recommendation
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Recommendation"/> class.
    /// </summary>
    public Recommendation()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the recommendation.
    /// </summary>
    [XmlElement("id")]
    public int Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the type of the recommendation.
    /// </summary>
    [XmlElement("recommendation-type")]
    public RecommendationType RecommendationType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the snippet of the recommendation.
    /// </summary>
    [XmlElement("recommendation-snippet")]
    public string Snippet
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the <see cref="Person" /> object representing the recommender.
    /// </summary>
    [XmlElement("recommender")]
    public Person Recommender
    {
      get;
      set;
    }
    #endregion
  }
}
