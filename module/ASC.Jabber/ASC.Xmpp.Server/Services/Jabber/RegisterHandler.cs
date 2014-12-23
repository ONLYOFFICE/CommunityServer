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

using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.register;
using ASC.Xmpp.Core.utils.Idn;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Users;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Register))]
	class RegisterHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.Type == IqType.get) return GetRegister(stream, iq, context);
			else if (iq.Type == IqType.set) return SetRegister(stream, iq, context);
			return null;
		}

		private IQ GetRegister(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var register = (Register)iq.Query;
			register.Username = string.Empty;
			register.Password = string.Empty;
			iq.Type = IqType.result;

			if (iq.From.HasUser && context.UserManager.IsUserExists(iq.From))
			{
				register.Username = iq.From.User;
				register.AddChild(new Element("registered"));
				iq.SwitchDirection();
				iq.From = null;
			}
			else
			{
				iq.From = iq.To = null;
			}
			return iq;
		}

		private IQ SetRegister(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var register = (Register)iq.Query;
			iq.Type = IqType.result;

			if (register.RemoveAccount)
			{
				if (!stream.Authenticated || !iq.From.HasUser) context.Sender.SendToAndClose(stream, XmppStreamError.NotAuthorized);

				context.UserManager.RemoveUser(iq.From);
				foreach (var s in context.SessionManager.GetBareJidSessions(iq.From))
				{
					if (s.Stream.Id == stream.Id) continue;
					context.Sender.SendToAndClose(s.Stream, XmppStreamError.Conflict);
				}
				//TODO: remove roster subscriptions
				register.RemoveAllChildNodes();
				iq.SwitchDirection();
				return iq;
			}

			if (string.IsNullOrEmpty(register.Username) ||
				string.IsNullOrEmpty(register.Password) ||
				Stringprep.NamePrep(register.Username) != register.Username)
			{
				var error = XmppStanzaError.ToNotAcceptable(iq);
				if (string.IsNullOrEmpty(register.Username)) error.Error.Message = "Empty required field Username.";
				else if (string.IsNullOrEmpty(register.Password)) error.Error.Message = "Empty required field Password.";
				else if (Stringprep.NamePrep(register.Username) != register.Username) error.Error.Message = "Invalid character.";
				return error;
			}

			var userJid = new Jid(register.Username, stream.Domain, null);
			if (context.UserManager.IsUserExists(userJid))
			{
				return XmppStanzaError.ToConflict(iq);
			}

			var user = new User(userJid, register.Password);
			context.UserManager.SaveUser(user);

			register.RemoveAllChildNodes();
			if (stream.Authenticated) iq.SwitchDirection();
			else iq.To = null;
			iq.From = null;
			return iq;
		}
	}
}