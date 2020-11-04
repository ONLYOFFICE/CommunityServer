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