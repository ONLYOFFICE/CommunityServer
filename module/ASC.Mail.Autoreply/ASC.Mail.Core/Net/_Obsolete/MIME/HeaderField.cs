/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// Mime entity header field.
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class HeaderField
    {
        #region Members

        private string m_Name = "";
        private string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets header field name. Header field name must end with colon(:) and may contain US-ASCII character values between 33 and 126.
        /// </summary>
        public string Name
        {
            get { return m_Name; }

            set
            {
                /* RFC 2822 2.2 Header Fields
					Header fields are lines composed of a field name, followed by a colon
					(":"), followed by a field body, and terminated by CRLF.  A field
					name MUST be composed of printable US-ASCII characters (i.e.,
					characters that have values between 33 and 126, inclusive), except
					colon.  A field body may be composed of any US-ASCII characters,
					except for CR and LF.  However, a field body may contain CRLF when
					used in header "folding" and  "unfolding" as described in section
					2.2.3.  All field bodies MUST conform to the syntax described in
					sections 3 and 4 of this standard.
				*/

                if (value == "")
                {
                    throw new Exception("Header Field name can't be empty !");
                }

                // Colon is missing from name, just add it
                if (!value.EndsWith(":"))
                {
                    value += ":";
                }

                // Check if name is between ASCII 33 - 126 characters
                foreach (char c in value.Substring(0, value.Length - 1))
                {
                    if (c < 33 || c > 126)
                    {
                        throw new Exception("Invalid field name '" + value +
                                            "'. A field name MUST be composed of printable US-ASCII characters (i.e.,characters that have values between 33 and 126, inclusive),except	colon.");
                    }
                }

                m_Name = value;
            }
        }

        /// <summary>
        /// Gets or sets header field value.
        /// </summary>
        public string Value
        {
            get { return MimeUtils.DecodeWords(m_Value); }

            set { m_Value = Core.CanonicalEncode(value, "utf-8"); }
        }

        /// <summary>
        /// Gets header field encoded value.
        /// </summary>
        internal string EncodedValue
        {
            get { return m_Value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default construtor.
        /// </summary>
        public HeaderField() {}

        /// <summary>
        /// Creates new header field with specified name and value.
        /// </summary>
        /// <param name="name">Header field name. Header field name must end with colon(:) and may contain US-ASCII character values between 33 and 126.</param>
        /// <param name="value">Header field value.</param>
        public HeaderField(string name, string value)
        {
            Name = name;
            Value = value;
        }

        #endregion
    }
}