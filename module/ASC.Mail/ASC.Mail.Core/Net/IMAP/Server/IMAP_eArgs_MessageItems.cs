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


namespace ASC.Mail.Net.IMAP.Server
{
    #region usings

    using System;
    using System.IO;

    #endregion

    /// <summary>
    /// Provides data to event GetMessageItems.
    /// </summary>
    public class IMAP_eArgs_MessageItems
    {
        #region Members

        private readonly IMAP_MessageItems_enum m_MessageItems = IMAP_MessageItems_enum.Message;
        private readonly IMAP_Message m_pMessageInfo;
        private readonly IMAP_Session m_pSession;
        private string m_BodyStructure;
        private bool m_CloseMessageStream = true;
        private string m_Envelope;
        private byte[] m_Header;
        private bool m_MessageExists = true;
        private long m_MessageStartOffset;
        private Stream m_MessageStream;

        #endregion

        #region Properties

        /// <summary>
        /// Gets reference to current IMAP session.
        /// </summary>
        public IMAP_Session Session
        {
            get { return m_pSession; }
        }

        /// <summary>
        /// Gets message info what message items to get.
        /// </summary>
        public IMAP_Message MessageInfo
        {
            get { return m_pMessageInfo; }
        }

        /// <summary>
        /// Gets what message items must be filled.
        /// </summary>
        public IMAP_MessageItems_enum MessageItems
        {
            get { return m_MessageItems; }
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
        /// Fill this property only if IMAP_MessageItems_enum.Message flag is specified.
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
        /// Gets or sets message main header.
        /// Fill this property only if IMAP_MessageItems_enum.Header flag is specified.
        /// </summary>
        public byte[] Header
        {
            get { return m_Header; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Property Header value can't be null !");
                }

                m_Header = value;
            }
        }

        /// <summary>
        /// Gets or sets IMAP ENVELOPE string.
        /// Fill this property only if IMAP_MessageItems_enum.Envelope flag is specified.
        /// </summary>
        public string Envelope
        {
            get { return m_Envelope; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Property Envelope value can't be null !");
                }

                m_Envelope = value;
            }
        }

        /// <summary>
        /// Gets or sets IMAP BODYSTRUCTURE string.
        /// Fill this property only if IMAP_MessageItems_enum.BodyStructure flag is specified.
        /// </summary>
        public string BodyStructure
        {
            get { return m_BodyStructure; }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Property BodyStructure value can't be null !");
                }

                m_BodyStructure = value;
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
        /// <param name="session">Reference to current IMAP session.</param>
        /// <param name="messageInfo">Message info what message items to get.</param>
        /// <param name="messageItems">Specifies message items what must be filled.</param>
        public IMAP_eArgs_MessageItems(IMAP_Session session,
                                       IMAP_Message messageInfo,
                                       IMAP_MessageItems_enum messageItems)
        {
            m_pSession = session;
            m_pMessageInfo = messageInfo;
            m_MessageItems = messageItems;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            if (m_CloseMessageStream && m_MessageStream != null)
            {
                m_MessageStream.Dispose();
                m_MessageStream = null;
            }
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Default deconstructor.
        /// </summary>
        ~IMAP_eArgs_MessageItems()
        {
            Dispose();
        }

        #endregion

        #region Internal methods

        /// <summary>
        /// Checks that all required data items are provided, if not throws exception.
        /// </summary>
        internal void Validate()
        {
            if ((m_MessageItems & IMAP_MessageItems_enum.BodyStructure) != 0 && m_BodyStructure == null)
            {
                throw new Exception(
                    "IMAP BODYSTRUCTURE is required, but not provided to IMAP server component !");
            }
            if ((m_MessageItems & IMAP_MessageItems_enum.Envelope) != 0 && m_Envelope == null)
            {
                throw new Exception("IMAP ENVELOPE is required, but not provided to IMAP server component  !");
            }
            if ((m_MessageItems & IMAP_MessageItems_enum.Header) != 0 && m_Header == null)
            {
                throw new Exception("Message header is required, but not provided to IMAP server component  !");
            }
            if ((m_MessageItems & IMAP_MessageItems_enum.Message) != 0 && m_MessageStream == null)
            {
                throw new Exception("Full message is required, but not provided to IMAP server component  !");
            }
        }

        #endregion
    }
}