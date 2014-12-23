/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
