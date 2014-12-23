/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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