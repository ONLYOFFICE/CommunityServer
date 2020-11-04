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
