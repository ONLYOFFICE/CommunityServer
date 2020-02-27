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


using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ASC.Common.Logging;
using ASC.Core.Notify.Signalr;
using ASC.Xmpp.Core.protocol.client;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;

using Uri = ASC.Xmpp.Core.protocol.Uri;

namespace ASC.Xmpp.Server.Gateway
{
    class XmppSender : IXmppSender
    {
        private readonly XmppGateway gateway;

        private static readonly ILog _log = LogManager.GetLogger("ASC.Xmpp.Server.Messages");
        private static readonly SignalrServiceClient SignalrServiceClient = new SignalrServiceClient("chat");

        private const string SEND_FORMAT = "Xmpp stream: connection {0}, namespace {1}\r\n\r\n(S) -------------------------------------->>\r\n{2}\r\n";

        public XmppSender(XmppGateway gateway)
        {
            if (gateway == null) throw new ArgumentNullException("gateway");

            this.gateway = gateway;
        }

        #region IXmppSender Members

        public void SendTo(XmppSession to, Node node)
        {
            if (to == null) throw new ArgumentNullException("to");
            SendTo(to.Stream, node);
        }

        public void SendTo(XmppStream to, Node node)
        {
            if (to == null) throw new ArgumentNullException("to");
            if (node == null) throw new ArgumentNullException("node");

            var connection = GetXmppConnection(to.ConnectionId);
            if (connection != null)
            {
                _log.Trace(string.Format(SEND_FORMAT, to.ConnectionId, to.Namespace, node.ToString(Formatting.Indented)));
                connection.Send(node, Encoding.UTF8);
            }
        }

        public void SendTo(XmppStream to, string text)
        {
            if (to == null) throw new ArgumentNullException("to");
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException("text");

            var connection = GetXmppConnection(to.ConnectionId);
            if (connection != null)
            {
                _log.Trace(string.Format(SEND_FORMAT, to.ConnectionId, to.Namespace, text));
                connection.Send(text, Encoding.UTF8);
            }
        }

        public void CloseStream(XmppStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var connection = GetXmppConnection(stream.ConnectionId);
            if (connection != null)
            {
                connection.Close();
            }
        }

        public void SendToAndClose(XmppStream to, Node node)
        {
            try
            {
                SendTo(to, node);
                SendTo(to, string.Format("</stream:{0}>", Uri.PREFIX));
            }
            finally
            {
                CloseStream(to);
            }
        }

        public void ResetStream(XmppStream stream)
        {
            if (stream == null) throw new ArgumentNullException("stream");

            var connection = GetXmppConnection(stream.ConnectionId);
            if (connection != null)
            {
                connection.Reset();
            }
        }

        public IXmppConnection GetXmppConnection(string connectionId)
        {
            return gateway.GetXmppConnection(connectionId);
        }

        public bool Broadcast(ICollection<XmppSession> sessions, Node node)
        {
            if (sessions == null) throw new ArgumentNullException("sessions");
            foreach (var session in sessions)
            {
                if (!session.IsSignalRFake)
                {
                    try
                    {
                        SendTo(session, node);
                    }
                    catch (Exception ex)
                    {
                        if (ex is IOException || ex is ObjectDisposedException)
                        {
                            // ignore
                        }
                        else
                        {
                            _log.ErrorFormat("Can not send to {0} in broadcast: {1}", session, ex);
                        }
                    }
                }
            }
            return 0 < sessions.Count;
        }

        public void SendPresenceToSignalR(Presence presence, XmppSessionManager sessionManager) 
        {
            try
            {
                var state = SignalRHelper.GetState(presence.Show, presence.Type);
                if (state == SignalRHelper.USER_OFFLINE && sessionManager != null)
                {
                    //sessionManager.CloseSession(presence.From.BareJid);
                }
                SignalrServiceClient.SendState(presence.From.User.ToLowerInvariant(), state, -1, presence.From.Server);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can not send to {0} to SignalR: {1} {2}", presence, e.Message, e.StackTrace);
            }
        }

        #endregion
    }
}