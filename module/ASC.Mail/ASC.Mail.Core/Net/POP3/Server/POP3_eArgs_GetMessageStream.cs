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