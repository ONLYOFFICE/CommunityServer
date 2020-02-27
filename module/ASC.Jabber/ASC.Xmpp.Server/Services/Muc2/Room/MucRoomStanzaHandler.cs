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
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.protocol.x.muc.iq.admin;
using ASC.Xmpp.Core.protocol.x.muc.iq.owner;
using ASC.Xmpp.Core.protocol.x.tm.history;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services.Muc2.Room.Member;
using ASC.Xmpp.Server.Storage;
using ASC.Xmpp.Server.Streams;
using Error = ASC.Xmpp.Core.protocol.client.Error;

namespace ASC.Xmpp.Server.Services.Muc2.Room
{
    [XmppHandler(typeof(Stanza))]
    internal class MucRoomStanzaHandler : XmppStanzaHandler
    {
        public MucRoom Room { get; set; }


        internal MucRoomStanzaHandler(MucRoom room)
        {
            Room = room;
        }

        public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
        {
            //Admins iq

            //New member
            MucRoomMember member = Room.GetRealMember(iq.From);
            if (member != null)
            {
                if (iq.Query != null)
                {
                    if (iq.Query is Admin && (member.Affiliation == Affiliation.admin || member.Affiliation == Affiliation.owner))
                    {
                        Room.AdminCommand(iq, member);
                    }
                    else if (iq.Query is Owner && (member.Affiliation == Affiliation.owner))
                    {
                        Room.OwnerCommand(iq, member);
                    }
                    else if (iq.Query is Core.protocol.x.tm.history.History  && iq.Type == IqType.get)
                    {
                        Jid jid = iq.To;
                        var mucStore = new DbMucStore();
                        var properties = new Dictionary<string, string>(1) {{"connectionStringName", "core"}};
                        mucStore.Configure(properties);

                        var history = (Core.protocol.x.tm.history.History)iq.Query;

                        foreach (var msg in mucStore.GetMucMessages(jid, history.Count, history.StartIndex))
                        {
                            if (msg == null) continue;

                            history.AddChild(HistoryItem.FromMessage(msg));
                        }
                        iq.Type = IqType.result;
                        iq.SwitchDirection();
                        return iq;
                    }
                    else
                    {
                        XmppStanzaError.ToForbidden(iq);
                    }
                }
                else
                {
                    XmppStanzaError.ToBadRequest(iq);
                }
            }
            else
            {
                XmppStanzaError.ToForbidden(iq);
            }
            if (!iq.Switched)
            {
                iq.SwitchDirection();
            }
            iq.From = Room.Jid;
            return iq;
        }

        public override void HandlePresence(Streams.XmppStream stream, Presence presence, XmppHandlerContext context)
        {
            string userName = presence.To.Resource;
            if (!string.IsNullOrEmpty(userName) && presence.Type == PresenceType.available)
            {
                //New member
                MucRoomMember member = Room.GetRealMember(presence.From);
                if (member != null)
                {
                    if (ReferenceEquals(stream, member.Stream))
                    {
                        if (!Room.TryNickChange(presence))
                        {
                            ErrorPresence(presence, ErrorCondition.NotAcceptable);
                            context.Sender.SendTo(stream, presence);
                        }
                    }
                    else
                    {
                        //Conflict. user with this jid already in room
                        ErrorPresence(presence, ErrorCondition.Conflict);
                        context.Sender.SendTo(stream, presence);
                    }
                }
                else
                {
                    //Doesn't exists
                    MucRoomMember newMember = new MucRoomMember(Room, presence.To, presence.From, stream, context);
                    Room.TryEnterRoom(newMember, presence);
                }
            }
            else
            {
                ErrorPresence(presence, ErrorCondition.BadRequest);
                context.Sender.SendTo(stream, presence);
            }
        }

        private static void ErrorPresence(Presence presence, ErrorCondition condition)
        {
            presence.SwitchDirection();
            presence.RemoveAllChildNodes();
            presence.AddChild(new Muc());
            presence.Type = PresenceType.error;
            presence.Error = new Error(condition);
        }

        public override void HandleMessage(Streams.XmppStream stream, Message msg, XmppHandlerContext context)
        {
            User user = (User)msg.SelectSingleElement(typeof(User));
            if (user != null)
            {
                HandleUserMessage(msg, user, stream);
            }
            else
            {
                //Groupchat message
                MucRoomMember member = Room.GetRealMember(msg.From);
                if (member != null && ReferenceEquals(member.Stream, stream) && member.Role != Role.none)
                {
                    if (msg.Type == MessageType.groupchat)
                    {
                        if (msg.Subject != null)
                        {
                            Room.ChangeSubject(member, msg.Subject);
                        }
                        else
                        {
                            MessageBroadcast(msg, member);
                        }
                    }
                    else
                    {
                        msg.SwitchDirection();
                        msg.Type = MessageType.error;
                        msg.Error = new Error(ErrorCondition.NotAcceptable);
                        context.Sender.SendTo(stream, msg);
                    }
                }
                else
                {
                    msg.SwitchDirection();
                    msg.Type = MessageType.error;
                    msg.Error = new Error(ErrorCondition.Forbidden);
                    context.Sender.SendTo(stream, msg);
                }
            }

        }

        private void HandleUserMessage(Message msg, User user, XmppStream stream)
        {
            if (user.Invite != null)
            {
                Room.InviteUser(msg, user, stream);
            }
            else if (user.Decline != null)
            {
                Room.DeclinedUser(msg, user, stream);
            }
        }



        private void MessageBroadcast(Message msg, MucRoomMember member)
        {
            Room.BroadcastMessage(msg, member);
        }
    }
}