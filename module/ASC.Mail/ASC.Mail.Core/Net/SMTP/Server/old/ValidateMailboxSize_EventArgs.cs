/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
	/// Provides data for the ValidateMailboxSize event.
	/// </summary>
	public class ValidateMailboxSize_EventArgs
	{
		private SMTP_Session m_pSession = null;
		private string       m_eAddress = "";
		private long         m_MsgSize  = 0;
		private bool         m_IsValid  = true;

		/// <summary>
		/// Default constructor.
		/// </summary>
		/// <param name="session">Reference to smtp session.</param>
		/// <param name="eAddress">Email address of recipient.</param>
		/// <param name="messageSize">Message size.</param>
		public ValidateMailboxSize_EventArgs(SMTP_Session session,string eAddress,long messageSize)
		{
			m_pSession = session;
			m_eAddress = eAddress;
			m_MsgSize  = messageSize;
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
		/// Email address which mailbox size to check.
		/// </summary>
		public string eAddress
		{
			get{ return m_eAddress; }
		}

		/// <summary>
		/// Message size.NOTE: value 0 means that size is unknown.
		/// </summary>
		public long MessageSize
		{
			get{ return m_MsgSize; }
		}

		/// <summary>
		/// Gets or sets if mailbox size is valid.
		/// </summary>
		public bool IsValid
		{
			get{ return m_IsValid; }

			set{ m_IsValid = value; }
		}

	//	public SMTP_Session aa
	//	{
	//		get{ return null; }
	//	}

		#endregion

	}
}
