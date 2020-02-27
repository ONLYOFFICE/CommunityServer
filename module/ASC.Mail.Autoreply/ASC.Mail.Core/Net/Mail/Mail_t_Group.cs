/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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