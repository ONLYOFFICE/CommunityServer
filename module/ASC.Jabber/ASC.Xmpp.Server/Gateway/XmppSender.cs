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

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using ASC.Xmpp.Core.utils.Xml.Dom;
using ASC.Xmpp.Server.Handler;
using ASC.Xmpp.Server.Session;
using ASC.Xmpp.Server.Streams;
using log4net;
using Uri = ASC.Xmpp.Core.protocol.Uri;
using ASC.Xmpp.Core.protocol.client;

namespace ASC.Xmpp.Server.Gateway
{
    class XmppSender : IXmppSender
    {
        private XmppGateway gateway;

        private static readonly ILog _log = LogManager.GetLogger("ASC.Xmpp.Server.Messages");
        private static readonly ReverseJabberServiceClient _reverseJabberServiceClient = new ReverseJabberServiceClient();

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
                _log.DebugFormat(SEND_FORMAT, to.ConnectionId, to.Namespace, node.ToString(Formatting.Indented));
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
                _log.DebugFormat(SEND_FORMAT, to.ConnectionId, to.Namespace, text);
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
                    var session = sessionManager.GetAvailableSession(presence.From.BareJid);
                    if (session != null && session.Presence != null)
                    {
                        _reverseJabberServiceClient.SendState(session.Presence.From.User.ToLowerInvariant(),
                            SignalRHelper.GetState(session.Presence.Show, session.Presence.Type), -1, session.Presence.From.Server);
                        return;
                    }
                }
                _reverseJabberServiceClient.SendState(presence.From.User.ToLowerInvariant(), state, -1, presence.From.Server);
            }
            catch (Exception e)
            {
                _log.ErrorFormat("Can not send to {0} to SignalR: {1} {2}", presence, e.Message, e.StackTrace);
            }
        }

        #endregion
    }
}