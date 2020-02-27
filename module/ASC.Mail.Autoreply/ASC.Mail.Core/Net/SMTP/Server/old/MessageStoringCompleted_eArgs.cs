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


using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace LumiSoft.Net.SMTP.Server
{
    /// <summary>
    /// This class provides data for the MessageStoringCompleted event.
    /// </summary>
    public class MessageStoringCompleted_eArgs
    {
        private SMTP_Session    m_pSession       = null;
        private string          m_ErrorText      = null;
      //private long            m_StoredCount    = 0;
        private Stream          m_pMessageStream = null;
        private SmtpServerReply m_pCustomReply   = null;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="session">Reference to calling SMTP session.</param>
        /// <param name="errorText">Gets errors what happened on storing message or null if no errors.</param>
        /// <param name="messageStream">Gets message stream where messages was stored. Stream postions is End of Stream, where message storing ended.</param>
        public MessageStoringCompleted_eArgs(SMTP_Session session,string errorText,Stream messageStream)
        {
            m_pSession       = session;
            m_ErrorText      = errorText;
            m_pMessageStream = messageStream;
            m_pCustomReply   = new SmtpServerReply();
        }


        #region Properties Implementation

        /// <summary>
		/// Gets reference to smtp session.
		/// </summary>
		public SMTP_Session Session
		{
			get{ return m_pSession; }
		}

        /// <summary>
        /// Gets errors what happened on storing message or null if no errors.
        /// </summary>
        public string ErrorText
        {
            get{ return m_ErrorText; }
        }
        /*
        /// <summary>
        /// Gets count of bytes stored to MessageStream. This value as meaningfull value only if this.ErrorText == null (No errors).
        /// </summary>
        public long StoredCount
        {
            get{ return m_StoredCount; }
        }*/

        /// <summary>
        /// Gets message stream where messages was stored. Stream postions is End of Stream, where message storing ended.
        /// </summary>
        public Stream MessageStream
        {
            get{ return m_pMessageStream; }
        }

        /// <summary>
		/// Gets SMTP server reply info. You can use this property for specifying custom reply text and optionally SMTP reply code.
		/// </summary>
		public SmtpServerReply ServerReply
		{
			get{ return m_pCustomReply; }
		}

        #endregion

    }
}
