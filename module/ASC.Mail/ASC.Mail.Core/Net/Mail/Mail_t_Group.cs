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

namespace ASC.Mail.Net.Mail
{
    #region usings

    using System.Collections.Generic;
    using System.Text;
    using MIME;

    #endregion

    /// <summary>
    /// Defined in RFC 2822 3.4.
    /// </summary>
    public class Mail_t_Group : Mail_t_Address
    {
        #region Members

        private readonly List<Mail_t_Mailbox> m_pList;
        private string m_DisplayName;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets diplay name. Value null means not specified.
        /// </summary>
        public string DisplayName
        {
            get { return m_DisplayName; }

            set { m_DisplayName = value; }
        }

        /// <summary>
        /// Gets groiup address members collection.
        /// </summary>
        public List<Mail_t_Mailbox> Members
        {
            get { return m_pList; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="displayName">Display name. Value null means not specified.</param>
        public Mail_t_Group(string displayName)
        {
            m_DisplayName = displayName;

            m_pList = new List<Mail_t_Mailbox>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns mailbox as string.
        /// </summary>
        /// <returns>Returns mailbox as string.</returns>
        public override string ToString()
        {
            return ToString(null);
        }

        /// <summary>
        /// Returns address as string value.
        /// </summary>
        /// <param name="wordEncoder">8-bit words ecnoder. Value null means that words are not encoded.</param>
        /// <returns>Returns address as string value.</returns>
        public override string ToString(MIME_Encoding_EncodedWord wordEncoder)
        {
            StringBuilder retVal = new StringBuilder();
            if (string.IsNullOrEmpty(m_DisplayName))
            {
                retVal.Append(":");
            }
            else
            {
                if (MIME_Encoding_EncodedWord.MustEncode(m_DisplayName))
                {
                    retVal.Append(wordEncoder.Encode(m_DisplayName) + ":");
                }
                else
                {
                    retVal.Append(TextUtils.QuoteString(m_DisplayName) + ":");
                }
            }
            for (int i = 0; i < m_pList.Count; i++)
            {
                retVal.Append(m_pList[i].ToString(wordEncoder));
                if (i < (m_pList.Count - 1))
                {
                    retVal.Append(",");
                }
            }
            retVal.Append(";");

            return retVal.ToString();
        }

        #endregion
    }
}