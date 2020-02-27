/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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