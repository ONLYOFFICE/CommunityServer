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


using System.Collections.Generic;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.iq.@private;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;

namespace ASC.Xmpp.Server.Services.Jabber
{
	[XmppHandler(typeof(Private))]
	class PrivateHandler : XmppStanzaHandler
	{
		public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			if (iq.To != null && iq.From != iq.To) return XmppStanzaError.ToForbidden(iq);

			if (iq.Type == IqType.get) return GetPrivate(stream, iq, context);
			else if (iq.Type == IqType.set) return SetPrivate(stream, iq, context);
			else return XmppStanzaError.ToBadRequest(iq);
		}

		private IQ SetPrivate(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var @private = (Private)iq.Query;
			
            if (!@private.HasChildElements) return XmppStanzaError.ToNotAcceptable(iq);

			foreach (var childNode in @private.ChildNodes)
			{
				if (childNode is Element)
				{
                    context.StorageManager.PrivateStorage.SetPrivate(iq.From, (Element)childNode);
				}
			}
            iq.Query = null;
			iq.SwitchDirection();
			iq.Type = IqType.result;
			return iq;
		}

		private IQ GetPrivate(XmppStream stream, IQ iq, XmppHandlerContext context)
		{
			var privateStore = (Private)iq.Query;
			
            if (!privateStore.HasChildElements) return XmppStanzaError.ToNotAcceptable(iq);

			var retrived = new List<Element>();
			foreach (var childNode in privateStore.ChildNodes)
			{
				if (childNode is Element)
				{
					var elementToRetrive = (Element)childNode;
                    var elementRestored = context.StorageManager.PrivateStorage.GetPrivate(iq.From, elementToRetrive);
					retrived.Add(elementRestored ?? elementToRetrive);
				}
			}

            privateStore.RemoveAllChildNodes();
			foreach (var element in retrived)
			{
				privateStore.AddChild(element);
			}
			
            iq.SwitchDirection();
			iq.Type = IqType.result;
			return iq;
		}
	}
}