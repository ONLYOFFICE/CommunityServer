//-----------------------------------------------------------------------
// <copyright file="Attribution.cs" company="Beemway">
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

using LinkedIn.Utility;

namespace LinkedIn.ServiceEntities
{
  /// <summary>
  /// Represents a attribution.
  /// </summary>
  [XmlRoot("attribution")]
  public class Attribution : IXmlSerializable
  {
    #region Private members
    private readonly string RootElementName = "attribution";
    private readonly string ShareElementName = "share";
    private readonly string ShareIdElementName = "id";
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Attribution"/> class.
    /// </summary>
    public Attribution()
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the share.
    /// </summary>
    [XmlElement("share")]
    public Share Share
    {
      get;
      set;
    }
    #endregion

    #region Methods
    public void WriteContractBody(System.Xml.XmlWriter writer, bool isRoot)
    {
      if (isRoot == false)
      {
        writer.WriteStartElement(RootElementName);
      }

      writer.WriteStartElement(ShareElementName);
      writer.WriteElementString(ShareIdElementName, this.Share.Id);
      writer.WriteEndElement(); // share

      if (isRoot == false)
      {
        writer.WriteEndElement();
      }
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
      WriteContractBody(writer, true);
    }
    #endregion
  }
}
