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

    #endregion

    /// <summary>
    /// This class represents ABNF "rule". Defined in RFC 5234 2.2.
    /// </summary>
    public class ABNF_Rule
    {
        #region Members

        private readonly string m_Name;
        private readonly ABNF_Alternation m_pElements;

        #endregion

        #region Properties

        /// <summary>
        /// Gets rule name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets rule elements.
        /// </summary>
        public ABNF_Alternation Elements
        {
            get { return m_pElements; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Rule name.</param>
        /// <param name="elements">Alternation elements.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> or <b>elements</b> is null reference.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public ABNF_Rule(string name, ABNF_Alternation elements)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == string.Empty)
            {
                throw new ArgumentException("Argument 'name' value must be specified.");
            }
            if (!ValidateName(name))
            {
                throw new ArgumentException(
                    "Invalid argument 'name' value. Value must be 'rulename =  ALPHA *(ALPHA / DIGIT / \"-\")'.");
            }
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            m_Name = name;
            m_pElements = elements;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ABNF_Rule Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            string[] name_value = value.Split(new[] {'='}, 2);
            if (name_value.Length != 2)
            {
                throw new ParseException("Invalid ABNF rule '" + value + "'.");
            }

            ABNF_Rule retVal = new ABNF_Rule(name_value[0].Trim(),
                                             ABNF_Alternation.Parse(new StringReader(name_value[1])));

            return retVal;
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