//-----------------------------------------------------------------------
// <copyright file="Activity.cs" company="Beemway">
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
  /// Represents an activity.
  /// </summary>
  [XmlRoot("activity")]
  public class Activity : IXmlSerializable
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Activity"/> class.
    /// </summary>
    public Activity() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the culture name, indicating the language of this activity.
    /// </summary>
    public string CultureName
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the body of the activity.
    /// </summary>
    public string Body
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the application identifier of the activity.
    /// </summary>
    public string AppId
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
      // Read the opening tag of the encapsulating element
      reader.ReadStartElement();

      reader.ReadStartElement("body");
      this.Body = reader.ReadString();
      reader.ReadEndElement();

      reader.ReadStartElement("app-id");
      this.AppId = reader.ReadString();
      reader.ReadEndElement();

      // Read the end tag of the encapsulating element
      reader.ReadEndElement();
    }

    /// <summary>
    /// Converts an object into its XML representation. 
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter" /> stream to which the object is serialized.</param>
    public void WriteXml(System.Xml.XmlWriter writer)
    {
      writer.WriteAttributeString("locale", this.CultureName);
      writer.WriteElementString("content-type", Constants.LinkedInHtmlContentType);
      writer.WriteElementString("body", this.Body);
    }
    #endregion
  }
}
