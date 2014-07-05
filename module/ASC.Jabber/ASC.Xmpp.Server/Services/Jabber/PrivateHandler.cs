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