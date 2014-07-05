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
