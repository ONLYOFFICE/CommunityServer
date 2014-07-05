/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

namespace ASC.Mail.Net.ABNF
{
    #region usings

    using System;
    using System.IO;
    using System.Text;

    #endregion

    /// <summary>
    /// This class represent ABNF "prose-val". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_ProseVal : ABNF_Element
    {
        #region Members

        private readonly string m_Value = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets value.
        /// </summary>
        public string Value
        {
            get { return m_Value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="value">The prose-val value.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public ABNF_ProseVal(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (!Validate(value))
            {
                // Just <> missing
                // throw new ArgumentException("Invalid argument 'value' value. Value must be: '*(%x20-3D / %x3F-7E)'.");
            }

            m_Value = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABNF_ProseVal Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            /*
                prose-val      =  "<" *(%x20-3D / %x3F-7E) ">"
                                ; bracketed string of SP and VCHAR
                                ;  without angles
                                ; prose description, to be used as
                                ;  last resort

            */

            if (reader.Peek() != '<')
            {
                throw new ParseException("Invalid ABNF 'prose-val' value '" + reader.ReadToEnd() + "'.");
            }

            // Eat "<"
            reader.Read();

            // TODO: *c-wsp

            StringBuilder value = new StringBuilder();

            while (true)
            {
                // We reached end of stream, no closing DQUOTE.
                if (reader.Peek() == -1)
                {
                    throw new ParseException("Invalid ABNF 'prose-val' value '" + reader.ReadToEnd() + "'.");
                }
                    // We have closing ">".
                else if (reader.Peek() == '>')
                {
                    reader.Read();
                    break;
                }
                    // Allowed char.
                else if ((reader.Peek() >= 0x20 && reader.Peek() <= 0x3D) ||
                         (reader.Peek() >= 0x3F && reader.Peek() <= 0x7E))
                {
                    value.Append((char) reader.Read());
                }
                    // Invalid value.
                else
                {
                    throw new ParseException("Invalid ABNF 'prose-val' value '" + reader.ReadToEnd() +
                                             "'.");
                }
            }

            return new ABNF_ProseVal(value.ToString());
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Validates "prose-val" value.
        /// </summary>
        /// <param name="value">The "prose-val" value.</param>
        /// <returns>Returns if value is "prose-val" value, otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>value</b> is null reference.</exception>
        private bool Validate(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // RFC 5234 4.
            //  prose-val =  "<" *(%x20-3D / %x3F-7E) ">"

            if (value.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (i == 0 && c != '<')
                {
                    return false;
                }
                else if (i == (value.Length - 1) && c != '>')
                {
                    return false;
                }
                else if (!((c >= 0x20 && c <= 0x3D) || (c >= 0x3F && c <= 0x7E)))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}