/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using System.Security;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.SignalR.Base.Hubs.Chat;
using ASC.VoipService;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using log4net;

namespace ASC.SignalR.Base.Hubs.Voip
{
    [AuthorizeHub]
    [HubName("voip")]
    public class VoipHub : Hub
    {
        private static readonly ILog Log;
        const string NumberIdKey = "numberId";
        private static readonly ListPhones Phones;
        public static readonly ConnectionMapping Connections;

        static VoipHub()
        {
            Log = LogManager.GetLogger("ASC.Voip");
            Phones = new ListPhones(Log);
            Connections = new ConnectionMapping();
        }

        public string GetNumberId()
        {
            var numberId = Context.Request.QueryString[NumberIdKey];

            if (string.IsNullOrEmpty(numberId))
            {
                numberId = Context.Request.Headers[NumberIdKey];
            }

            return numberId;
        }

        public override Task OnConnected()
        {
            Log.Debug("OnConnected:" + Context.ConnectionId);

            if (string.IsNullOrEmpty(Context.Request.Headers["voipConnection"]))
            {
                Groups.Add(Context.ConnectionId, DictionaryKey);
            }

            Phones.AddPhone(GetNumberId());

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            Status(AgentStatus.Offline);
            return base.OnReconnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Log.Debug("OnDisconnected:" + Context.ConnectionId);

            var user = Context.Request.Environment["server.User"] as GenericPrincipal;
            if (user == null)
            {
                AuthorizeHubAttribute.Authorize(Context.Request);
            }

            if (string.IsNullOrEmpty(Context.Request.Headers["voipConnection"]))
            {
                if (user != null)
                {
                    var userAccount = user.Identity as IUserAccount;
                    if (userAccount != null)
                    {
                        bool result;
                        var connectionCount = Connections.Remove(userAccount.Tenant, userAccount.ID.ToString(), Context.ConnectionId, out result);
                        if (connectionCount == 0)
                        {
                            Status(AgentStatus.Offline);
                        }
                    }
                }
                try
                {
                    Groups.Remove(Context.ConnectionId, DictionaryKey);
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        [HubMethodName("miss")]
        public void MissCall(string callId, string agent)
        {
            try
            {
                Log.Debug("miss");

                Phones.RemoveCall(GetNumberId(), callId);

                Clients.User(agent).miss(callId);
            }
            catch(Exception e)
            {
                Log.Error("miss", e);
            }
        }

        [HubMethodName("status")]
        public void Status(AgentStatus status)
        {
            try
            {
                var user = Context.Request.Environment["server.User"] as GenericPrincipal;
                if (user == null) return;

                var userAccount = user.Identity as IUserAccount;
                if (userAccount != null)
                {
                    CoreContext.TenantManager.SetCurrentTenant(userAccount.Tenant);
                }
                else
                {
                    Log.ErrorFormat("Unknown request without user.Identity as IUserAccount, url={0}",
                        Context.Request.Url);
                    throw new SecurityException("Unknown request without user.Identity as IUserAccount");
                }

                var userId = ClientUserId;

                Log.Debug("status:" + status);

                Clients.User(userId).status(status);

                switch (status)
                {
                    case AgentStatus.Online:
                        if (Phones.AnyCalls(GetNumberId()))
                        {
                            Dequeue();
                        }
                        else
                        {
                            Phones.AddOrUpdateAgent(GetNumberId(), new Agent {Id = Guid.Parse(userId), Status = status});
                            Connections.Add(userAccount.Tenant, userAccount.ID.ToString(), Context.ConnectionId);
                        }
                        break;
                    case AgentStatus.Paused:
                        Phones.AddOrUpdateAgent(GetNumberId(), new Agent { Id = Guid.Parse(userId), Status = status });
                        break;
                    case AgentStatus.Offline:
                        Phones.RemoveAgent(GetNumberId(), Guid.Parse(userId));

                        bool result;
                        Connections.Remove(userAccount.Tenant, userAccount.ID.ToString(), Context.ConnectionId, out result);
                        break;
                }

                OnlineAgents();
            }
            catch(Exception e)
            {
                Log.Error("Status", e);
            }
        }

        [HubMethodName("enqueue")]
        public void Enqueue(string callId, string agent)
        {
            try
            {
                Log.Debug("Enqueue");

                var result = Phones.Enqueue(GetNumberId(), callId, agent);
                if (!string.IsNullOrEmpty(result))
                {
                    Clients.User(result).dequeue(callId);
                }
            }
            catch(Exception e)
            {
                Log.Error("Enqueue", e);
            }
        }

        [HubMethodName("Dequeue")]
        public void Dequeue()
        {
            try
            {
                Log.Debug("Dequeue");

                Clients.User(ClientUserId).dequeue(Phones.DequeueCall(GetNumberId()));
            }
            catch(Exception e)
            {
                Log.Error("Dequeue", e);
            }
        }

        [HubMethodName("OnlineAgents")]
        public void OnlineAgents()
        {
            var userAccount = GetUserAccount();
            if (userAccount != null)
            {
                Clients.Group(DictionaryKey).onlineAgents(Phones.OnlineAgents(GetNumberId()));
            }
        }

        [HubMethodName("getStatus")]
        public int GetStatus()
        {
            var userAccount = GetUserAccount();
            if (userAccount != null)
            {
                return (int) Phones.GetStatus(GetNumberId(), ClientUserId);
            }

            return (int)AgentStatus.Offline;
        }

        [HubMethodName("GetAgent")]
        public Tuple<Agent, bool> GetAgent(List<Guid> contactsResponsibles)
        {
            var userAccount = GetUserAccount();
            if (userAccount != null)
            {
                return Phones.GetAgent(GetNumberId(), contactsResponsibles);
            }

            return null;
        }

        [HubMethodName("Incoming")]
        public void Incoming(string callId, string agent)
        {
            Clients.User(agent).dequeue(callId);
        }

        [HubMethodName("Start")]
        public void Start()
        {
            Clients.User(ClientUserId).start();
        }

        [HubMethodName("End")]
        public void End()
        {
            Clients.User(ClientUserId).end();
        }

        private string clientUserId;

        private string ClientUserId
        {
            get { return !string.IsNullOrEmpty(clientUserId) ? clientUserId : (clientUserId = new CustomUserIdProvider().GetUserId(Context.Request)); }
        }

        private IUserAccount GetUserAccount()
        {
            if (Context.User != null)
            {
                var userAccount = Context.User.Identity as IUserAccount;
                if (userAccount != null) return userAccount;
            }

            var user = Context.Request.Environment["server.User"] as GenericPrincipal ?? Context.Request.GetHttpContext().User as GenericPrincipal;
            if (user != null) return user.Identity as IUserAccount;

            return null;
        }

        private string dictionaryKey;

        private string DictionaryKey
        {
            get
            {
                if (!string.IsNullOrEmpty(dictionaryKey)) return dictionaryKey;

                var userAccount = GetUserAccount();
                if (userAccount != null)
                    dictionaryKey = userAccount.Tenant.ToString(CultureInfo.InvariantCulture);

                dictionaryKey += GetNumberId();
                Log.Debug("dictionaryKey:" + dictionaryKey);

                return dictionaryKey;
            }
        }
    }
}