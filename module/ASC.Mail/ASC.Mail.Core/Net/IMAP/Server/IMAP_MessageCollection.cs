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

namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// IMAP messages info collection.
    /// </summary>
    public class IMAP_MessageCollection : IEnumerable,IDisposable
    {
        #region Members

        private readonly SortedList<long, IMAP_Message> m_pMessages;

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of messages in the collection.
        /// </summary>
        public int Count
        {
            get { return m_pMessages.Count; }
        }

        /// <summary>
        /// Gets a IMAP_Message object in the collection by index number.
        /// </summary>
        /// <param name="index">An Int32 value that specifies the position of the IMAP_Message object in the IMAP_MessageCollection collection.</param>
        public IMAP_Message this[int index]
        {
            get { return m_pMessages.Values[index]; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public IMAP_MessageCollection()
        {
            m_pMessages = new SortedList<long, IMAP_Message>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds new message info to the collection.
        /// </summary>
        /// <param name="id">Message ID.</param>
        /// <param name="uid">Message IMAP UID value.</param>
        /// <param name="internalDate">Message store date.</param>
        /// <param name="size">Message size in bytes.</param>
        /// <param name="flags">Message flags.</param>
        /// <returns>Returns added IMAp message info.</returns>
        public IMAP_Message Add(string id, long uid, DateTime internalDate, long size, IMAP_MessageFlags flags)
        {
            if (uid < 1)
            {
                throw new ArgumentException("Message UID value must be > 0 !");
            }

            IMAP_Message message = new IMAP_Message(this, id, uid, internalDate, size, flags);
            m_pMessages.Add(uid, message);

            return message;
        }

        /// <summary>
        /// Removes specified IMAP message from the collection.
        /// </summary>
        /// <param name="message">IMAP message to remove.</param>
        public void Remove(IMAP_Message message)
        {
            m_pMessages.Remove(message.UID);
        }

        /// <summary>
        /// Gets collection contains specified message with specified UID.
        /// </summary>
        /// <param name="uid">Message UID.</param>
        /// <returns></returns>
        public bool ContainsUID(long uid)
        {
            return m_pMessages.ContainsKey(uid);
        }

        /// <summary>
        /// Gets index of specified message in the collection.
        /// </summary>
        /// <param name="message">Message indesx to get.</param>
        /// <returns>Returns index of specified message in the collection or -1 if message doesn't belong to this collection.</returns>
        public int IndexOf(IMAP_Message message)
        {
            return m_pMessages.IndexOfKey(message.UID);
        }

        /// <summary>
        /// Removes all messages from the collection.
        /// </summary>
        public void Clear()
        {
            m_pMessages.Clear();
        }

        /// <summary>
        /// Gets messages which has specified flags set.
        /// </summary>
        /// <param name="flags">Flags to match.</param>
        /// <returns></returns>
        public IMAP_Message[] GetWithFlags(IMAP_MessageFlags flags)
        {
            List<IMAP_Message> retVal = new List<IMAP_Message>();
            foreach (IMAP_Message message in m_pMessages.Values)
            {
                if ((message.Flags & flags) != 0)
                {
                    retVal.Add(message);
                }
            }
            return retVal.ToArray();
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pMessages.Values.GetEnumerator();
        }

        private bool isDisposed = false;

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                if (m_pMessages!=null)
                {
                    foreach (var imapMessage in m_pMessages)
                    {
                        imapMessage.Value.Dispose();
                    }
                }
            }
        }

        #endregion
    }
}