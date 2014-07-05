//-----------------------------------------------------------------------
// <copyright file="Recipient.cs" company="Beemway">
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
  /// Represents a recipient.
  /// </summary>
  [XmlRoot("recipient")]
  public class Recipient : IXmlSerializable
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Recipient"/> class.
    /// </summary>
    public Recipient() 
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Recipient"/> class.
    /// </summary>
    /// <value>The path to the recipient.</value>
    public Recipient(string path)
    {
      this.Path = path;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the path to the recipient.
    /// </summary>
    public string Path
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
      writer.WriteStartElement("person");

      if (string.IsNullOrEmpty(this.Path) == false)
      {
        writer.WriteAttributeString("path", this.Path);
      }

      writer.WriteEndElement(); // person
    }
    #endregion
  }
}
