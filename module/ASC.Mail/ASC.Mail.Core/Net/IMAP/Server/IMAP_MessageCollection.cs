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