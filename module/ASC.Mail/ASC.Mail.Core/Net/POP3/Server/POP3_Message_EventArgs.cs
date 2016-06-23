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