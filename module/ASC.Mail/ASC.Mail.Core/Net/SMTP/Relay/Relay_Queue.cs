/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

namespace ASC.Mail.Net.SMTP.Relay
{
    #region usings

    using System;
    using System.Collections.Generic;
    using System.IO;

    #endregion

    /// <summary>
    /// This class implements SMTP relay queue.
    /// </summary>
    public class Relay_Queue : IDisposable
    {
        #region Members

        private readonly string m_Name = "";
        private readonly Queue<Relay_QueueItem> m_pQueue;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="name">Relay queue name.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>name</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public Relay_Queue(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name == "")
            {
                throw new ArgumentException("Argument 'name' value may not be empty.");
            }

            m_Name = name;
            m_pQueue = new Queue<Relay_QueueItem>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets number of queued items in queue.
        /// </summary>
        public int Count
        {
            get { return m_pQueue.Count; }
        }

        /// <summary>
        /// Gets queue name.
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cleans up any resources being used.
        /// </summary>
        public void Dispose() {}

        /// <summary>
        /// Queues message for relay.
        /// </summary>
        /// <param name="from">Sender address.</param>
        /// <param name="to">Target recipient address.</param>
        /// <param name="messageID">Message ID.</param>
        /// <param name="message">Raw mime message. Message reading starts from current position.</param>
        /// <param name="tag">User data.</param>
        /// <exception cref="ArgumentNullException">Is raised when <b>to</b>,<b>to</b>,<b>messageID</b> or <b>message</b> is null.</exception>
        /// <exception cref="ArgumentException">Is raised when any of the arguments has invalid value.</exception>
        public void QueueMessage(string from, string to, string messageID, Stream message, object tag)
        {
            if (messageID == null)
            {
                throw new ArgumentNullException("messageID");
            }
            if (messageID == "")
            {
                throw new ArgumentException("Argument 'messageID' value must be specified.");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            lock (m_pQueue)
            {
                m_pQueue.Enqueue(new Relay_QueueItem(this, from, to, messageID, message, tag));
            }
        }

        /// <summary>
        /// Dequeues message from queue. If there are no messages, this method returns null.
        /// </summary>
        /// <returns>Returns queued relay message or null if no messages.</returns>
        public Relay_QueueItem DequeueMessage()
        {
            lock (m_pQueue)
            {
                if (m_pQueue.Count > 0)
                {
                    return m_pQueue.Dequeue();
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}