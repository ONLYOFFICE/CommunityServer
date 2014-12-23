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
