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
    /// <summary>
    /// Provides data for the GetMessgesList event.
    /// </summary>
    public class GetMessagesInfo_EventArgs
    {
        #region Members

        private readonly POP3_MessageCollection m_pPOP3_Messages;
        private readonly POP3_Session m_pSession;
        private readonly string m_UserName = "";

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
        /// Gets referance to POP3 messages info.
        /// </summary>
        public POP3_MessageCollection Messages
        {
            get { return m_pPOP3_Messages; }
        }

        /// <summary>
        /// User Name.
        /// </summary>
        public string UserName
        {
            get { return m_UserName; }
        }

        /// <summary>
        /// Mailbox name.
        /// </summary>
        public string Mailbox
        {
            get { return m_UserName; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to pop3 session.</param>
        /// <param name="messages"></param>
        /// <param name="mailbox">Mailbox name.</param>
        public GetMessagesInfo_EventArgs(POP3_Session session, POP3_MessageCollection messages, string mailbox)
        {
            m_pSession = session;
            m_pPOP3_Messages = messages;
            m_UserName = mailbox;
        }

        #endregion
    }
}