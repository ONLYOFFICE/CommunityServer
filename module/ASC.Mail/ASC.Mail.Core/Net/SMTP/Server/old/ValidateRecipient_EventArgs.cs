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
