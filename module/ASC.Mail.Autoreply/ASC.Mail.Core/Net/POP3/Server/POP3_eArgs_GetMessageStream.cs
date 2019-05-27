/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// Provides data to POP3 server event GetMessageStream.
    /// </summary>
    public class POP3_eArgs_GetMessageStream
    {
        #region Members

        private readonly POP3_Message m_pMessageInfo;
        private readonly POP3_Session m_pSession;
        private bool m_CloseMessageStream = true;
        private bool m_MessageExists = true;
        private long m_MessageStartOffset;
        private Stream m_MessageStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to current POP3 session.
        /// </summary>
        public POP3_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets message info what message stream to get.
        /// </summary>
        public POP3_Message MessageInfo
        {
            get { return m_pMessageInfo; }
        }

        /// <summary>
        /// Gets or sets if message stream is closed automatically if all actions on it are completed.
        /// Default value is true.
        /// </summary>
        public bool CloseMessageStream
        {
            get { return m_CloseMessageStream; }

            set { m_CloseMessageStream = value; }
        }

        /// <summary>
        /// Gets or sets message stream. When setting this property Stream position must be where message begins.
        /// </summary>
        public Stream MessageStream
        {
            get
            {
                if (m_MessageStream != null)
                {
                    m_MessageStream.Position = m_MessageStartOffset;
                }
                return m_MessageStream;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Property MessageStream value can't be null !");
                }
                if (!value.CanSeek)
                {
                    throw new Exception("Stream must support seeking !");
                }

                m_MessageStream = value;
                m_MessageStartOffset = m_MessageStream.Position;
            }
        }

        /// <summary>
        /// Gets message size in bytes.
        /// </summary>
        public long MessageSize
        {
            get
            {
                if (m_MessageStream == null)
                {
                    throw new Exception("You must set MessageStream property first to use this property !");
                }
                else
                {
                    return m_MessageStream.Length - m_MessageStream.Position;
                }
            }
        }

        /// <summary>
        /// Gets or sets if message exists. Set this false, if message actually doesn't exist any more.
        /// </summary>
        public bool MessageExists
        {
            get { return m_MessageExists; }

            set { m_MessageExists = value; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to current POP3 session.</param>
        /// <param name="messageInfo">Message info what message items to get.</param>
        public POP3_eArgs_GetMessageStream(POP3_Session session, POP3_Message messageInfo)
        {
            m_pSession = session;
            m_pMessageInfo = messageInfo;
        }

        #endregion
    }
}