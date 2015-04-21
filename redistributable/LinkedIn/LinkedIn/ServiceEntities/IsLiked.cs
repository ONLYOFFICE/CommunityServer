//-----------------------------------------------------------------------
// <copyright file="IsLiked.cs" company="Beemway">
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
  /// Represents a like.
  /// </summary>
  [XmlRoot("is-liked")]
  public class IsLiked : IXmlSerializable
  {
    #region Private members
    private const string RootElementName = "is-liked";
    private const string IsLikedElementName = "is-liked";
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="IsLiked"/> class.
    /// </summary>
    public IsLiked()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets a value indicating whether an network update is liked or not.
    /// </summary>
    [XmlElement("is-liked")]
    public bool IsNetworkUpdateLiked
    {
      get;
      set;
    }
    #endregion

    #region IXmlSerializable Members
    /// <summary>
    /// This method is reserved and should not be used.
    /// </summary>
    /// <returns>This should always returns <b>null</b> (<b>Nothing</b> in Visual Basic).</returns>
    public System.Xml.Schema.XmlSchema GetSchema()
    {
      return null;
    }

    /// <summary>
    /// Generates an object from its XML representation.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader" /> stream from which the object is deserialized.</param>
    public void ReadXml(System.Xml.XmlReader reader)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Converts an object into its XML representation. 
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter" /> stream to which the object is serialized.</param>
    public void WriteXml(System.Xml.XmlWriter writer)
    {
      writer.WriteValue(this.IsNetworkUpdateLiked.ToString().ToLowerInvariant());
    }
    #endregion
  }
}
