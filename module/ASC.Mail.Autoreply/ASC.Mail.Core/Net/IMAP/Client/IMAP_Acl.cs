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