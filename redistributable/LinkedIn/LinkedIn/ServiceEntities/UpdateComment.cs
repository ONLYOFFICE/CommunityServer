//-----------------------------------------------------------------------
// <copyright file="UpdateComment.cs" company="Beemway">
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
  /// Represents a update comment.
  /// </summary>
  [XmlRoot("update-comment")]
  public class UpdateComment : IXmlSerializable
  {
    #region Private members
    private const string RootElementName = "update-comment";
    private const string IdElementName = "id";
    private const string SequenceNumberElementName = "sequence-number";
    private const string CommentElementName = "comment";
    private const string PersonElementName = "person";
    private const string TimestampElementName = "timestamp";
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateComment"/> class.
    /// </summary>
    public UpdateComment()
    {
    }
    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the unique identifier of the comment.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the chronological placement of the comment on the update, starting at 0 for the oldest.
    /// </summary>
    [XmlElement("sequence-number")]
    public int SequenceNumber
    {
      get;
      set;
    }
    
    /// <summary>
    /// Gets or sets the actual comment text.
    /// </summary>
    [XmlElement("comment")]
    public string Comment
    {
      get;
      set;
    }
    
    /// <summary>
    /// Gets or sets the person who commented on a update.
    /// </summary>
    [XmlElement("person")]
    public Person Person
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the time the comment was made (in milliseconds since epoch).
    /// </summary>
    [XmlElement("timestamp")]
    public long Timestamp
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

      writer.WriteElementString(CommentElementName, this.Comment);

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
      int initialDepth = reader.Depth;

      while (reader.Read() && reader.Depth >= initialDepth)
      {
        while (reader.IsStartElement())
        {
          switch (reader.Name)
          {
            case IdElementName:
              this.Id = reader.ReadString();
              break;
            case SequenceNumberElementName:
              this.SequenceNumber = int.Parse(reader.ReadString());
              break;
            case CommentElementName:
              this.Comment = reader.ReadString();
              break;
            case PersonElementName:
              this.Person = Utility.Utilities.DeserializeXml<Person>(string.Format("<person>{0}</person>", reader.ReadInnerXml()));
              break;
            case TimestampElementName:
              this.Timestamp = long.Parse(reader.ReadString());
              break;
            default:
              reader.Read();
              break;
          }
        }
      }
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
