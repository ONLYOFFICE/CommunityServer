/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.POP3.Client
{
    #region usings

    using System;
    using System.Collections;
    using System.Collections.Generic;

    #endregion

    /// <summary>
    /// This class represents POP3 client messages collection.
    /// </summary>
    public class POP3_ClientMessageCollection : IEnumerable, IDisposable
    {
        #region Members

        private readonly POP3_Client m_pPop3Client;
        private bool m_IsDisposed;
        private List<POP3_ClientMessage> m_pMessages;

        #endregion

        #region Properties

        /// <summary>
        /// Gets total size of messages, messages marked for deletion are included.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public long TotalSize
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                long size = 0;
                foreach (POP3_ClientMessage message in m_pMessages)
                {
                    size += message.Size;
                }

                return size;
            }
        }

        /// <summary>
        /// Gets number of messages in the collection, messages marked for deletion are included.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public int Count
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return m_pMessages.Count;
            }
        }

        /// <summary>
        /// Gets message from specified index.
        /// </summary>
        /// <param name="index">Message zero based index in the collection.</param>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Is raised when index is out of range.</exception>
        public POP3_ClientMessage this[int index]
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (index < 0 || index > m_pMessages.Count)
                {
                    throw new ArgumentOutOfRangeException();
                }

                return m_pMessages[index];
            }
        }

        /// <summary>
        /// Gets message with specified UID value.
        /// </summary>
        /// <param name="uid">Message UID value.</param>
        /// <returns>Returns message or null if message doesn't exist.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        /// <exception cref="NotSupportedException">Is raised when POP3 server doesn't support UIDL.</exception>
        public POP3_ClientMessage this[string uidl]
        {
            get
            {
                if (m_IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }
                if (!m_pPop3Client.IsUidlSupported)
                {
                    throw new NotSupportedException();
                }

                foreach (POP3_ClientMessage message in m_pMessages)
                {
                    if (message.UIDL == uidl)
                    {
                        return message;
                    }
                }

                return null;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="pop3">Owner POP3 client.</param>
        internal POP3_ClientMessageCollection(POP3_Client pop3)
        {
            m_pPop3Client = pop3;

            m_pMessages = new List<POP3_ClientMessage>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (m_IsDisposed)
            {
                return;
            }
            m_IsDisposed = true;

            // Release messages.
            foreach (POP3_ClientMessage message in m_pMessages)
            {
                message.Dispose();
            }
            m_pMessages = null;
        }

        /// <summary>
        /// Gets enumerator.
        /// </summary>
        /// <returns>Returns IEnumerator interface.</returns>
        /// <exception cref="ObjectDisposedException">Is raised when this object is disposed and this property is accessed.</exception>
        public IEnumerator GetEnumerator()
        {
            if (m_IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            return m_pMessages.GetEnumerator();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Adds new message to messages collection.
        /// </summary>
        /// <param name="size">Message size in bytes.</param>
        internal void Add(int size)
        {
            m_pMessages.Add(new POP3_ClientMessage(m_pPop3Client, m_pMessages.Count + 1, size));
        }

        #endregion
    }
}