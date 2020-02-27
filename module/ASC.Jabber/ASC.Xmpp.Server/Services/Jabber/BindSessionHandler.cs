/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.bind;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;
using System;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Core.protocol.iq.session.Session))]
	[XmppHandler(typeof(Bind))]
	class BindSessionHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.Session != null) return ProcessSession(stream, iq, context);
			else if (iq.Bind != null) return ProcessBind(stream, iq, context);
			else return XmppStanzaError.ToServiceUnavailable(iq);
		}

		private IQ ProcessBind(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.Type != IqType.set) return XmppStanzaError.ToBadRequest(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;

			var bind = (Bind)iq.Bind;
			var resource = !string.IsNullOrEmpty(bind.Resource) ? bind.Resource : stream.User;

			if (bind.TagName.Equals("bind", StringComparison.OrdinalIgnoreCase))
			{
				var jid = new Jid(stream.User, stream.Domain, resource);

				var session = context.SessionManager.GetSession(jid);
				if (session != null)
				{
					if (session.Stream.Id != stream.Id)
					{
						context.Sender.SendToAndClose(session.Stream, XmppStreamError.Conflict);
						answer.AddTag("newTalkWindow", "true");
					}
					else
					{
						return XmppStanzaError.ToConflict(iq);
					}
				}

				stream.BindResource(resource);
				context.SessionManager.AddSession(new XmppSession(jid, stream));
				answer.Bind = new Bind(jid);
			}
			else if (bind.TagName.Equals("unbind", StringComparison.OrdinalIgnoreCase))
			{
				if (!stream.Resources.Contains(resource)) return XmppStanzaError.ToNotFound(iq);

				context.SessionManager.CloseSession(iq.From);
				stream.UnbindResource(resource);
				if (stream.Resources.Count == 0)
				{
					context.Sender.CloseStream(stream);
				}
			}
			else
			{
				return XmppStanzaError.ToBadRequest(iq);
			}
			if (stream.MultipleResources) answer.To = iq.From;
			return answer;
		}

		private IQ ProcessSession(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var session = context.SessionManager.GetSession(iq.From);
			if (session == null) return XmppStanzaError.ToItemNotFound(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.Session = new Core.protocol.iq.session.Session();
			session.Active = true;
			return answer;
		}
	}
}