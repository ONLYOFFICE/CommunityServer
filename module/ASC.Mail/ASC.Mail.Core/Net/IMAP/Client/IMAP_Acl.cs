/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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