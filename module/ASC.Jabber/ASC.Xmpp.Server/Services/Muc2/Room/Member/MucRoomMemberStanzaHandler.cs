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