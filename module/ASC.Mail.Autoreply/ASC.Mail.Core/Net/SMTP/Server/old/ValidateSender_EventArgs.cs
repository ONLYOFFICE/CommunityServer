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
	/// Provides data for the ValidateMailFrom event.
	/// </summary>
	public class ValidateSender_EventArgs
	{
		private SMTP_Session m_pSession  = null;
		private string       m_MailFrom  = "";
		private string       m_ErrorText = "";
		private bool         m_Validated = true;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">Reference to smtp session.</param>
		/// <param name="mailFrom">Sender email address.</param>
		public ValidateSender_EventArgs(SMTP_Session session,string mailFrom)
		{
			m_pSession = session;
			m_MailFrom = mailFrom;
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
		/// Sender's email address.
		/// </summary>
		public string MailFrom
		{
			get{ return m_MailFrom; }
		}

		/// <summary>
		/// Gets or sets error text reported to connected client.
		/// </summary>
		public string ErrorText
		{
			get{ return m_ErrorText; }

			set{
				value = value.Replace("\r\n"," ");
				if(value.Length > 200){
					m_ErrorText = value.Substring(0,200);
				}
				else{
					m_ErrorText = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets if sender is ok.
		/// </summary>
		public bool Validated
		{
			get{ return m_Validated; }

			set{ m_Validated = value; }
		}

		#endregion

	}
}
