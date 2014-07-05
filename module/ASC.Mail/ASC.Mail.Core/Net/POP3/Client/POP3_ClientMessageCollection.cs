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