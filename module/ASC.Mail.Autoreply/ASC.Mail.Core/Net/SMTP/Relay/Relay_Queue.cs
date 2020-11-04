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