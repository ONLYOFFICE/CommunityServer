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

namespace ASC.Mail.Net.IMAP.Client
{
    #region usings

    using Server;

    #endregion

    /// <summary>
    /// IMAP ACL entry. Defined in RFC 2086.
    /// </summary>
    public class IMAP_Acl
    {
        #region Members

        private readonly string m_Name = "";
        private readonly IMAP_ACL_Flags m_Rights = IMAP_ACL_Flags.None;

        #endregion

        #region Properties

        /// <summary>
        /// Gets authentication identifier name. Normally this is user or group name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// Gets the rights associated with this ACL entry.
        /// </summary>
        public IMAP_ACL_Flags Rights
        {
            get { return m_Rights; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Authentication identifier name. Normally this is user or group name.</param>
        /// <param name="rights">Rights associated with this ACL entry.</param>
        public IMAP_Acl(string name, IMAP_ACL_Flags rights)
        {
            m_Name = name;
            m_Rights = rights;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Parses ACL entry from IMAP ACL response string.
        /// </summary>
        /// <param name="aclResponseString">IMAP ACL response string.</param>
        /// <returns></returns>
        internal static IMAP_Acl Parse(string aclResponseString)
        {
            string[] args = TextUtils.SplitQuotedString(aclResponseString, ' ', true);
            return new IMAP_Acl(args[1], IMAP_Utils.ACL_From_String(args[2]));
        }

        #endregion
    }
}