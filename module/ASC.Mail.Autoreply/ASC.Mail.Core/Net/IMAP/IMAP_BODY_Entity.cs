/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


namespace ASC.Mail.Net.IMAP
{
    #region usings

    using System;
    using System.Collections.Generic;
    using Mime;

    #endregion

    /// <summary>
    /// IMAP BODY mime entity info.
    /// </summary>
    public class IMAP_BODY_Entity
    {
        #region Members

        private readonly List<IMAP_BODY_Entity> m_pChildEntities;
        private string m_ContentDescription;
        private ContentTransferEncoding_enum m_ContentEncoding = ContentTransferEncoding_enum._7bit;
        private string m_ContentID;
        private int m_ContentLines;
        private int m_ContentSize;
        private MediaType_enum m_pContentType = MediaType_enum.Text_plain;
        private HeaderFieldParameter[] m_pContentTypeParams;
        private IMAP_Envelope m_pEnvelope;
        private IMAP_BODY_Entity m_pParentEntity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets parent entity of this entity. If this entity is top level, then this property returns null.
        /// </summary>
        public IMAP_BODY_Entity ParentEntity
        {
            get { return m_pParentEntity; }
        }

        /// <summary>
        /// Gets child entities. This property is available only if ContentType = multipart/... .
        /// </summary>
        public IMAP_BODY_Entity[] ChildEntities
        {
            get
            {
                //  if((this.ContentType & MediaType_enum.Multipart) == 0){
                //      throw new Exception("NOTE: ChildEntities property is available only for non-multipart contentype !");
                //  }

                return m_pChildEntities.ToArray();
            }
        }

        /// <summary>
        /// Gets header field "<b>Content-Type:</b>" value.
        /// </summary>
        public MediaType_enum ContentType
        {
            get { return m_pContentType; }
        }

        /// <summary>
        /// Gets header field "<b>Content-Type:</b>" prameters. This value is null if no parameters.
        /// </summary>
        public HeaderFieldParameter[] ContentType_Paramters
        {
            get { return m_pContentTypeParams; }
        }

        /// <summary>
        /// Gets header field "<b>Content-ID:</b>" value. Returns null if value isn't set.
        /// </summary>
        public string ContentID
        {
            get { return m_ContentID; }
        }

        /// <summary>
        /// Gets header field "<b>Content-Description:</b>" value. Returns null if value isn't set.
        /// </summary>
        public string ContentDescription
        {
            get { return m_ContentDescription; }
        }

        /// <summary>
        /// Gets header field "<b>Content-Transfer-Encoding:</b>" value.
        /// </summary>
        public ContentTransferEncoding_enum ContentTransferEncoding
        {
            get { return m_ContentEncoding; }
        }

        /// <summary>
        /// Gets content encoded data size. NOTE: This property is available only for non-multipart contentype !
        /// </summary>
        public int ContentSize
        {
            get
            {
                if ((ContentType & MediaType_enum.Multipart) != 0)
                {
                    throw new Exception(
                        "NOTE: ContentSize property is available only for non-multipart contentype !");
                }

                return m_ContentSize;
            }
        }

        /// <summary>
        /// Gets content envelope. NOTE: This property is available only for message/xxx content type !
        /// Yhis value can be also null if no ENVELOPE provided by server.
        /// </summary>
        public IMAP_Envelope Envelope
        {
            get
            {
                if ((ContentType & MediaType_enum.Message) == 0)
                {
                    throw new Exception(
                        "NOTE: Envelope property is available only for non-multipart contentype !");
                }

                return null;
            }
        }

        /// <summary>
        /// Gets content encoded data lines. NOTE: This property is available only for text/xxx content type !
        /// </summary>
        public int ContentLines
        {
            get
            {
                if ((ContentType & MediaType_enum.Text) == 0)
                {
                    throw new Exception(
                        "NOTE: ContentLines property is available only for text/xxx content type !");
                }

                return m_ContentSize;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        internal IMAP_BODY_Entity()
        {
            m_pChildEntities = new List<IMAP_BODY_Entity>();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses entity and it's child entities.
        /// </summary>
        internal void Parse(string text)
        {
            StringReader r = new StringReader(text);
            r.ReadToFirstChar();

            // If starts with ( then multipart/xxx, otherwise normal single part entity
            if (r.StartsWith("("))
            {
                // Entities are (entity1)(entity2)(...) <SP> ContentTypeSubType
                while (r.StartsWith("("))
                {
                    IMAP_BODY_Entity entity = new IMAP_BODY_Entity();
                    entity.Parse(r.ReadParenthesized());
                    entity.m_pParentEntity = this;
                    m_pChildEntities.Add(entity);

                    r.ReadToFirstChar();
                }

                // Read multipart values. (nestedMimeEntries) contentTypeSubMediaType
                string contentTypeSubMediaType = r.ReadWord();

                m_pContentType = MimeUtils.ParseMediaType("multipart/" + contentTypeSubMediaType);
            }
            else
            {
                // Basic fields for non-multipart
                // contentTypeMainMediaType contentTypeSubMediaType (conentTypeParameters) contentID contentDescription contentEncoding contentSize [envelope] [contentLine]

                // Content-Type
                string contentTypeMainMediaType = r.ReadWord();
                string contentTypeSubMediaType = r.ReadWord();
                if (contentTypeMainMediaType.ToUpper() != "NIL" && contentTypeSubMediaType.ToUpper() != "NIL")
                {
                    m_pContentType =
                        MimeUtils.ParseMediaType(contentTypeMainMediaType + "/" + contentTypeSubMediaType);
                }

                // Content-Type header field parameters
                // Parameters syntax: "name" <SP> "value" <SP> "name" <SP> "value" <SP> ... .
                r.ReadToFirstChar();
                string conentTypeParameters = "NIL";
                if (r.StartsWith("("))
                {
                    conentTypeParameters = r.ReadParenthesized();

                    StringReader contentTypeParamReader =
                        new StringReader(MimeUtils.DecodeWords(conentTypeParameters));
                    List<HeaderFieldParameter> parameters = new List<HeaderFieldParameter>();
                    while (contentTypeParamReader.Available > 0)
                    {
                        string parameterName = contentTypeParamReader.ReadWord();
                        string parameterValue = contentTypeParamReader.ReadWord();

                        parameters.Add(new HeaderFieldParameter(parameterName, parameterValue));
                    }
                    m_pContentTypeParams = parameters.ToArray();
                }
                else
                {
                    // Skip NIL
                    r.ReadWord();
                }

                // Content-ID:
                string contentID = r.ReadWord();
                if (contentID.ToUpper() != "NIL")
                {
                    m_ContentID = contentID;
                }

                // Content-Description:
                string contentDescription = r.ReadWord();
                if (contentDescription.ToUpper() != "NIL")
                {
                    m_ContentDescription = contentDescription;
                }

                // Content-Transfer-Encoding:
                string contentEncoding = r.ReadWord();
                if (contentEncoding.ToUpper() != "NIL")
                {
                    m_ContentEncoding = MimeUtils.ParseContentTransferEncoding(contentEncoding);
                }

                // Content Encoded data size in bytes
                string contentSize = r.ReadWord();
                if (contentSize.ToUpper() != "NIL")
                {
                    m_ContentSize = Convert.ToInt32(contentSize);
                }

                // Only for ContentType message/rfc822
                if (ContentType == MediaType_enum.Message_rfc822)
                {
                    r.ReadToFirstChar();

                    // envelope
                    if (r.StartsWith("("))
                    {
                        m_pEnvelope = new IMAP_Envelope();
                        m_pEnvelope.Parse(r.ReadParenthesized());
                    }
                    else
                    {
                        // Skip NIL, ENVELOPE wasn't included
                        r.ReadWord();
                    }

                    // TODO:
                    // BODYSTRUCTURE

                    // contentLine
                }

                // Only for ContentType text/xxx
                if (contentTypeMainMediaType.ToLower() == "text")
                {
                    // contentLine
                    string contentLines = r.ReadWord();
                    if (contentLines.ToUpper() != "NIL")
                    {
                        m_ContentLines = Convert.ToInt32(contentLines);
                    }
                }
            }
        }

        #endregion
    }
}