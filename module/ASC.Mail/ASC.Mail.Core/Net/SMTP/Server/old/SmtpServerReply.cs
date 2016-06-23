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
	/// SMTP server reply info. This class specifies smtp reply what is returned to connected client.
	/// </summary>
	public class SmtpServerReply
	{
		private bool   m_ErrorReply    = false;
		private int    m_SmtpReplyCode = -1;
		private string m_ReplyText     = "";

		/// <summary>
		/// Default consttuctor.
		/// </summary>
		public SmtpServerReply()
		{
		}


		#region method ToSmtpReply

		internal string ToSmtpReply(string defaultSmtpRelyCode,string defaultReplyText)
		{
			string replyText = "";
			if(this.SmtpReplyCode == -1){
				replyText = defaultSmtpRelyCode + " ";
			}
			else{
				replyText = this.SmtpReplyCode.ToString() + " ";
			}

			if(this.ReplyText == ""){
				replyText += defaultReplyText;
			}
			else{
				replyText += this.ReplyText;
			}

			return replyText;
		}

		#endregion


		#region Properties Implementation

		/// <summary>
		/// Gets or sets if CustomReply is error or ok reply.
		/// </summary>
		public bool ErrorReply
		{
			get{ return m_ErrorReply; }

			set{ m_ErrorReply = value; }
		}

		/// <summary>
		/// Gets or sets SMTP reply code (250,500,500, ...). Value -1 means that SMTP reply code isn't specified and server uses it's defult error code. 
		/// </summary>
		public int SmtpReplyCode
		{
			get{ return m_SmtpReplyCode; }

			set{ m_SmtpReplyCode = value; }
		}

		/// <summary>
		/// Gets or sets reply text what is shown to connected client. 
		/// Note: Maximum lenth of reply text is 500 chars and text can contain only ASCII chars 
		/// without CR or LF.  
		/// </summary>
		public string ReplyText
		{
			get{ return m_ReplyText; }

			set{				
				if(!Core.IsAscii(value)){
					throw new Exception("Reply text can contian only ASCII chars !");
				}
				if(value.Length > 500){
					value = value.Substring(0,500);
				}
				value = value.Replace("\r","").Replace("\n","");

				m_ReplyText = value;
			}
		}

		#endregion

	}
}
