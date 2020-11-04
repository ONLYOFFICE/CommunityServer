/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
