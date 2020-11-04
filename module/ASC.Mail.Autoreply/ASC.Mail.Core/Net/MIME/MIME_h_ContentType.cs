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


namespace ASC.Mail.Net.MIME
{
    #region usings

    using System;
    using System.Text;

    #endregion

    /// <summary>
    /// Represents "Content-Type:" header. Defined in RFC 2045 5.1.
    /// </summary>
    /// <remarks>
    /// <code>
    /// RFC 2045 5.1.
    /// In the Augmented BNF notation of RFC 822, a Content-Type header field
    /// value is defined as follows:
    ///
    ///     content := "Content-Type" ":" type "/" subtype
    ///                *(";" parameter)
    ///                ; Matching of media type and subtype
    ///                ; is ALWAYS case-insensitive.
    ///
    ///     type := discrete-type / composite-type
    ///
    ///     discrete-type := "text" / "image" / "audio" / "video" / "application" / extension-token
    ///
    ///     composite-type := "message" / "multipart" / extension-token
    ///
    ///     extension-token := ietf-token / x-token
    ///
    ///     ietf-token := (An extension token defined by a standards-track RFC and registered with IANA.)
    ///
    ///     x-token := (The two characters "X-" or "x-" followed, with no intervening white space, by any token)
    ///
    ///     subtype := extension-token / iana-token
    ///
    ///     iana-token := (A publicly-defined extension token. Tokens of this form must be registered with IANA as specified in RFC 2048.)
    ///
    ///     parameter := attribute "=" value
    ///
    ///     attribute := token
    ///                  ; Matching of attributes
    ///                  ; is ALWAYS case-insensitive.
    ///
    ///     value := token / quoted-string
    ///
    ///     token := 1*(any (US-ASCII) CHAR except SPACE, CTLs,or tspecials)
    ///
    ///     tspecials :=  "(" / ")" / "&lt;" / "&gt;" / "@" /
    ///                   "," / ";" / ":" / "\" / "
    ///                   "/" / "[" / "]" / "?" / "="
    ///                   ; Must be in quoted-string,
    ///                   ; to use within parameter values
    /// </code>
    /// </remarks>
    public class MIME_h_ContentType : MIME_h
    {
        #region Members

        private readonly MIME_h_ParameterCollection m_pParameters;
        private bool m_IsModified;
        private string m_ParseValue;
        private string m_SubType = "";
        private string m_Type = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets if this header field is modified since it has loaded.
        /// </summary>
        /// <remarks>All new added header fields has <b>IsModified = true</b>.</remarks>
        /// <exception cref="ObjectDisposedException">Is riased when this class is disposed and this property is accessed.</exception>
        public override bool IsModified
        {
            get { return m_IsModified || m_pParameters.IsModified; }
        }

        /// <summary>
        /// Returns always "Content-Type".
        /// </summary>
        public override string Name
        {
            get { return "Content-Type"; }
        }

        /// <summary>
        /// Gets media type. For example: application,image,text, ... .
        /// </summary>
        /// <remarks>The official list of reggistered types are http://www.iana.org/assignments/media-types .</remarks>
        public string Type
        {
            get { return m_Type; }
        }

        /// <summary>
        /// Gets media sub-type. For example for text/plain, sub-type is 'plain'.
        /// </summary>
        /// <remarks>The official list of reggistered types are http://www.iana.org/assignments/media-types .</remarks>
        public string SubType
        {
            get { return m_SubType; }
        }

        /// <summary>
        /// Gets media type with subtype as Type/SubType. Well known value are in <see cref="MIME_MediaTypes">MIME_MediaTypes</see>. For example: text/plain.
        /// </summary>
        public string TypeWithSubype
        {
            get { return m_Type + "/" + m_SubType; }
        }

        /// <summary>
        /// Gets Content-Type parameters collection.
        /// </summary>
        public MIME_h_ParameterCollection Parameters
        {
            get { return m_pParameters; }
        }

        /// <summary>
        /// Gets or sets Content-Type <b>name</b> parameter value. Value null means not specified.
        /// </summary>
        public string Param_Name
        {
            get { return m_pParameters["name"]; }

            set { m_pParameters["name"] = value; }
        }

        /// <summary>
        /// Gets or sets Content-Type <b>charset</b> parameter value. Value null means not specified.
        /// </summary>
        public string Param_Charset
        {
            get { return m_pParameters["charset"]; }

            set { m_pParameters["charset"] = value; }
        }

        /// <summary>
        /// Gets or sets Content-Type <b>boundary</b> parameter value. Value null means not specified.
        /// </summary>
        public string Param_Boundary
        {
            get { return m_pParameters["boundary"]; }

            set { m_pParameters["boundary"] = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="mediaType">Media type with subtype. For example <b>text/plain</b>.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>mediaType</b> is null reference.</exception>
        public MIME_h_ContentType(string mediaType)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException(mediaType);
            }

            /*string[] type_subtype = mediaType.Split(new[] {'/',}, 2);
            if (type_subtype.Length == 2)
            {
                if (type_subtype[0] == "" || !MIME_Reader.IsToken(type_subtype[0]))
                {
                    throw new ArgumentException("Invalid argument 'mediaType' value '" + mediaType +
                                                "', value must be token.");
                }
                if (type_subtype[1] == "" || !MIME_Reader.IsToken(type_subtype[1]))
                {
                    throw new ArgumentException("Invalid argument 'mediaType' value '" + mediaType +
                                                "', value must be token.");
                }

                m_Type = type_subtype[0];
                m_SubType = type_subtype[1];
            }
            else
            {
                throw new ArgumentException("Invalid argument 'mediaType' value '" + mediaType + "'.");
            }

            m_pParameters = new MIME_h_ParameterCollection(this);
            m_IsModified = true;*/

            MIME_Reader r = new MIME_Reader(mediaType);
            string type = r.Token();
            if (type == null)
            {
                throw new ParseException("Invalid Content-Type: header field value '" + mediaType + "'.");
            }
            m_Type = type;

            if (r.Char(false) != '/')
            {
                throw new ParseException("Invalid Content-Type: header field value '" + mediaType + "'.");
            }

            string subtype = r.Token();
            if (subtype == null)
            {
                //throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
                subtype = "";
            }
            m_SubType = subtype;
            m_pParameters = new MIME_h_ParameterCollection(this);
            m_pParameters.Parse(r);

            m_ParseValue = mediaType;
            m_IsModified = true;

        }

        /// <summary>
        /// Internal parser constructor.
        /// </summary>
        private MIME_h_ContentType()
        {
            m_pParameters = new MIME_h_ParameterCollection(this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses header field from the specified value.
        /// </summary>
        /// <param name="value">Header field value. Header field name must be included. For example: 'Content-Type: text/plain'.</param>
        /// <returns>Returns parsed header field.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ParseException">Is raised when header field parsing errors.</exception>
        public static MIME_h_ContentType Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            MIME_h_ContentType retVal = new MIME_h_ContentType();

            string[] name_value = value.Split(new[] {':'}, 2);
            if (name_value.Length != 2)
            {
                throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
            }

            MIME_Reader r = new MIME_Reader(name_value[1]);
            string type = r.Token();
            if (type == null)
            {
                throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
            }
            retVal.m_Type = type;

            if (r.Char(false) != '/')
            {
                throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
            }

            string subtype = r.Token();
            if (subtype == null)
            {
                //throw new ParseException("Invalid Content-Type: header field value '" + value + "'.");
                subtype = "";
            }
            retVal.m_SubType = subtype;

            retVal.m_pParameters.Parse(r);

            retVal.m_ParseValue = value;
            retVal.m_IsModified = false;

            return retVal;
        }

        /// <summary>
        /// Returns header field as string.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <param name="parmetersCharset">Charset to use to encode 8-bit characters. Value null means parameters not encoded.</param>
        /// <returns>Returns header field as string.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder, Encoding parmetersCharset)
        {
            if (!IsModified)
            {
                return m_ParseValue;
            }
            else
            {
                StringBuilder retVal = new StringBuilder();
                retVal.Append("Content-Type: " + m_Type + "/" + m_SubType);
                retVal.Append(m_pParameters.ToString(parmetersCharset));
                retVal.Append("\r\n");

                return retVal.ToString();
            }
        }

        #endregion
    }
}