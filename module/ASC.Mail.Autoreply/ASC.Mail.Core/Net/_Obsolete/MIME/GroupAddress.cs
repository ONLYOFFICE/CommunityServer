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


namespace ASC.Mail.Net.Mime
{
    #region usings

    using System;

    #endregion

    /// <summary>
    /// RFC 2822 3.4. (Address Specification) Group address.
    /// <p/>
    /// Syntax: display-name':'[mailbox *(',' mailbox)]';'
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class GroupAddress : Address
    {
        #region Members

        private readonly MailboxAddressCollection m_pGroupMembers;
        private string m_DisplayName = "";

        #endregion

        #region Properties

        /// <summary>
        /// Gets Group as RFC 2822(3.4. Address Specification) string. Syntax: display-name':'[mailbox *(',' mailbox)]';'
        /// </summary>
        public string GroupString
        {
            get { return TextUtils.QuoteString(DisplayName) + ":" + GroupMembers.ToMailboxListString() + ";"; }
        }

        /// <summary>
        /// Gets or sets display name.
        /// </summary>
        public string DisplayName
        {
            get { return m_DisplayName; }

            set
            {
                m_DisplayName = value;

                OnChanged();
            }
        }

        /// <summary>
        /// Gets group members collection.
        /// </summary>
        public MailboxAddressCollection GroupMembers
        {
            get { return m_pGroupMembers; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GroupAddress() : base(true)
        {
            m_pGroupMembers = new MailboxAddressCollection();
            m_pGroupMembers.Owner = this;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Parses Rfc 2822 3.4 group address from group address string. Syntax: display-name':'[mailbox *(',' mailbox)]';'
        /// </summary>
        /// <param name="group">Group address string.</param>
        /// <returns></returns>
        public static GroupAddress Parse(string group)
        {
            GroupAddress g = new GroupAddress();

            // Syntax: display-name':'[mailbox *(',' mailbox)]';'
            string[] parts = TextUtils.SplitQuotedString(group, ':');
            if (parts.Length > -1)
            {
                g.DisplayName = TextUtils.UnQuoteString(parts[0]);
            }
            if (parts.Length > 1)
            {
                g.GroupMembers.Parse(parts[1]);
            }

            return g;
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// This called when group address has changed.
        /// </summary>
        internal void OnChanged()
        {
            if (Owner != null)
            {
                if (Owner is AddressList)
                {
                    ((AddressList) Owner).OnCollectionChanged();
                }
            }
        }

        #endregion
    }
}