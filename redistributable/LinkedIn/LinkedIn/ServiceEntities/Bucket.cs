//-----------------------------------------------------------------------
// <copyright file="Bucket.cs" company="Beemway">
//     Copyright (c) Beemway. All rights reserved.
// </copyright>
// <license>
//     Microsoft Public License (Ms-PL http://opensource.org/licenses/ms-pl.html).
//     Contributors may add their own copyright notice above.
// </license>
//-----------------------------------------------------------------------

using System;
using System.Xml.Serialization;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a bucket.
  /// </summary>
  [XmlType("bucket")]  
  public class Bucket
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Bucket"/> class.
    /// </summary>
    public Bucket() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the name of the bucket.
    /// </summary>
    [XmlElement("name")]
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the code of the bucket.
    /// </summary>
    [XmlElement("code")]
    public string Code
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the number of items in this bucket.
    /// </summary>
    [XmlElement("count")]
    public int Count
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets whether the bucket was selected.
    /// </summary>
    [XmlElement("selected")]
    public string Selected
    {
      get;
      set;
    }
    #endregion
  }
}
