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

using ASC.Xmpp.Core;
using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x.muc.iq;
using Error = ASC.Xmpp.Core.protocol.client.Error;

namespace ASC.Xmpp.Server.Services.Muc2
{
    using ASC.Xmpp.Server.Streams;
    using Handler;
    using Helpers;
    using Room.Settings;
    using Utils;

    [XmppHandler(typeof(Stanza))]
    internal class MucStanzaHandler : XmppStanzaHandler
    {
        public MucService Service { get; set; }

        internal MucStanzaHandler(MucService service)
        {
            Service = service;
        }

        public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
        {
            Unique unique = (Unique)iq.SelectSingleElement(typeof(Unique));
            if (unique != null)
            {
                // Gen unique id
                unique.Value = UniqueId.CreateNewId(16);
                iq.Type = IqType.result;
                iq.SwitchDirection();
                return iq;
            }
            iq.SwitchDirection();
            iq.Type = IqType.error;
            iq.Error = new Error(ErrorType.cancel, ErrorCondition.ItemNotFound);
            return iq;
        }

        public override void HandlePresence(XmppStream stream, Presence presence, XmppHandlerContext context)
        {
            //Presence to open new room
            if (MucHelpers.IsJoinRequest(presence))
            {
                //Register
                Service.CreateRoom(new Jid(presence.To.Bare), null);
                Service.HandlerManager.ProcessStreamElement(presence, stream);//Forward to room
            }
            else
            {
                //Return error
                presence.Type = PresenceType.error;
                presence.Error = new Error(ErrorType.cancel, ErrorCondition.NotAllowed);
                presence.SwitchDirection();
                context.Sender.SendTo(stream, presence);
            }
        }

        public override void HandleMessage(XmppStream stream, Message msg, XmppHandlerContext context)
        {
            msg.SwitchDirection();
            msg.Type = MessageType.error;
            msg.Error = new Error(ErrorType.cancel, ErrorCondition.ItemNotFound);
            context.Sender.SendTo(stream, msg);
        }
    }
}