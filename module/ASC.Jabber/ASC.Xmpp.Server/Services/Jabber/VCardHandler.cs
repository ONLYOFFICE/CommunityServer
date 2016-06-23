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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.vcard;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Vcard))]
	class VCardHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (!iq.HasTo) iq.To = iq.From;
			if (iq.Type == IqType.get) return GetVCard(stream, iq, context);
			else if (iq.Type == IqType.set) return SetVCard(stream, iq, context);
			else return XmppStanzaError.ToBadRequest(iq);
		}

		private IQ SetVCard(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.From != iq.To) return XmppStanzaError.ToForbidden(iq);

			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;
			context.StorageManager.VCardStorage.SetVCard(iq.To, iq.Vcard);
			answer.Vcard = iq.Vcard;
			return answer;
		}

		private IQ GetVCard(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var answer = new IQ(IqType.result);
			answer.Id = iq.Id;
			answer.To = iq.From;
			answer.From = iq.To;

			answer.Vcard = iq.To.HasUser ?
                context.StorageManager.VCardStorage.GetVCard(iq.To, iq.Id) :
				GetServiceVcard(iq.To, context);

			if (answer.Vcard == null) return XmppStanzaError.ToNotFound(iq);
			return answer;
		}

		private Vcard GetServiceVcard(Jid jid, XmppHandlerContext context)
		{
			var serviceManager = (XmppServiceManager)context.ServiceProvider.GetService(typeof(XmppServiceManager));
			var service = serviceManager.GetService(jid);
			return service != null ? service.Vcard : null;
		}
	}
}