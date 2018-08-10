/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// Rfc 2822 3.4 mailbox-list. Syntax: mailbox *(',' mailbox).
    /// </summary>
    [Obsolete("See LumiSoft.Net.MIME or LumiSoft.Net.Mail namepaces for replacement.")]
    public class MailboxAddressCollection : IEnumerable
    {
        #region Members

        private readonly List<MailboxAddress> m_pMailboxes;
        private Address m_pOwner;

        #endregion

        #region Properties

        /// <summary>
        /// Gets mailbox from specified index.
        /// </summary>
        public MailboxAddress this[int index]
        {
            get { return m_pMailboxes[index]; }
        }

        /// <summary>
        /// Gets mailboxes count in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pMailboxes.Count; }
        }

        /// <summary>
        /// Gets or sets owner of this collection.
        /// </summary>
        internal Address Owner
        {
            get { return m_pOwner; }

            set { m_pOwner = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MailboxAddressCollection()
        {
            m_pMailboxes = new List<MailboxAddress>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a new mailbox to the end of the collection.
        /// </summary>
        /// <param name="mailbox">Mailbox to add.</param>
        public void Add(MailboxAddress mailbox)
        {
            m_pMailboxes.Add(mailbox);

            OnCollectionChanged();
        }

        /// <summary>
        /// Inserts a new mailbox into the collection at the specified location.
        /// </summary>
        /// <param name="index">The location in the collection where you want to add the mailbox.</param>
        /// <param name="mailbox">Mailbox to add.</param>
        public void Insert(int index, MailboxAddress mailbox)
        {
            m_pMailboxes.Insert(index, mailbox);

            OnCollectionChanged();
        }

        /// <summary>
        /// Removes header field at the specified index from the collection.
        /// </summary>
        /// <param name="index">Index of the mailbox which to remove.</param>
        public void Remove(int index)
        {
            m_pMailboxes.RemoveAt(index);

            OnCollectionChanged();
        }

        /// <summary>
        /// Removes specified mailbox from the collection.
        /// </summary>
        /// <param name="mailbox">Mailbox to remove.</param>
        public void Remove(MailboxAddress mailbox)
        {
            m_pMailboxes.Remove(mailbox);

            OnCollectionChanged();
        }

        /// <summary>
        /// Clears the collection of all mailboxes.
        /// </summary>
        public void Clear()
        {
            m_pMailboxes.Clear();

            OnCollectionChanged();
        }

        /// <summary>
        /// Parses mailboxes from Rfc 2822 3.4 mailbox-list string. Syntax: mailbox *(',' mailbox).
        /// </summary>
        /// <param name="mailboxList">Mailbox list string.</param>
        public void Parse(string mailboxList)
        {
            // We need to parse right !!! 
            // Can't use standard String.Split() because commas in quoted strings must be skiped.
            // Example: "ivar, test" <ivar@lumisoft.ee>,"xxx" <ivar2@lumisoft.ee>

            string[] mailboxes = TextUtils.SplitQuotedString(mailboxList, ',');
            foreach (string mailbox in mailboxes)
            {
                m_pMailboxes.Add(MailboxAddress.Parse(mailbox));
            }
        }

        /// <summary>
        /// Convert addresses to Rfc 2822 mailbox-list string.
        /// </summary>
        /// <returns></returns>
        public string ToMailboxListString()
        {
            string retVal = "";
            for (int i = 0; i < m_pMailboxes.Count; i++)
            {
                // For last address don't add , and <TAB>
                if (i == (m_pMailboxes.Count - 1))
                {
                    retVal += (m_pMailboxes[i]).ToMailboxAddressString();
                }
                else
                {
                    retVal += (m_pMailboxes[i]).ToMailboxAddressString() + ",\t";
                }
            }

            return retVal;
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pMailboxes.GetEnumerator();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// This called when collection has changed. Item is added,deleted,changed or collection cleared.
        /// </summary>
        internal void OnCollectionChanged()
        {
            if (m_pOwner != null)
            {
                if (m_pOwner is GroupAddress)
                {
                    ((GroupAddress) m_pOwner).OnChanged();
                }
            }
        }

        #endregion
    }
}