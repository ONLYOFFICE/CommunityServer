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

using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.extensions.ping;
using ASC.Xmpp.Core.protocol.iq.time;
using ASC.Xmpp.Core.protocol.iq.version;
using ASC.Xmpp.Server.Gateway;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;
using ASC.Xmpp.Server.Utils;

namespace ASC.Xmpp.Server.Services.Jabber
{
    [XmppHandler(typeof(Version))]
    [XmppHandler(typeof(EntityTime))]
    [XmppHandler(typeof(Ping))]
    class VerTimePingHandler : XmppStanzaHandler
    {
        public override IQ HandleIQ(XmppStream stream, IQ iq, XmppHandlerContext context)
        {
            var answer = new IQ(IqType.result)
            {
                Id = iq.Id,
                To = iq.From,
                From = iq.To,
            };

            //iq sended to server
            if (iq.Type == IqType.get && (!iq.HasTo || iq.To.IsServer || iq.To == iq.From))
            {
                if (iq.Query is Version)
                {
                    answer.Query = new Version()
                    {
                        Name = "TeamLab Jabber Server",
                        Os = System.Environment.OSVersion.ToString(),
                        Ver = "1.0",
                    };
                    return answer;
                }
                else if (iq.Query is Ping)
                {
                    return answer;
                }
                return XmppStanzaError.ToServiceUnavailable(iq);
            }

            if (iq.Type == IqType.get && iq.HasTo)
            {
                //resend iq
                var sessionTo = context.SessionManager.GetSession(iq.To);
                var sessionFrom = context.SessionManager.GetSession(iq.From);
                if (sessionTo != null && sessionFrom != null)
                {
                    if (string.IsNullOrEmpty(iq.Id))
                    {
                        iq.Id = System.Guid.NewGuid().ToString("N");
                    }

                    IdleWatcher.StartWatch(
                        iq.Id + iq.From,
                        System.TimeSpan.FromSeconds(3),
                        (s, e) => { context.Sender.SendTo(sessionFrom, XmppStanzaError.ToServiceUnavailable(iq)); });
                    context.Sender.SendTo(sessionTo, iq);
                }
                else
                {
                    return XmppStanzaError.ToRecipientUnavailable(iq);
                }
            }
            if (iq.Type == IqType.error || iq.Type == IqType.result)
            {
                if (!iq.HasTo)
                {
                    return XmppStanzaError.ToBadRequest(iq);
                }

                IdleWatcher.StopWatch(iq.Id + iq.To);
                var session = context.SessionManager.GetSession(iq.To);
                if (session != null)
                {
                    context.Sender.SendTo(session, iq);
                }
            }
            return null;
        }
    }
}