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