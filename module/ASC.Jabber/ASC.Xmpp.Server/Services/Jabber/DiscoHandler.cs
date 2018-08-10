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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.disco;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;
using System;

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
				IdleWatcher.StartWatch(iq.Id, TimeSpan.FromSeconds(4.5f), IQLost, iq);
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
            if (session != null)
            {
                // iChat bug
                if (iq.Id == null || iq.Id.IndexOf("ichat", StringComparison.OrdinalIgnoreCase) < 0)
                {
                    xmppSender.SendTo(session, XmppStanzaError.ToServiceUnavailable(iq));
                }
                else
                {
                    xmppSender.SendTo(session, ToResult(iq));
                }
            }
		}

		private IQ ToResult(IQ iq)
		{
			if (!iq.Switched) iq.SwitchDirection();
			iq.Type = IqType.result;
			return iq;
		}
	}
}