/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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