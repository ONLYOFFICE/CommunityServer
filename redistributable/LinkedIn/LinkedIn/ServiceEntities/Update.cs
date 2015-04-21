//-----------------------------------------------------------------------
// <copyright file="Update.cs" company="Beemway">
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
  /// Represents a update.
  /// </summary>
  [XmlType("update")]  
  public class Update
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Update"/> class.
    /// </summary>
    public Update() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the timestamp of the update.
    /// </summary>
    [XmlElement("timestamp")]
    public long Timestamp
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the key of the update.
    /// </summary>
    [XmlElement("update-key")]
    public string UpdateKey
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the type of the update.
    /// </summary>
    [XmlElement("update-type")]
    public string UpdateType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the content of the update.
    /// </summary>
    [XmlElement("update-content")]
    public UpdateContent UpdateContent
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the update is commentable.
    /// </summary>
    [XmlElement("is-commentable")]
    public bool IsCommentable
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of comment on the update.
    /// </summary>
    [XmlElement("update-comments")]
    public UpdateComments UpdateComments
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the update is likable.
    /// </summary>
    [XmlElement("is-likable")]
    public bool IsLikable
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the update is liked.
    /// </summary>
    [XmlElement("is-liked")]
    public bool IsLiked
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the number of likes of the update.
    /// </summary>
    [XmlElement("num-likes")]
    public int NumberOfLikes
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets a collection of likes the update has had.
    /// </summary>
    [XmlElement("likes")]
    public Likes Likes
    {
      get;
      set;
    }
    #endregion
  }
}
