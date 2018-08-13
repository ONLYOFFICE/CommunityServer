/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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