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


namespace ASC.Mail.Net.Mail
{
    #region usings

    using System;
    using System.Text;
    using MIME;

    #endregion

    /// <summary>
    /// This class provides mail message related utility methods.
    /// </summary>
    public class Mail_Utils
    {
        #region Internal methods

        /// <summary>
        /// Reads SMTP "Mailbox" from the specified MIME reader.
        /// </summary>
        /// <param name="reader">MIME reader.</param>
        /// <returns>Returns SMTP "Mailbox" or null if no SMTP mailbox available.</returns>
        /// <exception cref="ArgumentNullException">Is raised when <b>reader</b> is null reference.</exception>
        internal static string SMTP_Mailbox(MIME_Reader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // TODO:

            /* RFC 5321.
                Mailbox        = Local-part "@" ( Domain / address-literal )
                Local-part     = Dot-string / Quoted-string ; MAY be case-sensitive
                Dot-string     = Atom *("."  Atom)
            */

            StringBuilder retVal = new StringBuilder();
            if (reader.Peek(true) == '\"')
            {
                retVal.Append("\"" + reader.QuotedString() + "\"");
            }
            else
            {
                retVal.Append(reader.DotAtom());
            }

            if (reader.Peek(true) != '@')
            {
                return null; ;
            }
            else
            {
                // Eat "@".
                reader.Char(true);

                retVal.Append('@');
                retVal.Append(reader.DotAtom());
            }

            return retVal.ToString();
        }

        #endregion
    }
}