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