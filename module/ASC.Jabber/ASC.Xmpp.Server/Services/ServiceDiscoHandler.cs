/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using System;

namespace ASC.Xmpp.Server.Services
{
	[XmppHandler(typeof(DiscoInfo))]
	[XmppHandler(typeof(DiscoItems))]
	class ServiceDiscoHandler : XmppStanzaHandler
	{
		protected Jid Jid
		{
			get;
			private set;
		}

		protected XmppServiceManager ServiceManager
		{
			get;
			private set;
		}

		public ServiceDiscoHandler(Jid jid)
		{
			Jid = jid;
		}

		public override void OnRegister(IServiceProvider serviceProvider)
		{
			ServiceManager = (XmppServiceManager)serviceProvider.GetService(typeof(XmppServiceManager));
		}

		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.HasTo && iq.To != Jid) return XmppStanzaError.ToServiceUnavailable(iq);
			if (iq.Query is DiscoInfo && iq.Type == IqType.get) return GetDiscoInfo(stream, iq, context);
			if (iq.Query is DiscoItems && iq.Type == IqType.get) return GetDiscoItems(stream, iq, context);
			return XmppStanzaError.ToServiceUnavailable(iq);
		}

		protected virtual IQ GetDiscoInfo(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (((DiscoInfo)iq.Query).Node != null) return XmppStanzaError.ToServiceUnavailable(iq);

			var service = ServiceManager.GetService(Jid);
			if (service == null) return XmppStanzaError.ToItemNotFound(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.From = Jid;
			answer.To = iq.From;
			answer.Query = service.DiscoInfo;
			return answer;
		}

		protected virtual IQ GetDiscoItems(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (((DiscoItems)iq.Query).Node != null) return XmppStanzaError.ToServiceUnavailable(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.From = Jid;
			answer.To = iq.From;
			var items = new DiscoItems();
			answer.Query = items;
			foreach (var service in ServiceManager.GetChildServices(Jid))
			{
				if (service.DiscoItem != null) items.AddDiscoItem(service.DiscoItem);
			}
			return answer;
		}
	}
}
