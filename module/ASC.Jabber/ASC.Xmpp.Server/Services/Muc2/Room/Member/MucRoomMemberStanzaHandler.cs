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


using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;

namespace ASC.Xmpp.Server.Services.Muc2.Room.Member
{
    using Handler;

    [XmppHandler(typeof(Stanza))]
    internal class MucRoomMemberStanzaHandler : XmppStanzaHandler
    {
        public MucRoomMember Member { get; set; }

        internal MucRoomMemberStanzaHandler(MucRoomMember member)
        {
            Member = member;
        }

        public override IQ HandleIQ(ASC.Xmpp.Server.Streams.XmppStream stream, IQ iq, XmppHandlerContext context)
        {
            if (iq.Vcard!=null && iq.Type==IqType.get)
            {
                //Handle vcard
                iq.Vcard = Member.GetVcard();
                iq.Type = IqType.result;
                iq.SwitchDirection();
                return iq;
            }
            return base.HandleIQ(stream, iq, context);
        }

        public override void HandlePresence(Streams.XmppStream stream, Presence presence, XmppHandlerContext context)
        {
            if (presence.Type == PresenceType.available || presence.Type == PresenceType.unavailable)
            {
                if (!ReferenceEquals(Member.Stream, stream))
                {
                    //Set stream
                    Member.Stream = stream;
                    if (presence.Type == PresenceType.available)
                    {
                        //If stream changed then we should broadcast presences
                        Member.ReEnterRoom();
                    }
                }
                Member.ChangePesence(presence);
            }
            else
            {
                //Bad request                
                presence.SwitchDirection();
                presence.From = Member.RoomFrom;
                presence.Type = PresenceType.error;
                presence.Error = new Error(ErrorCondition.BadRequest);
                context.Sender.SendTo(stream, presence);
            }
        }

        public override void HandleMessage(Streams.XmppStream stream, Message msg, XmppHandlerContext context)
        {
            //Private msg
            if (msg.Type==MessageType.chat)
            {
                if (Member.ResolveRoomJid(msg.From)==null)
                {
                    //Error
                    msg.SwitchDirection();
                    msg.From = Member.RoomFrom;
                    msg.Type = MessageType.error;
                    msg.Error = new Error(ErrorCondition.ItemNotFound);
                    context.Sender.SendTo(stream, msg);
                }
                else
                {
                    //Send
                    msg.To = Member.RealJid;
                    msg.From = Member.ResolveRoomJid(msg.From);
                    Member.Send(msg);
                }

            }
            else
            {
                msg.SwitchDirection();
                msg.From = Member.RoomFrom;
                msg.Type = MessageType.error;
                msg.Error = new Error(ErrorCondition.BadRequest);
                context.Sender.SendTo(stream, msg);
            }
        }
    }
}