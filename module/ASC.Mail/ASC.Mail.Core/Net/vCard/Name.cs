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

namespace ASC.Mail.Net.Mime.vCard
{
    /// <summary>
    /// vCard name implementation.
    /// </summary>
    public class Name
    {
        #region Members

        private string m_AdditionalNames = "";
        private string m_FirstName = "";
        private string m_HonorificPrefix = "";
        private string m_HonorificSuffix = "";
        private string m_LastName = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="lastName">Last name.</param>
        /// <param name="firstName">First name.</param>
        /// <param name="additionalNames">Comma separated additional names.</param>
        /// <param name="honorificPrefix">Honorific prefix.</param>
        /// <param name="honorificSuffix">Honorific suffix.</param>
        public Name(string lastName,
                    string firstName,
                    string additionalNames,
                    string honorificPrefix,
                    string honorificSuffix)
        {
            m_LastName = lastName;
            m_FirstName = firstName;
            m_AdditionalNames = additionalNames;
            m_HonorificPrefix = honorificPrefix;
            m_HonorificSuffix = honorificSuffix;
        }

        /// <summary>
        /// Internal parse constructor.
        /// </summary>
        internal Name() {}

        #endregion

        #region Properties

        /// <summary>
        /// Gets comma separated additional names.
        /// </summary>
        public string AdditionalNames
        {
            get { return m_AdditionalNames; }
        }

        /// <summary>
        /// Gets first name.
        /// </summary>
        public string FirstName
        {
            get { return m_FirstName; }
        }

        /// <summary>
        /// Gets honorific prefix.
        /// </summary>
        public string HonorificPerfix
        {
            get { return m_HonorificPrefix; }
        }

        /// <summary>
        /// Gets honorific suffix.
        /// </summary>
        public string HonorificSuffix
        {
            get { return m_HonorificSuffix; }
        }

        /// <summary>
        /// Gets last name.
        /// </summary>
        public string LastName
        {
            get { return m_LastName; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts item to vCard N structure string.
        /// </summary>
        /// <returns></returns>
        public string ToValueString()
        {
            return m_LastName + ";" + m_FirstName + ";" + m_AdditionalNames + ";" + m_HonorificPrefix + ";" +
                   m_HonorificSuffix;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses name info from vCard N item.
        /// </summary>
        /// <param name="item">vCard N item.</param>
        internal static Name Parse(Item item)
        {
            string[] items = item.DecodedValue.Split(';');
            Name name = new Name();
            if (items.Length >= 1)
            {
                name.m_LastName = items[0];
            }
            if (items.Length >= 2)
            {
                name.m_FirstName = items[1];
            }
            if (items.Length >= 3)
            {
                name.m_AdditionalNames = items[2];
            }
            if (items.Length >= 4)
            {
                name.m_HonorificPrefix = items[3];
            }
            if (items.Length >= 5)
            {
                name.m_HonorificSuffix = items[4];
            }
            return name;
        }

        #endregion
    }
}