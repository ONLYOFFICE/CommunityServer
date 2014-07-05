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
using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;

namespace ASC.Xmpp.Server.Services.Jabber
{
	class DiscoHandler : ServiceDiscoHandler
	{
		private IXmppSender xmppSender;

		private XmppSessionManager sessionManager;


		public DiscoHandler(Jid jid)
			: base(jid)
		{

		}

		public override void OnRegister(IServiceProvider serviceProvider)
		{
			sessionManager = (XmppSessionManager)serviceProvider.GetService(typeof(XmppSessionManager));
			xmppSender = (IXmppSender)serviceProvider.GetService(typeof(IXmppSender));
			base.OnRegister(serviceProvider);
		}

		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.Type == IqType.result || iq.Type == IqType.error)
			{
				IdleWatcher.StopWatch(iq.Id);

				var session = context.SessionManager.GetSession(iq.From);
				if (session != null && iq.Query is DiscoInfo)
				{
					session.ClientInfo.SetDiscoInfo((DiscoInfo)iq.Query);
				}
				if (iq.HasTo)
				{
					session = context.SessionManager.GetSession(iq.To);
					if (session != null) context.Sender.SendTo(session, iq);
					return null;
				}
			}
			else if (iq.HasTo && iq.To.HasUser)
			{
				return GetUserDisco(stream, iq, context);
			}
			return base.HandleIQ(stream, iq, context);
		}

		private IQ GetUserDisco(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.To.HasResource)
			{
				var session = context.SessionManager.GetSession(iq.To);

				if (session != null && iq.Query is DiscoInfo)
				{
					var discoInfo = session.ClientInfo.GetDiscoInfo(((DiscoInfo)iq.Query).Node);
					if (discoInfo != null)
					{
						iq.Query = discoInfo;
						return ToResult(iq);
					}
				}

				if (session == null) return XmppStanzaError.ToRecipientUnavailable(iq);
				context.Sender.SendTo(session, iq);
				IdleWatcher.StartWatch(UniqueId.CreateNewId(), TimeSpan.FromSeconds(4.5f), IQLost, iq);
			}
			else
			{
				if (iq.Query is DiscoInfo && context.UserManager.IsUserExists(iq.To))
				{
					((DiscoInfo)iq.Query).AddIdentity(new DiscoIdentity("registered", "account"));
					return ToResult(iq);
				}
				else if (iq.Query is DiscoItems)
				{
					foreach (var s in context.SessionManager.GetBareJidSessions(iq.To))
					{
						((DiscoItems)iq.Query).AddDiscoItem(new DiscoItem() { Jid = s.Jid });
					}
					return ToResult(iq);
				}
				return XmppStanzaError.ToServiceUnavailable(iq);
			}
			return null;
		}

		private void IQLost(object sender, TimeoutEventArgs e)
		{
			var iq = (IQ)e.Data;
			var session = sessionManager.GetSession(iq.From);
			if (session != null) xmppSender.SendTo(session, XmppStanzaError.ToServiceUnavailable(iq));
		}

		private IQ ToResult(IQ iq)
		{
			if (!iq.Switched) iq.SwitchDirection();
			iq.Type = IqType.result;
			return iq;
		}
	}
}