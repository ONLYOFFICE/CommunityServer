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

namespace ASC.Mail.Net.POP3.Server
{
    #region usings

    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// POP3 messages info collection.
    /// </summary>
    public class POP3_MessageCollection : IEnumerable
    {
        #region Members

        private readonly List<POP3_Message> m_pMessages;

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
        /// Gets a POP3_Message object in the collection by index number.
        /// </summary>
        /// <param name="index">An Int32 value that specifies the position of the POP3_Message object in the POP3_MessageCollection collection.</param>
        public POP3_Message this[int index]
        {
            get { return m_pMessages[index]; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        public POP3_MessageCollection()
        {
            m_pMessages = new List<POP3_Message>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds new message info to the collection.
        /// </summary>
        /// <param name="id">Message ID.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        public POP3_Message Add(string id, string uid, long size)
        {
            return Add(id, uid, size, null);
        }

        /// <summary>
        /// Adds new message info to the collection.
        /// </summary>
        /// <param name="id">Message ID.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size in bytes.</param>
        /// <param name="tag">Message user data.</param>
        public POP3_Message Add(string id, string uid, long size, object tag)
        {
            POP3_Message message = new POP3_Message(this, id, uid, size);
            m_pMessages.Add(message);
            message.Tag = tag;

            return message;
        }

        /// <summary>
        /// Removes specified message from the collection.
        /// </summary>
        /// <param name="message">Message to remove.</param>
        public void Remove(POP3_Message message)
        {
            m_pMessages.Remove(message);
        }

        /// <summary>
        /// Gets if collection contains message with the specified UID.
        /// </summary>
        /// <param name="uid">Message UID to check.</param>
        /// <returns></returns>
        public bool ContainsUID(string uid)
        {
            foreach (POP3_Message message in m_pMessages)
            {
                if (message.UID.ToLower() == uid.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes all messages from the collection.
        /// </summary>
        public void Clear()
        {
            m_pMessages.Clear();
        }

        /// <summary>
        /// Checks if message exists. NOTE marked for delete messages returns false.
        /// </summary>
        /// <param name="sequenceNo">Message 1 based sequence number.</param>
        /// <returns></returns>
        public bool MessageExists(int sequenceNo)
        {
            if (sequenceNo > 0 && sequenceNo <= m_pMessages.Count)
            {
                if (!m_pMessages[sequenceNo - 1].MarkedForDelete)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets messages total sizes. NOTE messages marked for deletion is excluded.
        /// </summary>
        /// <returns></returns>
        public long GetTotalMessagesSize()
        {
            long totalSize = 0;
            foreach (POP3_Message msg in m_pMessages)
            {
                if (!msg.MarkedForDelete)
                {
                    totalSize += msg.Size;
                }
            }

            return totalSize;
        }

        /// <summary>
        /// Resets deleted flags on all messages.
        /// </summary>
        public void ResetDeletedFlag()
        {
            foreach (POP3_Message msg in m_pMessages)
            {
                msg.MarkedForDelete = false;
            }
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator()
        {
            return m_pMessages.GetEnumerator();
        }

        #endregion
    }
}