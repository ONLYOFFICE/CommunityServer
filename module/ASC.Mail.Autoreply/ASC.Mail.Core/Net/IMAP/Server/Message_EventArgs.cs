/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
    /// <summary>
    /// Provides data for message related events.
    /// </summary>
    public class Message_EventArgs
    {
        #region Members

        private readonly string m_CopyLocation = "";
        private readonly string m_Folder = "";
        private readonly bool m_HeadersOnly;
        private readonly IMAP_Message m_pMessage;

        #endregion

        #region Properties

        /// <summary>
        /// Gets IMAP folder.
        /// </summary>
        public string Folder
        {
            get { return m_Folder; }
        }

        /// <summary>
        /// Gets IMAP message info.
        /// </summary>
        public IMAP_Message Message
        {
            get { return m_pMessage; }
        }

        /// <summary>
        /// Gets message new location. NOTE: this is available for copy command only.
        /// </summary>
        public string CopyLocation
        {
            get { return m_CopyLocation; }
        }

        /// <summary>
        /// Gets or sets message data. NOTE: this is available for GetMessage and StoreMessage event only.
        /// </summary>
        public byte[] MessageData { get; set; }

        /// <summary>
        /// Gets if message headers or full message wanted. NOTE: this is available for GetMessage event only.
        /// </summary>
        public bool HeadersOnly
        {
            get { return m_HeadersOnly; }
        }

        /// <summary>
        /// Gets or sets custom error text, which is returned to client.
        /// </summary>
        public string ErrorText { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="folder">IMAP folder which message is.</param>
        /// <param name="msg"></param>
        public Message_EventArgs(string folder, IMAP_Message msg)
        {
            m_Folder = folder;
            m_pMessage = msg;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="folder">IMAP folder which message is.</param>
        /// <param name="msg"></param>
        /// <param name="copyLocation"></param>
        public Message_EventArgs(string folder, IMAP_Message msg, string copyLocation)
        {
            m_Folder = folder;
            m_pMessage = msg;
            m_CopyLocation = copyLocation;
        }

        /// <summary>
        /// GetMessage constructor.
        /// </summary>
        /// <param name="folder">IMAP folder which message is.</param>
        /// <param name="msg"></param>
        /// <param name="headersOnly">Specifies if messages headers or full message is needed.</param>
        public Message_EventArgs(string folder, IMAP_Message msg, bool headersOnly)
        {
            m_Folder = folder;
            m_pMessage = msg;
            m_HeadersOnly = headersOnly;
        }

        #endregion
    }
}