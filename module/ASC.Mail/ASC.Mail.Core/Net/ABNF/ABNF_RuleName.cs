/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
    /// This class represent ABNF "rulename". Defined in RFC 5234 4.
    /// </summary>
    public class ABNF_RuleName : ABNF_Element
    {
        #region Members

        private readonly string m_RuleName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets rule name.
        /// </summary>
        public string RuleName
        {
            get { return m_RuleName; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="ruleName">Rule name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>ruleName</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public ABNF_RuleName(string ruleName)
        {
            if (ruleName == null)
            {
                throw new ArgumentNullException("ruleName");
            }
            if (!ValidateName(ruleName))
            {
                throw new ArgumentException(
                    "Invalid argument 'ruleName' value. Value must be 'rulename =  ALPHA *(ALPHA / DIGIT / \"-\")'.");
            }

            m_RuleName = ruleName;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ABNF_RuleName Parse(StringReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            // RFC 5234 4.
            //  rulename =  ALPHA *(ALPHA / DIGIT / "-")

            if (!char.IsLetter((char) reader.Peek()))
            {
                throw new ParseException("Invalid ABNF 'rulename' value '" + reader.ReadToEnd() + "'.");
            }

            StringBuilder ruleName = new StringBuilder();

            while (true)
            {
                // We reached end of string.
                if (reader.Peek() == -1)
                {
                    break;
                }
                    // We have valid rule name char.
                else if (char.IsLetter((char) reader.Peek()) | char.IsDigit((char) reader.Peek()) |
                         (char) reader.Peek() == '-')
                {
                    ruleName.Append((char) reader.Read());
                }
                    // Not rule name char, probably readed name.
                else
                {
                    break;
                }
            }

            return new ABNF_RuleName(ruleName.ToString());
        }

        #endregion

        #region Utility methods

        /// <summary>
        /// Validates 'rulename' value.
        /// </summary>
        /// <param name="name">Rule name.</param>
        /// <returns>Returns true if rule name is valid, otherwise false.</returns>
        private bool ValidateName(string name)
        {
            if (name == null)
            {
                return false;
            }
            if (name == string.Empty)
            {
                return false;
            }

            // RFC 5234 4.
            //  rulename =  ALPHA *(ALPHA / DIGIT / "-")

            if (!char.IsLetter(name[0]))
            {
                return false;
            }
            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!(char.IsLetter(c) | char.IsDigit(c) | c == '-'))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}