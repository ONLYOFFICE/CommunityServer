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


using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.protocol.extensions.ping;
using ASC.Xmpp.Core.protocol.iq.time;
using ASC.Xmpp.Core.protocol.iq.version;
using ASC.Xmpp.Server.Handler;
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
                if (iq.GetTag(typeof(Version)) != null)
                {
                    answer.Query = new Version()
                    {
                        Name = "OnlyOffice Jabber Server",
                        Os = System.Environment.OSVersion.ToString(),
                        Ver = "1.0",
                    };
                    return answer;
                }
                else if (iq.GetTag(typeof(Ping)) != null)
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