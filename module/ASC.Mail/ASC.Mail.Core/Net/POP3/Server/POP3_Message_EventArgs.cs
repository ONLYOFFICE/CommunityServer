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