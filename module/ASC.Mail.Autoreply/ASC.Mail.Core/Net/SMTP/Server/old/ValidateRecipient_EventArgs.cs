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

namespace LumiSoft.Net.SMTP.Server
{
	/// <summary>
	/// Provides data for the ValidateMailTo event.
	/// </summary>
	public class ValidateRecipient_EventArgs
	{
		private SMTP_Session m_pSession       = null;
		private string       m_MailTo         = "";
		private bool         m_Validated      = true;
		private bool         m_Authenticated  = false;
		private bool         m_LocalRecipient = true;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">Reference to smtp session.</param>
		/// <param name="mailTo">Recipient email address.</param>
		/// <param name="authenticated">Specifies if connected user is authenticated.</param>
		public ValidateRecipient_EventArgs(SMTP_Session session,string mailTo,bool authenticated)
		{
			m_pSession      = session;
			m_MailTo        = mailTo;
			m_Authenticated = authenticated;
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
		/// Recipient's email address.
		/// </summary>
		public string MailTo
		{
			get{ return m_MailTo; }
		}

		/// <summary>
		/// Gets if connected user is authenticated.
		/// </summary>
		public bool Authenticated
		{
			get{ return m_Authenticated; }
		}

		/// <summary>
		/// IP address of computer, which is sending mail to here.
		/// </summary>
		public string ConnectedIP
		{
			get{ return m_pSession.RemoteEndPoint.Address.ToString(); }
		}

		/// <summary>
		/// Gets or sets if reciptient is allowed to send mail here.
		/// </summary>
		public bool Validated
		{
			get{ return m_Validated; }

			set{ m_Validated = value; }
		}

		/// <summary>
		/// Gets or sets if recipient is local or needs relay.
		/// </summary>
		public bool LocalRecipient
		{
			get{ return m_LocalRecipient; }

			set{ m_LocalRecipient = value; }
		}

		#endregion

	}
}
