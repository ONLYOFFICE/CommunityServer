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
using ASC.Xmpp.Core.protocol.iq.auth;
using ASC.Xmpp.Core.utils;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Users;

namespace ASC.Xmpp.Server.Authorization
{
	[XmppHandler(typeof(AuthIq))]
	class AuthIQHandler : XmppStreamHandler
	{
		public override void ElementHandle(XmppStream stream, Element element, XmppHandlerContext context)
		{
			var iq = (AuthIq)element;

			if (stream.Authenticated)
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToConflict(iq));
				return;
			}

			if (iq.Type == IqType.get) ProcessAuthIQGet(stream, iq, context);
			else if (iq.Type == IqType.set) ProcessAuthIQSet(stream, iq, context);
			else context.Sender.SendTo(stream, XmppStanzaError.ToNotAcceptable(iq));
		}

		private void ProcessAuthIQSet(XmppStream stream, AuthIq iq, XmppHandlerContext context)
		{
			if (string.IsNullOrEmpty(iq.Query.Username) || string.IsNullOrEmpty(iq.Query.Resource))
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToNotAcceptable(iq));
				return;
			}

			bool authorized = false;
			if (!string.IsNullOrEmpty(iq.Query.Digest))
			{
				authorized = AuthDigest(iq.Query.Username, iq.Query.Digest, stream, context.UserManager);
			}
			if (!string.IsNullOrEmpty(iq.Query.Password))
			{
				authorized = AuthPlain(iq.Query.Username, iq.Query.Password, stream, context.UserManager);
			}
			if (authorized)
			{
				stream.Authenticate(iq.Query.Username);

				var answer = new IQ(IqType.result);
				answer.Id = iq.Id;
				answer.To = iq.From;
				answer.From = iq.To;
				context.Sender.SendTo(stream, answer);
			}
			else
			{
				context.Sender.SendTo(stream, XmppStanzaError.ToNotAuthorized(iq));
			}
		}

		private void ProcessAuthIQGet(XmppStream stream, AuthIq iq, XmppHandlerContext context)
		{
			iq.SwitchDirection();
			iq.Type = IqType.result;
			iq.Query.AddChild(new Element("password"));
			iq.Query.AddChild(new Element("digest"));
			iq.Query.AddChild(new Element("resource"));
			context.Sender.SendTo(stream, iq);
		}

		private bool AuthDigest(string username, string hash, XmppStream stream, UserManager userManager)
		{
			var user = userManager.GetUser(new Jid(username, stream.Domain, null));
			if (user != null)
			{
				string serverhash = Hash.Sha1Hash(stream.Id + user.Password);
				return string.Compare(serverhash, hash, StringComparison.OrdinalIgnoreCase) == 0;
			}
			return false;
		}

		private bool AuthPlain(string username, string password, XmppStream stream, UserManager userManager)
		{
			var user = userManager.GetUser(new Jid(username, stream.Domain, null));
			if (user != null)
			{
				return string.Compare(user.Password, password, StringComparison.Ordinal) == 0;
			}
			return false;
		}
	}
}