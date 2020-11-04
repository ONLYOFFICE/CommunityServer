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