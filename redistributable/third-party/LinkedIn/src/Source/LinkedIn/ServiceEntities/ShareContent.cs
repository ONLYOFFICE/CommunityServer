//-----------------------------------------------------------------------
// <copyright file="ShareContent.cs" company="Beemway">
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
  /// Represents the content of a share.
  /// </summary>
  [XmlRoot("content")]
  public class ShareContent : IXmlSerializable
  {
    #region Private members
    private const string RootElementName = "content";
    private const string IdElementName = "id";
    private const string TitleElementName = "title";
    private const string DescriptionElementName = "description";
    private const string SubmittedUrlElementName = "submitted-url";
    private const string ShortenedUrlElementName = "shortened-url";
    private const string ResolvedUrlElementName = "resolved-url";
    private const string SubmittedImageUrlElementName = "submitted-image-url";
    private const string ThumbnailUrlElementName = "thumbnail-url";
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="ShareContent"/> class.
    /// </summary>
    public ShareContent() 
    {
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets or sets the identifier of the content.
    /// </summary>
    [XmlElement("id")]
    public string Id
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the title of the content.
    /// </summary>
    [XmlElement("title")]
    public string Title
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the description of the content.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the submitted url.
    /// </summary>
    [XmlElement("submitted-url")]
    public string SubmittedUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the short version of the submitted URL. If the submitted URL is generated via a URL shortener, 
    /// this is the original URL and the submitted URL is the expanded version. Otherwise, this is a LinkedIn 
    /// generated short URL using http://lnkd.in/.
    /// </summary>
    [XmlElement("shortened-url")]
    public string ShortenedUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the submitted URL unwound from any URL shortener services.
    /// </summary>
    [XmlElement("resolved-url")]
    public string ResolvedUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the summited image url.
    /// </summary>
    [XmlElement("submitted-image-url")]
    public string SubmittedImageUrl
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the thumbnail of the summited image url.
    /// </summary>
    [XmlElement("thumbnail-url")]
    public string ThumbnailUrl
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

      if (string.IsNullOrEmpty(this.Title) == false)
      {
        writer.WriteElementString(TitleElementName, this.Title);
      }

      if (string.IsNullOrEmpty(this.Description) == false)
      {
        writer.WriteElementString(DescriptionElementName, this.Description);
      }

      if (string.IsNullOrEmpty(this.SubmittedUrl) == false)
      {
        writer.WriteElementString(SubmittedUrlElementName, this.SubmittedUrl);
      }

      if (string.IsNullOrEmpty(this.ShortenedUrl) == false)
      {
        writer.WriteElementString(ShortenedUrlElementName, this.ShortenedUrl);
      }

      if (string.IsNullOrEmpty(this.ResolvedUrl) == false)
      {
        writer.WriteElementString(ResolvedUrlElementName, this.ResolvedUrl);
      }

      if (string.IsNullOrEmpty(this.SubmittedImageUrl) == false)
      {
        writer.WriteElementString(SubmittedImageUrlElementName, this.SubmittedImageUrl);
      }

      if (string.IsNullOrEmpty(this.ThumbnailUrl) == false)
      {
        writer.WriteElementString(ThumbnailUrlElementName, this.ThumbnailUrl);
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
      while (reader.Read())
      {
        if (reader.IsStartElement())
        {
          switch (reader.Name)
          {
            case IdElementName:
              this.Id = reader.ReadString();
              break;
            case TitleElementName:
              this.Title = reader.ReadString();
              break;
            case DescriptionElementName:
              this.Description = reader.ReadString();
              break;
            case SubmittedUrlElementName:
              this.SubmittedUrl = reader.ReadString();
              break;
            case ShortenedUrlElementName:
              this.ShortenedUrl = reader.ReadString();
              break;
            case ResolvedUrlElementName:
              this.ResolvedUrl = reader.ReadString();
              break;
            case SubmittedImageUrlElementName:
              this.SubmittedImageUrl = reader.ReadString();
              break;
            case ThumbnailUrlElementName:
              this.ThumbnailUrl = reader.ReadString();
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
