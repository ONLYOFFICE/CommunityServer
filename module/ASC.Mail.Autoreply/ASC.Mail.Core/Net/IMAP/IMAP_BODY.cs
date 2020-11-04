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

    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Mime;

    #endregion

    /// <summary>
    /// IMAP BODYSTRUCTURE. Defined in RFC 3501 7.4.2.
    /// </summary>
    public class IMAP_BODY
    {
        #region Members

        private IMAP_BODY_Entity m_pMainEntity;

        #endregion

        #region Properties

        /// <summary>
        /// Gets main entity.
        /// </summary>
        public IMAP_BODY_Entity MainEntity
        {
            get { return m_pMainEntity; }
        }

        /// <summary>
        /// Gets all entities contained in BODYSTRUCTURE, including child entities.
        /// </summary>
        public IMAP_BODY_Entity[] Entities
        {
            get
            {
                List<IMAP_BODY_Entity> allEntities = new List<IMAP_BODY_Entity>();
                allEntities.Add(m_pMainEntity);
                GetEntities(m_pMainEntity.ChildEntities, allEntities);

                return allEntities.ToArray();
            }
        }

        /// <summary>
        /// Gets attachment entities. Entity is considered as attachmnet if:<p/>
        ///     *) Content-Type: name = "" is specified  (old RFC 822 message)<p/>
        /// </summary>
        public IMAP_BODY_Entity[] Attachmnets
        {
            // Cant use these at moment, we need to use extended BODYSTRUCTURE fo that    
            //     *) Content-Disposition: attachment (RFC 2822 message)<p/>
            //     *) Content-Disposition: filename = "" is specified  (RFC 2822 message)<p/>

            get
            {
                List<IMAP_BODY_Entity> attachments = new List<IMAP_BODY_Entity>();
                IMAP_BODY_Entity[] entities = Entities;
                foreach (IMAP_BODY_Entity entity in entities)
                {
                    if (entity.ContentType_Paramters != null)
                    {
                        foreach (HeaderFieldParameter parameter in entity.ContentType_Paramters)
                        {
                            if (parameter.Name.ToLower() == "name")
                            {
                                attachments.Add(entity);
                                break;
                            }
                        }
                    }
                }

                return attachments.ToArray();
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_BODY()
        {
            m_pMainEntity = new IMAP_BODY_Entity();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructs FETCH BODY and BODYSTRUCTURE response.
        /// </summary>
        /// <param name="mime">Mime message.</param>
        /// <param name="bodystructure">Specifies if to construct BODY or BODYSTRUCTURE.</param>
        /// <returns></returns>
        public static string ConstructBodyStructure(Mime mime, bool bodystructure)
        {
            if (bodystructure)
            {
                return "BODYSTRUCTURE " + ConstructParts(mime.MainEntity, bodystructure);
            }
            else
            {
                return "BODY " + ConstructParts(mime.MainEntity, bodystructure);
            }
        }

        /// <summary>
        /// Parses IMAP BODYSTRUCTURE from body structure string.
        /// </summary>
        /// <param name="bodyStructureString">Body structure string</param>
        public void Parse(string bodyStructureString)
        {
            m_pMainEntity = new IMAP_BODY_Entity();
            m_pMainEntity.Parse(bodyStructureString);
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Constructs specified entity and it's childentities bodystructure string.
        /// </summary>
        /// <param name="entity">Mime entity.</param>
        /// <param name="bodystructure">Specifies if to construct BODY or BODYSTRUCTURE.</param>
        /// <returns></returns>
        private static string ConstructParts(MimeEntity entity, bool bodystructure)
        {
            /* RFC 3501 7.4.2 BODYSTRUCTURE
							  BODY A form of BODYSTRUCTURE without extension data.
			  
				A parenthesized list that describes the [MIME-IMB] body
				structure of a message.  This is computed by the server by
				parsing the [MIME-IMB] header fields, defaulting various fields
				as necessary.

				For example, a simple text message of 48 lines and 2279 octets
				can have a body structure of: ("TEXT" "PLAIN" ("CHARSET"
				"US-ASCII") NIL NIL "7BIT" 2279 48)

				Multiple parts are indicated by parenthesis nesting.  Instead
				of a body type as the first element of the parenthesized list,
				there is a sequence of one or more nested body structures.  The
				second element of the parenthesized list is the multipart
				subtype (mixed, digest, parallel, alternative, etc.).
					
				For example, a two part message consisting of a text and a
				BASE64-encoded text attachment can have a body structure of:
				(("TEXT" "PLAIN" ("CHARSET" "US-ASCII") NIL NIL "7BIT" 1152
				23)("TEXT" "PLAIN" ("CHARSET" "US-ASCII" "NAME" "cc.diff")
				"<960723163407.20117h@cac.washington.edu>" "Compiler diff"
				"BASE64" 4554 73) "MIXED")

				Extension data follows the multipart subtype.  Extension data
				is never returned with the BODY fetch, but can be returned with
				a BODYSTRUCTURE fetch.  Extension data, if present, MUST be in
				the defined order.  The extension data of a multipart body part
				are in the following order:

				body parameter parenthesized list
					A parenthesized list of attribute/value pairs [e.g., ("foo"
					"bar" "baz" "rag") where "bar" is the value of "foo", and
					"rag" is the value of "baz"] as defined in [MIME-IMB].

				body disposition
					A parenthesized list, consisting of a disposition type
					string, followed by a parenthesized list of disposition
					attribute/value pairs as defined in [DISPOSITION].

				body language
					A string or parenthesized list giving the body language
					value as defined in [LANGUAGE-TAGS].

				body location
					A string list giving the body content URI as defined in [LOCATION].

				Any following extension data are not yet defined in this
				version of the protocol.  Such extension data can consist of
				zero or more NILs, strings, numbers, or potentially nested
				parenthesized lists of such data.  Client implementations that
				do a BODYSTRUCTURE fetch MUST be prepared to accept such
				extension data.  Server implementations MUST NOT send such
				extension data until it has been defined by a revision of this
				protocol.

				The basic fields of a non-multipart body part are in the
				following order:

				body type
					A string giving the content media type name as defined in [MIME-IMB].
				
				body subtype
					 A string giving the content subtype name as defined in [MIME-IMB].

				body parameter parenthesized list
					A parenthesized list of attribute/value pairs [e.g., ("foo"
					"bar" "baz" "rag") where "bar" is the value of "foo" and
					"rag" is the value of "baz"] as defined in [MIME-IMB].

				body id
					A string giving the content id as defined in [MIME-IMB].

				body description
					A string giving the content description as defined in [MIME-IMB].

				body encoding
					A string giving the content transfer encoding as defined in	[MIME-IMB].

				body size
					A number giving the size of the body in octets.  Note that
					this size is the size in its transfer encoding and not the
					resulting size after any decoding.

				A body type of type MESSAGE and subtype RFC822 contains,
				immediately after the basic fields, the envelope structure,
				body structure, and size in text lines of the encapsulated
				message.

				A body type of type TEXT contains, immediately after the basic
				fields, the size of the body in text lines.  Note that this
				size is the size in its content transfer encoding and not the
				resulting size after any decoding.

				Extension data follows the basic fields and the type-specific
				fields listed above.  Extension data is never returned with the
				BODY fetch, but can be returned with a BODYSTRUCTURE fetch.
				Extension data, if present, MUST be in the defined order.

				The extension data of a non-multipart body part are in the
				following order:

				body MD5
					A string giving the body MD5 value as defined in [MD5].
					
				body disposition
					A parenthesized list with the same content and function as
					the body disposition for a multipart body part.

				body language
					A string or parenthesized list giving the body language
					value as defined in [LANGUAGE-TAGS].

				body location
					A string list giving the body content URI as defined in [LOCATION].

				Any following extension data are not yet defined in this
				version of the protocol, and would be as described above under
				multipart extension data.
			
			
				// We don't construct extention fields like rfc says:
					Server implementations MUST NOT send such
					extension data until it has been defined by a revision of this
					protocol.
			
										
				contentTypeMainMediaType - Example: 'TEXT'
				contentTypeSubMediaType  - Example: 'PLAIN'
				conentTypeParameters     - Example: '("CHARSET" "iso-8859-1" ...)'
				contentID                - Content-ID: header field value.
				contentDescription       - Content-Description: header field value.
				contentEncoding          - Content-Transfer-Encoding: header field value.
				contentSize              - mimeEntity ENCODED data size
				[envelope]               - NOTE: included only if contentType = "message" !!!
				[contentLines]           - number of ENCODED data lines. NOTE: included only if contentType = "text" !!!
									   			
				// Basic fields for multipart
				(nestedMimeEntries) contentTypeSubMediaType
												
				// Basic fields for non-multipart
				contentTypeMainMediaType contentTypeSubMediaType (conentTypeParameters) contentID contentDescription contentEncoding contentSize [envelope] [contentLine]

			*/

            StringBuilder retVal = new StringBuilder();
            // Multipart message
            if ((entity.ContentType & MediaType_enum.Multipart) != 0)
            {
                retVal.Append("(");

                // Construct child entities.
                foreach (MimeEntity childEntity in entity.ChildEntities)
                {
                    // Construct child entity. This can be multipart or non multipart.
                    retVal.Append(ConstructParts(childEntity, bodystructure));
                }

                // Add contentTypeSubMediaType
                string contentType = entity.ContentTypeString.Split(';')[0];
                if (contentType.Split('/').Length == 2)
                {
                    retVal.Append(" \"" + contentType.Split('/')[1].Replace(";", "") + "\"");
                }
                else
                {
                    retVal.Append(" NIL");
                }

                retVal.Append(")");
            }
                // Single part message
            else
            {
                retVal.Append("(");

                // NOTE: all header fields and parameters must in ENCODED form !!!

                // Add contentTypeMainMediaType
                if (entity.ContentTypeString != null)
                {
                    string contentType = entity.ContentTypeString.Split(';')[0];
                    if (contentType.Split('/').Length == 2)
                    {
                        retVal.Append("\"" + entity.ContentTypeString.Split('/')[0] + "\"");
                    }
                    else
                    {
                        retVal.Append("NIL");
                    }
                }
                else
                {
                    retVal.Append("NIL");
                }

                // contentTypeSubMediaType
                if (entity.ContentTypeString != null)
                {
                    string contentType = entity.ContentTypeString.Split(';')[0];
                    if (contentType.Split('/').Length == 2)
                    {
                        retVal.Append(" \"" + contentType.Split('/')[1].Replace(";", "") + "\"");
                    }
                    else
                    {
                        retVal.Append(" NIL");
                    }
                }
                else
                {
                    retVal.Append(" NIL");
                }

                // conentTypeParameters - Syntax: {("name" SP "value" *(SP "name" SP "value"))}
                if (entity.ContentTypeString != null)
                {
                    ParametizedHeaderField contentTypeParameters =
                        new ParametizedHeaderField(entity.Header.GetFirst("Content-Type:"));
                    if (contentTypeParameters.Parameters.Count > 0)
                    {
                        retVal.Append(" (");

                        bool first = true;
                        foreach (HeaderFieldParameter param in contentTypeParameters.Parameters)
                        {
                            // For first item, don't add SP
                            if (!first)
                            {
                                retVal.Append(" ");
                            }
                            else
                            {
                                // Clear first flag
                                first = false;
                            }

                            retVal.Append("\"" + param.Name + "\" \"" +
                                          MimeUtils.EncodeHeaderField(param.Value) + "\"");
                        }

                        retVal.Append(")");
                    }
                    else
                    {
                        retVal.Append(" NIL");
                    }
                }
                else
                {
                    retVal.Append(" NIL");
                }

                // contentID
                string contentID = entity.ContentID;
                if (contentID != null)
                {
                    retVal.Append(" \"" + MimeUtils.EncodeHeaderField(contentID) + "\"");
                }
                else
                {
                    retVal.Append(" NIL");
                }

                // contentDescription
                string contentDescription = entity.ContentDescription;
                if (contentDescription != null)
                {
                    retVal.Append(" \"" + MimeUtils.EncodeHeaderField(contentDescription) + "\"");
                }
                else
                {
                    retVal.Append(" NIL");
                }

                // contentEncoding
                HeaderField contentEncoding = entity.Header.GetFirst("Content-Transfer-Encoding:");
                if (contentEncoding != null)
                {
                    retVal.Append(" \"" + MimeUtils.EncodeHeaderField(contentEncoding.Value) + "\"");
                }
                else
                {
                    // If not specified, then must be 7bit.
                    retVal.Append(" \"7bit\"");
                }

                // contentSize
                if (entity.DataEncoded != null)
                {
                    retVal.Append(" " + entity.DataEncoded.Length);
                }
                else
                {
                    retVal.Append(" 0");
                }

                // envelope ---> FOR ContentType: message/rfc822 ONLY ###
                if ((entity.ContentType & MediaType_enum.Message_rfc822) != 0)
                {
                    retVal.Append(" " + IMAP_Envelope.ConstructEnvelope(entity));

                    // TODO: BODYSTRUCTURE,LINES
                }

                // contentLines ---> FOR ContentType: text/xxx ONLY ###
                if ((entity.ContentType & MediaType_enum.Text) != 0)
                {
                    if (entity.DataEncoded != null)
                    {
                        long lineCount = 0;
                        StreamLineReader r = new StreamLineReader(new MemoryStream(entity.DataEncoded));
                        byte[] line = r.ReadLine();
                        while (line != null)
                        {
                            lineCount++;

                            line = r.ReadLine();
                        }

                        retVal.Append(" " + lineCount);
                    }
                    else
                    {
                        retVal.Append(" 0");
                    }
                }

                retVal.Append(")");
            }

            return retVal.ToString();
        }

        /// <summary>
        /// Gets mime entities, including nested entries. 
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="allEntries"></param>
        private void GetEntities(IMAP_BODY_Entity[] entities, List<IMAP_BODY_Entity> allEntries)
        {
            if (entities != null)
            {
                foreach (IMAP_BODY_Entity ent in entities)
                {
                    allEntries.Add(ent);

                    // Add child entities, if any
                    if (ent.ChildEntities.Length > 0)
                    {
                        GetEntities(ent.ChildEntities, allEntries);
                    }
                }
            }
        }

        #endregion
    }
}