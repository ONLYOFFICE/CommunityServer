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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Linq;
using ASC.Api.Client;
using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.SignalR.Base.Hubs.OnlineUsers;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json.Linq;
using log4net;

namespace ASC.SignalR.Base.Hubs.Voip
{
    [AuthorizeHub]
    [HubName("voip")]
    public class VoipHub : Hub
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(VoipHub));

        private static readonly ConcurrentDictionary<string, Queue<string>> Calls = new ConcurrentDictionary<string, Queue<string>>();
        private static readonly ConcurrentDictionary<string, Queue<string>> Agents = new ConcurrentDictionary<string, Queue<string>>();

        private static readonly ConcurrentDictionary<string, CancelableTask> agentOfflineInspectors = new ConcurrentDictionary<string, CancelableTask>();

        public override Task OnConnected()
        {
            Log.Debug("OnConnected");

            if (string.IsNullOrEmpty(Context.Request.Headers["voipConnection"]))
            {
                Groups.Add(Context.ConnectionId, DictionaryKey);
            }

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            Log.Debug("OnDisconnected");

            var user = Context.Request.Environment["server.User"] as GenericPrincipal;
            if (user == null)
            {
                AuthorizeHubAttribute.Authorize(Context.Request);
            }

            if (string.IsNullOrEmpty(Context.Request.Headers["voipConnection"]))
            {
                AddAgentOfflineInspector();
                try
                {
                    Groups.Remove(Context.ConnectionId, DictionaryKey);
                }
                catch(Exception e)
                {
                    Log.Error(e);
                }
            }

            return base.OnDisconnected(stopCalled);
        }

        private void AddAgentOfflineInspector()
        {
            var inspector = UsersHub.GetUserOfflineInspector(ClientTenantId, ClientUserId);
            inspector.Task.ContinueWith(task =>
                {
                    if (!task.Result) return;
                    Status(2);
                });

            agentOfflineInspectors.AddOrUpdate(ClientUserId, inspector,
                                               (user, oldInspector) =>
                                                   {
                                                       oldInspector.CancellationTokenSource.Cancel();
                                                       return inspector;
                                                   });

            inspector.Task.Start();
        }

        [HubMethodName("miss")]
        public void MissCall(string callId)
        {
            try
            {
                Log.Debug("miss");

                var call = GetCall(callId);

                RemoveFromQueue(Calls, call);

                var agent = JObject.Parse(call)["answeredBy"]["id"].ToString();
                ChangeStatus(0, agent);

                Clients.User(agent).miss(call);
            }
            catch(Exception e)
            {
                Log.Error("miss", e);
            }
        }

        [HubMethodName("mail")]
        public void VoiceMail(string callId)
        {
            try
            {
                Log.Debug("voiceMail");

                var call = GetCall(callId);

                RemoveFromQueue(Calls, call);

                var agent = JObject.Parse(call)["answeredBy"]["id"].ToString();
                ChangeStatus(0, agent);

                Clients.User(agent).mail(call);
            }
            catch(Exception e)
            {
                Log.Error("voiceMail", e);
            }
        }

        [HubMethodName("status")]
        public void Status(int status)
        {
            ChangeStatus(status, ClientUserId);
        }

        private void ChangeStatus(int status, string userId)
        {
            try
            {
                Log.Debug("status:" + status);

                Clients.User(userId).status(status);

                ChangeApiStatus(status, userId);

                var calls = Calls.GetOrAdd(DictionaryKey, new Queue<string>());
                var agents = Agents.GetOrAdd(DictionaryKey, new Queue<string>());

                if (status == 0)
                {
                    if (calls.Any())
                    {
                        Dequeue();
                    }
                    else
                    {
                        agents.Enqueue(userId);
                        Log.Debug("Agents:" + string.Join(",", agents.Select(r => r)));
                    }
                }
                else
                {
                    RemoveFromQueue(Agents, userId);
                    Log.Debug("Agents:" + string.Join(",", agents.Select(r => r)));
                }

                OnlineAgents();
            }
            catch(Exception e)
            {
                Log.Error("Status", e);
            }
        }

        [HubMethodName("enqueue")]
        public void Enqueue(string callId)
        {
            try
            {
                Log.Debug("Enqueue");

                var call = GetCall(callId);
                var calls = Calls.GetOrAdd(DictionaryKey, new Queue<string>());
                var agents = Agents.GetOrAdd(DictionaryKey, new Queue<string>());

                if (agents.Any())
                {
                    AnswerCall(call, agents.Dequeue());
                    Log.Debug("Agents:" + string.Join(",", agents.Select(r => r)));
                }
                else
                {
                    Log.Debug("Calls:");
                    calls.Enqueue(call);
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
                var calls = Calls.GetOrAdd(DictionaryKey, new Queue<string>());

                if (!calls.Any()) return;

                AnswerCall(calls.Dequeue(), ClientUserId);
            }
            catch(Exception e)
            {
                Log.Error("Dequeue", e);
            }
        }

        [HubMethodName("OnlineAgents")]
        public void OnlineAgents()
        {
            var agents = Agents.GetOrAdd(DictionaryKey, new Queue<string>());

            var userAccount = GetUserAccount();
            if (userAccount != null)
            {
                Clients.Group(DictionaryKey).onlineAgents(agents.Select(r => r));
            }
        }

        [HubMethodName("Incoming")]
        public void Incoming(string callId, string agent)
        {
            AnswerCall(GetCall(callId), agent);
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

        private void AnswerCall(string call, string agent)
        {
            ChangeStatus(1, agent);
            Clients.User(agent).dequeue(call);
            SaveCall(JObject.Parse(call)["id"].Value<string>(), agent, 2);
        }

        private string clientUserId;

        private string ClientUserId
        {
            get { return !string.IsNullOrEmpty(clientUserId) ? clientUserId : (clientUserId = new CustomUserIdProvider().GetUserId(Context.Request)); }
        }

        private int ClientTenantId
        {
            get
            {
                var account = GetUserAccount();
                if (account != null) return account.Tenant;

                throw new InvalidOperationException();
            }
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

        private void RemoveFromQueue(ConcurrentDictionary<string, Queue<string>> dict, string queueItem)
        {
            dict.AddOrUpdate(DictionaryKey, new Queue<string>(), (s, queue) => new Queue<string>(queue.Where(r => r != queueItem)));
        }

        private string dictionaryKey;

        private string DictionaryKey
        {
            get
            {
                if (string.IsNullOrEmpty(dictionaryKey))
                {
                    var userAccount = GetUserAccount();
                    if (userAccount != null)
                        dictionaryKey = userAccount.Tenant.ToString(CultureInfo.InvariantCulture);

                    dictionaryKey += Context.Request.QueryString["numberId"];
                    Log.Debug("dictionaryKey:" + dictionaryKey);
                }

                return dictionaryKey;
            }
        }

        #region Api

        private string GetCall(string callId)
        {
            var userAccount = GetUserAccount();
            if (userAccount == null) return null;

            var tenant = CoreContext.TenantManager.GetTenant(userAccount.Tenant);
            var cookie = (string)Context.Request.Environment["server.UserCookie"];

            var apiClient = new ApiClient(tenant.GetTenantDomain());
            var request = new ApiRequest(string.Format("crm/voip/call/{0}", callId), cookie)
                {
                    Method = HttpMethod.Get,
                    ResponseType = ResponseType.Json
                };

            return apiClient.GetResponse(request).Response;
        }

        private void SaveCall(string callId, string answeredBy, int status)
        {
            var userAccount = GetUserAccount();
            if (userAccount == null) return;

            var tenant = CoreContext.TenantManager.GetTenant(userAccount.Tenant);
            var cookie = (string)Context.Request.Environment["server.UserCookie"];

            var apiClient = new ApiClient(tenant.GetTenantDomain());
            var request = new ApiRequest(string.Format("crm/voip/call/{0}", callId), cookie)
                {
                    Method = HttpMethod.Post,
                    ResponseType = ResponseType.Json
                };

            request.Parameters.Add(new RequestParameter {Name = "answeredBy", Value = answeredBy});
            request.Parameters.Add(new RequestParameter {Name = "status", Value = status});

            apiClient.GetResponse(request);
        }

        private void ChangeApiStatus(int status, string userId)
        {
            try
            {
                var userAccount = GetUserAccount();
                if (userAccount == null) return;

                var tenant = CoreContext.TenantManager.GetTenant(userAccount.Tenant);
                var cookie = (string)Context.Request.Environment["server.UserCookie"];

                var apiClient = new ApiClient(tenant.GetTenantDomain());
                var request = new ApiRequest(string.Format("crm/voip/opers/{0}", userId), cookie)
                    {
                        Method = HttpMethod.Put
                    };
                request.Parameters.Add(new RequestParameter {Name = "status", Value = status});
                apiClient.GetResponse(request);
            }
            catch(ApiErrorException e)
            {
                Log.ErrorFormat("ChangeApiStatus userId: {0}, ErrorStackTrace: {1}, ErrorMessage:{2}", userId, e.ErrorStackTrace, e.ErrorMessage);
                throw;
            }
            catch(Exception e)
            {
                Log.Error("ChangeApiStatus userId:" + userId, e);
                throw;
            }
        }

        #endregion
    }
}