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
using ASC.Xmpp.Core.protocol.extensions.nickname;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Streams;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Xmpp.Server.Services.Jabber
{
    [XmppHandler(typeof(Message))]
    class MessageAnnounceHandler : XmppStanzaHandler
    {
        public const string ANNOUNCE = "announce";

        public const string ONLINE = "online";

        public const string ONLINEBROADCAST = "onlinebroadcast";

        public const string SERVICE = "service";

        public const string MOTD = "motd";

        public override void HandleMessage(XmppStream stream, Message message, XmppHandlerContext context)
        {
            if (!message.HasTo || !message.To.HasResource) return;

            string[] commands = message.To.Resource.Split('/');
            if (commands.Length == 1 && commands[0] == ANNOUNCE)
            {
                Announce(stream, message, context);
            }
            else if (commands.Length == 2 && commands[1] == ONLINE)
            {
                AnnounceOnline(stream, message, context);
            }
            else if (commands.Length == 2 && commands[1] == ONLINEBROADCAST)
            {
                AnnounceOnlineBroadcast(stream, message, context);
            }
            else if (commands.Length == 2 && commands[1] == SERVICE)
            {
                AnnounceService(stream, message, context);
            }
            else
            {
                context.Sender.SendTo(stream, XmppStanzaError.ToServiceUnavailable(message));
            }
        }

        private void Announce(XmppStream stream, Message message, XmppHandlerContext context)
        {
            var userName = GetUser(message);
            message.Body = string.Format("{0} announces {1}", userName, message.Body);
            var offlineMessages = new List<Message>();

            foreach (var user in context.UserManager.GetUsers(stream.Domain))
            {
                message.To = user.Jid;
                var session = context.SessionManager.GetSession(message.To);
                if (session != null)
                {
                    context.Sender.SendTo(session, message);
                }
                else
                {
                    offlineMessages.Add(message);
                }
            }
            context.StorageManager.OfflineStorage.SaveOfflineMessages(offlineMessages.ToArray());
        }

        private void AnnounceOnline(XmppStream stream, Message message, XmppHandlerContext context)
        {
            foreach (var session in context.SessionManager.GetSessions().Where(x => x.Available))
            {
                message.To = session.Jid;
                context.Sender.SendTo(session, message);
            }
        }

        private void AnnounceOnlineBroadcast(XmppStream stream, Message message, XmppHandlerContext context)
        {
            string user = GetUser(message);
            message.Body = string.Format("{0} says:\r\n{1}", user, message.Body);
            AnnounceService(stream, message, context);
        }

        private void AnnounceService(XmppStream stream, Message message, XmppHandlerContext context)
        {
            message.From = new Jid(stream.Domain);
            message.Nickname = null;
            AnnounceOnline(stream, message, context);
        }

        private string GetUser(Message message)
        {
            var nick = message.SelectSingleElement<Nickname>();
            return nick != null ? nick.Value : message.From.User;
        }
    }
}