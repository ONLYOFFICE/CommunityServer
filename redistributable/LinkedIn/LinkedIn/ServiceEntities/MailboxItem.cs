//-----------------------------------------------------------------------
// <copyright file="MailboxItem.cs" company="Beemway">
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
  /// Represents a mailbox item.
  /// </summary>
  [XmlRoot("mailbox-item")]
  public class MailboxItem : IXmlSerializable
  {
    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxItem"/> class.
    /// </summary>
    public MailboxItem()
    {
      this.Recipients = new List<Recipient>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MailboxItem"/> class.
    /// </summary>
    /// <param name="recipients">A list of <see cref="Recipient" /> objects.</param>
    public MailboxItem(List<Recipient> recipients) 
    {
      if (recipients == null)
      {
        throw new ArgumentNullException("recipients");
      }

      this.Recipients = recipients;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets a list of <see cref="Recipient" /> objects.
    /// </summary>
    public List<Recipient> Recipients
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the subject of the mailbox item.
    /// </summary>
    public string Subject
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the body of the mailbox item.
    /// </summary>
    public string Body
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the content of the mailbox item.
    /// </summary>
    public Invitation ItemContent
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
      writer.WriteStartElement("recipients");

      foreach (Recipient recipient in this.Recipients)
      {
        writer.WriteStartElement("recipient");
        recipient.WriteXml(writer);
        writer.WriteEndElement(); // recipient
      }

      writer.WriteEndElement(); // recipients
      writer.WriteElementString("subject", this.Subject);
      writer.WriteElementString("body", this.Body);

      if (this.ItemContent != null)
      {
        writer.WriteStartElement("item-content");

        writer.WriteStartElement("invitation-request");
        this.ItemContent.WriteXml(writer);
        writer.WriteEndElement(); // invitation-request

        writer.WriteEndElement(); // item-content
      }
    }
    #endregion
  }
}
