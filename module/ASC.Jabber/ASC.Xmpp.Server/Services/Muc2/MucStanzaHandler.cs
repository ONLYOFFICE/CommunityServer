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


using ASC.Xmpp.Core.protocol;
using ASC.Xmpp.Core.protocol.Base;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.x.muc.iq;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Services.Muc2.Helpers;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;
using Error = ASC.Xmpp.Core.protocol.client.Error;

namespace ASC.Xmpp.Server.Services.Muc2
{
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