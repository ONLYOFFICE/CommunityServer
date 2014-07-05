/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
