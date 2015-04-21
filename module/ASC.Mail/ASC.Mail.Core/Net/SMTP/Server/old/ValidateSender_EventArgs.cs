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
