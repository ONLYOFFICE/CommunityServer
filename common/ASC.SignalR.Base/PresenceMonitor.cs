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

using ASC.Core;
using ASC.SignalR.Base.Hubs.Chat;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Transports;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ASC.SignalR.Base
{
    public class PresenceMonitor
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(PresenceMonitor));
        private readonly static IHubContext context = GlobalHost.ConnectionManager.GetHubContext<Chat>();
        private readonly ITransportHeartbeat heartbeat;
        private Timer _timer;

        private readonly TimeSpan presenceCheckInterval = TimeSpan.FromSeconds(20);


        public PresenceMonitor(ITransportHeartbeat heartbeat)
        {
            this.heartbeat = heartbeat;
        }

        public void StartMonitoring()
        {
            if (_timer == null)
            {
                _timer = new Timer(_ =>
                {
                    try
                    {
                        Check();
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError(e.Message);
                        log.Error(e);
                    }
                },
                null,
                TimeSpan.Zero,
                presenceCheckInterval);
            }
        }

        private void Check()
        {
            foreach (var trackedConnection in heartbeat.GetConnections().Where(trackedConnection => !trackedConnection.IsAlive))
            {
                log.DebugFormat("Find zombie connection. Connection id:{0}", trackedConnection.ConnectionId);
                int tenant;
                string userName = Chat.Connections.GetUserNameByConnectionId(trackedConnection.ConnectionId, out tenant);
                if (!string.IsNullOrEmpty(userName))
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenant);
                    var userInfo = CoreContext.UserManager.GetUserByUserName(userName);
                    context.Groups.Remove(trackedConnection.ConnectionId, userInfo.Tenant + userName);
                    context.Groups.Remove(trackedConnection.ConnectionId, userInfo.Tenant.ToString(CultureInfo.InvariantCulture));
                    Disconnect(userInfo.Tenant, userName, userInfo.ID.ToString(), trackedConnection.ConnectionId);
                }
            }
        }

        private void Disconnect(int tenant, string userName, string userId, string connectionId)
        {
            context.Groups.Remove(connectionId, tenant + userName);
            context.Groups.Remove(connectionId, tenant.ToString(CultureInfo.InvariantCulture));
            var connectionsCount = Chat.Connections.Remove(tenant, userName, connectionId);

            if (connectionsCount == 0)
            {
                try
                {
                    if (Chat.JabberServiceClient.RemoveXmppConnection(userId, userName, tenant))
                    {
                        Chat.States.Add(tenant, userName, Chat.UserOffline);
                        // setState
                        context.Clients.Group(tenant.ToString(CultureInfo.InvariantCulture)).ss(userName, Chat.UserOffline, false);
                    }
                }
                catch (Exception e)
                {
                    Chat.States.Add(tenant, userName, Chat.UserOffline);
                    // setState
                    context.Clients.Group(tenant.ToString(CultureInfo.InvariantCulture)).ss(userName, Chat.UserOffline, false);
                    log.ErrorFormat("Error on removing of Jabber connection. Username={0}. {1} {2} {3}",
                        userName, e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
                }
            }
        }
    }
}