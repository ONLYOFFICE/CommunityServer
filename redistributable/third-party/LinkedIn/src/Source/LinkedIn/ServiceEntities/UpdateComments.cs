//-----------------------------------------------------------------------
// <copyright file="UpdateComments.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a collection of update comments.
  /// </summary>
  [XmlRoot("update-comments")]
  public class UpdateComments : PagedCollection<UpdateComment>
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateComments"/> class.
    /// </summary>
    public UpdateComments()
      : base()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the collection of <see cref="UpdateComment" /> objects representing the update comments.
    /// </summary>
    [XmlElement("update-comment")]
    public Collection<UpdateComment> Items
    {
      get;
      set;
    }
    #endregion
  }
}
