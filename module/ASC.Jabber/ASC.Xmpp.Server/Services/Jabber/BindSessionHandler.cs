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