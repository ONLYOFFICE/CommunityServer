//-----------------------------------------------------------------------
// <copyright file="Share.cs" company="Beemway">
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
  /// Represents a share.
  /// </summary>
  [XmlRoot("share")]
  public class Share : IXmlSerializable
  {
    #region Private members
    private const string RootElementName = "share";
    private const string IdElementName = "id";
    private const string TimestampElementName = "timestamp";
    private const string CommentElementName = "comment";
    private const string ContentElementName = "content";
    private const string VisibilityElementName = "visibility";
    private const string SourceElementName = "source";
    private const string AuthorElementName = "author";
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Share"/> class.
    /// </summary>
    public Share()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Share"/> class.
    /// </summary>
    /// <param name="comment">The comment of the share.</param>
    /// <param name="title">The title of the share.</param>
    /// <param name="description">The description of the share.</param>
    /// <param name="visibilityCode">The visibility of the share.</param>
    public Share(string comment, string title, string description, VisibilityCode visibilityCode)
    {
      this.Comment = comment;
      if (string.IsNullOrEmpty(title) == false || string.IsNullOrEmpty(description) == false)
      {
        this.Content = new ShareContent 
        {
          Title = title,
          Description = description
        };
      }

      this.Visibility = new Visibility { Code = visibilityCode };
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the share.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the time of posting in milliseconds since epoch.
    /// </summary>
    [XmlElement("timestamp")]
    public long Timestamp
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the comment of the share.
    /// </summary>
    [XmlElement("comment")]
    public string Comment
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the content of the share.
    /// </summary>
    [XmlElement("content")]
    public ShareContent Content
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the visibility of the share.
    /// </summary>
    [XmlElement("visibility")]
    public Visibility Visibility
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the source of the share.
    /// </summary>
    [XmlElement("source")]
    public ShareSource Source
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the author of the original share.
    /// </summary>
    /// <remarks>Only appears when retrieving a reshare, not an original share.</remarks>
    [XmlElement("author")]
    public Person Author
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

      if (string.IsNullOrEmpty(this.Id) == false)
      {
        writer.WriteElementString(IdElementName, this.Id);
      }

      if (this.Timestamp > 0)
      {
        writer.WriteElementString(TimestampElementName, this.Timestamp.ToString("G"));
      }

      if (string.IsNullOrEmpty(this.Comment) == false)
      {
        writer.WriteElementString(CommentElementName, this.Comment);
      }

      if (this.Content != null)
      {
        this.Content.WriteContractBody(writer, false);
      }

      if (this.Visibility != null)
      {
        this.Visibility.WriteContractBody(writer, false);
      }

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
      System.Xml.XmlReader shareReader = reader;
      if (reader.IsStartElement())
      {
        shareReader = reader.ReadSubtree();
      }

      while (shareReader.Read())
      {
        while (shareReader.IsStartElement())
        {
          switch (shareReader.Name)
          {
            case IdElementName:
              this.Id = shareReader.ReadString();
              break;
            case TimestampElementName:
              this.Timestamp = long.Parse(shareReader.ReadString());
              break;
            case CommentElementName:
              this.Comment = shareReader.ReadString();
              break;
            case ContentElementName:
              ShareContent content = new ShareContent();
              content.ReadXml(shareReader.ReadSubtree());
              this.Content = content;
              break;
            case VisibilityElementName:
              Visibility visibility = new Visibility();
              visibility.ReadXml(shareReader);
              this.Visibility = visibility;
              break;
            case SourceElementName:
              this.Source = Utility.Utilities.DeserializeXml<ShareSource>(shareReader.ReadOuterXml());
              break;
            case AuthorElementName:
              this.Author = Utility.Utilities.DeserializeXml<Person>(string.Format("<person>{0}</person>", shareReader.ReadInnerXml()));
              break;
            default:
              shareReader.Read();
              break;
          }
        }
      }

      shareReader.Close();
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
