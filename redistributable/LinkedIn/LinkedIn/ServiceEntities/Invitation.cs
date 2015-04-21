//-----------------------------------------------------------------------
// <copyright file="Invitation.cs" company="Beemway">
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
  /// Represents a invitation.
  /// </summary>
  [XmlRoot("invitation-request")]
  public class Invitation : IXmlSerializable
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Invitation"/> class.
    /// </summary>
    public Invitation() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the connection type of the invitation.
    /// </summary>
    public ConnectionType ConnectType
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the authorization headers of the invitation.
    /// </summary>
    public KeyValuePair<string, string> Authorization
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
      writer.WriteElementString("connect-type", EnumHelper.GetDescription(this.ConnectType));
      writer.WriteStartElement("authorization");
      writer.WriteElementString("name", this.Authorization.Key);
      writer.WriteElementString("value", this.Authorization.Value);
      writer.WriteEndElement(); // authorization
    }
    #endregion
  }
}
