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