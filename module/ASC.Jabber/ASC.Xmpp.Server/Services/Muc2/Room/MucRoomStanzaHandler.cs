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

using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x.muc;
using ASC.Xmpp.Core.protocol.x.muc.iq.admin;
using ASC.Xmpp.Core.protocol.x.muc.iq.owner;

namespace ASC.Xmpp.Server.Services.Muc2.Room
{
    using System;
    using Handler;
    using Helpers;
    using Member;
    using Streams;
    using Utils;

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
                if (member!=null)
                {
                    if (iq.Query!=null)
                    {
                        if (iq.Query is Admin && (member.Affiliation==Affiliation.admin || member.Affiliation==Affiliation.owner))
                        {
                            Room.AdminCommand(iq, member);
                        }
                        else if (iq.Query is Owner && (member.Affiliation == Affiliation.owner))
                        {
                            Room.OwnerCommand(iq, member);
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

        private static void ErrorPresence(Presence presence, ErrorCondition condition) {
            presence.SwitchDirection();
            presence.RemoveAllChildNodes();
            presence.AddChild(new Muc());
            presence.Type = PresenceType.error;
            presence.Error = new Error(condition);
        }

        public override void HandleMessage(Streams.XmppStream stream, Message msg, XmppHandlerContext context)
        {
            User user = (User) msg.SelectSingleElement(typeof (User));
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
                        if (msg.Subject!=null)
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
                Room.InviteUser(msg, user,stream);
            }
            else if (user.Decline != null)
            {
                Room.DeclinedUser(msg, user,stream);
            }
        }

        

        private void MessageBroadcast(Message msg, MucRoomMember member)
        {
            Room.BroadcastMessage(msg, member);
        }
    }
}