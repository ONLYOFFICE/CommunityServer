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