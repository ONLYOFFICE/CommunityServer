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


namespace ASC.Mail.Net.IMAP.Client
{
    #region usings

    using System;
    using System.IO;
    using System.Text;
    using Mime;
    using Server;

    #endregion

    /// <summary>
    /// IMAP fetch item.
    /// </summary>
    public class IMAP_FetchItem
    {
        #region Members

        private readonly string m_BodyStructure = "";
        private readonly byte[] m_Data;
        private readonly string m_Envelope = "";
        private readonly IMAP_FetchItem_Flags m_FetchFlags = IMAP_FetchItem_Flags.MessageFlags;
        private readonly IMAP_MessageFlags m_Flags = IMAP_MessageFlags.Recent;
        private readonly string m_InternalDate = "";
        private readonly int m_No;
        private readonly int m_Size;
        private readonly int m_UID;

        #endregion

        #region Properties

        /// <summary>
        /// Specifies what data this IMAP_FetchItem contains. This is flagged value and can contain multiple values.
        /// </summary>
        public IMAP_FetchItem_Flags FetchFlags
        {
            get { return m_FetchFlags; }
        }

        /// <summary>
        /// Gets number of message in folder.
        /// </summary>
        public int MessageNumber
        {
            get { return m_No; }
        }

        /// <summary>
        /// Gets message UID. This property is available only if IMAP_FetchItem_Flags.UID was specified,
        /// otherwise throws exception.
        /// </summary>
        public int UID
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.UID) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.UID wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return m_UID;
            }
        }

        /// <summary>
        /// Gets message size. This property is available only if IMAP_FetchItem_Flags.Size was specified,
        /// otherwise throws exception.
        /// </summary>
        public int Size
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.Size) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.Size wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return m_Size;
            }
        }

        /// <summary>
        /// Gets message IMAP server INTERNAL date. This property is available only if IMAP_FetchItem_Flags.InternalDate was specified,
        /// otherwise throws exception.
        /// </summary>
        public DateTime InternalDate
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.InternalDate) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.InternalDate wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return IMAP_Utils.ParseDate(m_InternalDate);
            }
        }

        /// <summary>
        /// Gets message flags. This property is available only if IMAP_FetchItem_Flags.MessageFlags was specified,
        /// otherwise throws exception.
        /// </summary>
        public IMAP_MessageFlags MessageFlags
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.MessageFlags) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.MessageFlags wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return m_Flags;
            }
        }

        /// <summary>
        /// Gets message IMAP ENVELOPE. This property is available only if IMAP_FetchItem_Flags.Envelope was specified,
        /// otherwise throws exception.
        /// </summary>
        public IMAP_Envelope Envelope
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.Envelope) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.Envelope wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                IMAP_Envelope envelope = new IMAP_Envelope();
                envelope.Parse(m_Envelope);
                return envelope;
            }
        }

        /// <summary>
        /// Gets message IMAP BODYSTRUCTURE. This property is available only if IMAP_FetchItem_Flags.BodyStructure was specified,
        /// otherwise throws exception.
        /// </summary>
        public IMAP_BODY BodyStructure
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.BodyStructure) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.BodyStructure wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                IMAP_BODY bodyStructure = new IMAP_BODY();
                bodyStructure.Parse(m_BodyStructure);
                return bodyStructure;
            }
        }

        /// <summary>
        /// Gets message header data. This property is available only if IMAP_FetchItem_Flags.Header was specified,
        /// otherwise throws exception.
        /// </summary>
        public byte[] HeaderData
        {
            get
            {
                if (
                    !((m_FetchFlags & IMAP_FetchItem_Flags.Header) != 0 ||
                      (m_FetchFlags & IMAP_FetchItem_Flags.Message) != 0))
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.Header wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                if ((m_FetchFlags & IMAP_FetchItem_Flags.Message) != 0)
                {
                    return Encoding.ASCII.GetBytes(MimeUtils.ParseHeaders(new MemoryStream(m_Data)));
                }
                else
                {
                    return m_Data;
                }
            }
        }

        /// <summary>
        /// Gets message data. This property is available only if IMAP_FetchItem_Flags.Message was specified,
        /// otherwise throws exception.
        /// </summary>
        public byte[] MessageData
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.Message) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.Message wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return m_Data;
            }
        }

        /// <summary>
        /// Gets if message is unseen. This property is available only if IMAP_FetchItem_Flags.MessageFlags was specified,
        /// otherwise throws exception.
        /// </summary>
        public bool IsNewMessage
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.MessageFlags) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.MessageFlags wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return (m_Flags & IMAP_MessageFlags.Seen) == 0;
            }
        }

        /// <summary>
        /// Gets if message is answered. This property is available only if IMAP_FetchItem_Flags.MessageFlags was specified,
        /// otherwise throws exception.
        /// </summary>
        public bool IsAnswered
        {
            get
            {
                if ((m_FetchFlags & IMAP_FetchItem_Flags.MessageFlags) == 0)
                {
                    throw new Exception(
                        "IMAP_FetchItem_Flags.MessageFlags wasn't specified in FetchMessages command, becuse of it this property is unavailable.");
                }

                return (m_Flags & IMAP_MessageFlags.Answered) != 0;
            }
        }

        /// <summary>
        /// Gets message data(headers or full message), it depends on HeadersOnly property.
        /// </summary>
        [Obsolete("Use HeaderData or MessageData data instead.")]
        public byte[] Data
        {
            get { return m_Data; }
        }

        /// <summary>
        /// Gets if headers or full message requested in fetch.
        /// </summary>
        [Obsolete("Use FetchFlags property instead !")]
        public bool HeadersOnly
        {
            get { return (m_FetchFlags & IMAP_FetchItem_Flags.Header) != 0; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="no">Number of message in folder.</param>
        /// <param name="uid">Message UID.</param>
        /// <param name="size">Message size.</param>
        /// <param name="data">Message data.</param>
        /// <param name="flags">Message flags.</param>
        /// <param name="internalDate">Message INTERNALDATE.</param>
        /// <param name="envelope">Envelope string.</param>
        /// <param name="bodyStructure">BODYSTRUCTURE string.</param>
        /// <param name="fetchFlags">Specifies what data fetched from IMAP server.</param>
        internal IMAP_FetchItem(int no,
                                int uid,
                                int size,
                                byte[] data,
                                IMAP_MessageFlags flags,
                                string internalDate,
                                string envelope,
                                string bodyStructure,
                                IMAP_FetchItem_Flags fetchFlags)
        {
            m_No = no;
            m_UID = uid;
            m_Size = size;
            m_Data = data;
            m_Flags = flags;
            m_InternalDate = internalDate;
            m_Envelope = envelope;
            m_FetchFlags = fetchFlags;
            m_BodyStructure = bodyStructure;
        }

        #endregion
    }
}