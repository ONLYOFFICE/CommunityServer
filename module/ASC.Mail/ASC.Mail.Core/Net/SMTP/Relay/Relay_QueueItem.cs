/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


namespace ASC.Mail.Net.SMTP.Relay
{
    #region usings

    using System.IO;

    #endregion

    /// <summary>
    /// Thsi class holds Relay_Queue queued item.
    /// </summary>
    public class Relay_QueueItem
    {
        #region Members

        private readonly string m_From = "";
        private readonly string m_MessageID = "";
        private readonly Stream m_pMessageStream;
        private readonly Relay_Queue m_pQueue;
        private readonly string m_To = "";

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="queue">Item owner queue.</param>
        /// <param name="from">Sender address.</param>
        /// <param name="to">Target recipient address.</param>
        /// <param name="messageID">Message ID.</param>
        /// <param name="message">Raw mime message. Message reading starts from current position.</param>
        /// <param name="tag">User data.</param>
        internal Relay_QueueItem(Relay_Queue queue,
                                 string from,
                                 string to,
                                 string messageID,
                                 Stream message,
                                 object tag)
        {
            m_pQueue = queue;
            m_From = from;
            m_To = to;
            m_MessageID = messageID;
            m_pMessageStream = message;
            Tag = tag;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets from address.
        /// </summary>
        public string From
        {
            get { return m_From; }
        }

        /// <summary>
        /// Gets message ID which is being relayed now.
        /// </summary>
        public string MessageID
        {
            get { return m_MessageID; }
        }

        /// <summary>
        /// Gets raw mime message which must be relayed.
        /// </summary>
        public Stream MessageStream
        {
            get { return m_pMessageStream; }
        }

        /// <summary>
        /// Gets this relay item owner queue.
        /// </summary>
        public Relay_Queue Queue
        {
            get { return m_pQueue; }
        }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets target recipient.
        /// </summary>
        public string To
        {
            get { return m_To; }
        }

        #endregion
    }
}