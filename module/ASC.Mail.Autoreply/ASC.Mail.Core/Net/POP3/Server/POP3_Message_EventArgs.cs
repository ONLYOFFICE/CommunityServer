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


namespace ASC.Mail.Net.POP3.Server
{
    #region usings

    using System.Net.Sockets;

    #endregion

    /// <summary>
    /// Provides data for the GetMailEvent,DeleteMessage,GetTopLines event.
    /// </summary>
    public class POP3_Message_EventArgs
    {
        #region Members

        private readonly int m_Lines;
        private readonly POP3_Message m_pMessage;
        private readonly POP3_Session m_pSession;
        private Socket m_pSocket;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to pop3 session.
        /// </summary>
        public POP3_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets reference to message, which to get.
        /// </summary>
        public POP3_Message Message
        {
            get { return m_pMessage; }
        }

        /// <summary>
        /// ID of message which to retrieve.
        /// </summary>
        public string MessageID
        {
            get { return m_pMessage.ID; }
        }

        /// <summary>
        /// UID of message which to retrieve.
        /// </summary>
        public string MessageUID
        {
            get { return m_pMessage.UID; }
        }

        /*
		/// <summary>
		/// Gets direct access to connected socket.
		/// This is meant for advanced users only.
		/// Just write message to this socket.
		/// NOTE: Message must be period handled and doesn't(MAY NOT) contain message terminator at end.
		/// </summary>
		public Socket ConnectedSocket
		{
			get{ return m_pSocket; }
		}
*/

        /// <summary>
        /// Mail message which is delivered to user. NOTE: may be full message or top lines of message.
        /// </summary>
        public byte[] MessageData { get; set; }

        /// <summary>
        /// Number of lines to get.
        /// </summary>
        public int Lines
        {
            get { return m_Lines; }
        }

        /// <summary>
        /// User Name.
        /// </summary>
        public string UserName
        {
            get { return m_pSession.UserName; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to pop3 session.</param>
        /// <param name="message">Message which to get.</param>
        /// <param name="socket">Connected socket.</param>
        public POP3_Message_EventArgs(POP3_Session session, POP3_Message message, Socket socket)
        {
            m_pSession = session;
            m_pMessage = message;
            m_pSocket = socket;
        }

        /// <summary>
        /// TopLines constructor.
        /// </summary>
        /// <param name="session">Reference to pop3 session.</param>
        /// <param name="message">Message which to get.</param>
        /// <param name="socket">Connected socket.</param>
        /// <param name="nLines">Number of lines to get.</param>
        public POP3_Message_EventArgs(POP3_Session session, POP3_Message message, Socket socket, int nLines)
        {
            m_pSession = session;
            m_pMessage = message;
            m_pSocket = socket;
            m_Lines = nLines;
        }

        #endregion
    }
}