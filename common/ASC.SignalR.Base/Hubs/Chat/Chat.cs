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

using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core.Jabber;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;

namespace ASC.SignalR.Base.Hubs.Chat
{
    [AuthorizeHub]
    [HubName("c")]
    public class Chat : Hub
    {
        public readonly static ConnectionMapping Connections = new ConnectionMapping();
        public readonly static UserStateMapping States = new UserStateMapping();
        public readonly static JabberServiceClient JabberServiceClient = new JabberServiceClient();
        private readonly static ILog ChatLog = LogManager.GetLogger(typeof(Chat));
        private readonly static object FirstChatLoadingSyncRoot = new object();
        private volatile static int allConnectionsCount;
        private volatile static bool firstChatLoading = true;
        private const string websockets = "webSockets";
        private const string transport = "transport";
        private const string stateNumber = "State";
        private const string soundPath = "usercontrols/common/smallchat/css/sounds/chat";
        private const byte userOnline = 1;
        public const byte UserOffline = 4;
        public const byte TraceError = 0;
        public const byte TraceDebug = 1;

        

        public override Task OnDisconnected()
        {
            var user = Context.Request.Environment["server.User"] as GenericPrincipal;

            if (user == null)
            {
                AuthorizeHubAttribute.Authorize(Context.Request);
            }
            try
            {
                DisconnectUser();
            }
            catch (Exception)
            {

            }
            return base.OnDisconnected();
        }

        // Method for JS-clients

        [HubMethodName("s")]
        public void Send(string calleeUserName, string messageText)
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUser = CoreContext.UserManager.GetUsers(user.ID);
            if (calleeUserName != string.Empty && CoreContext.UserManager.GetUserByUserName(calleeUserName).Equals(Core.Users.Constants.LostUser))
            {
                TraceMessage(TraceError, String.Format("Can't get UserInfo by calleeUserName={0}, TenantId={1}.", calleeUserName, currentUser.Tenant));
                throw new HubException();
            }
            TraceMessage(TraceDebug, String.Format("Send: calleeUserName={0}, messageText={1}", calleeUserName, messageText));
            var callerUserName = currentUser.UserName.ToLowerInvariant();
            var message = new MessageClass
            {
                UserName = callerUserName,
                Text = messageText
            };
            if (calleeUserName != string.Empty)
            {
                // send
                Clients.Group(currentUser.Tenant + calleeUserName).s(message, calleeUserName);
                // send
                Clients.OthersInGroup(currentUser.Tenant + callerUserName).s(message, calleeUserName);
            }
            try
            {
                JabberServiceClient.SendMessage(currentUser.Tenant, callerUserName, calleeUserName, messageText);
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on sending message to Jabber service. CallerUserName={0}, TenantId={1}. {2}",
                    callerUserName, currentUser.Tenant, e));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("gs")]
        public void GetStates()
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUserInfo = CoreContext.UserManager.GetUsers(user.ID);
            var currentUserName = currentUserInfo.UserName.ToLowerInvariant();
            TraceMessage(TraceDebug, String.Format("Get States currentUserName={0}", currentUserName));
            lock (States.SyncRoot)
            {
                GetStatesFromJabber();
                // statesRetrieved
                Clients.Caller.sr(States.GetStatesOfTenant(currentUserInfo.Tenant).
                    Where(s => s.Key != currentUserName && s.Value != UserOffline).ToDictionary(s => s.Key, s => s.Value));
            }
        }

        [HubMethodName("gci")]
        public Tuple<string, byte> GetContactInfo(string userName)
        {
            var u = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(u.Tenant);
            var user = CoreContext.UserManager.GetUserByUserName(userName);
            TraceMessage(TraceDebug, String.Format("Get Contact Info userName={0}", userName));
            if (user.Equals(Core.Users.Constants.LostUser))
            {
                TraceMessage(TraceError, String.Format("Can't getUserInfo by userName={0}, TenantId={1}.",
                    userName, CoreContext.TenantManager.GetCurrentTenant().TenantId));
                throw new HubException();
            }

            return Tuple.Create(user.DisplayUserName(), States.GetState(user.Tenant, userName));
        }

        [HubMethodName("gid")]
        public void GetInitData()
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUserInfo = CoreContext.UserManager.GetUsers(user.ID);
            TraceMessage(TraceDebug, String.Format("Get Init Data userName={0}", currentUserInfo.UserName));
            lock (States.SyncRoot)
            {
                GetStatesFromJabber();
                var states = States.GetStatesOfTenant(currentUserInfo.Tenant);
                var users = GetUsers(states);
                // initDataRetrieved
                Clients.Caller.idr(currentUserInfo.UserName.ToLowerInvariant(),
                    currentUserInfo.DisplayUserName(), users, currentUserInfo.Tenant,
                    CoreContext.TenantManager.GetCurrentTenant().GetTenantDomain(false),
                    WebPath.GetPath(soundPath));
            }
        }

        [HubMethodName("st")]
        public void SendTyping(string calleeUserName)
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUser = CoreContext.UserManager.GetUsers(user.ID);
            if (CoreContext.UserManager.GetUserByUserName(calleeUserName).Equals(Core.Users.Constants.LostUser))
            {
                TraceMessage(TraceError, String.Format("Can't getUserInfo by calleeUserName = {0}, TenantId = {1}.",
                    calleeUserName, currentUser.Tenant));
                throw new HubException();
            }
            // sendTypingSignal
            Clients.Group(currentUser.Tenant + calleeUserName).sts(currentUser.UserName.ToLowerInvariant());
        }

        [HubMethodName("sstt")]
        public async Task SendStateToTenant(byte state)
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUser = CoreContext.UserManager.GetUsers(user.ID);
            var userName = currentUser.UserName.ToLowerInvariant();
            TraceMessage(TraceDebug, String.Format("Send State To Tenant userName={0}, state={1}", userName, state));
            States.Add(currentUser.Tenant, userName, state);
            // setState
            Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture)).ss(userName, state, false);
            try
            {
                await JabberServiceClient.SendState(currentUser.Tenant, userName, state);
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on sending state to Jabber service. UserName = {0}, TenantId = {1}. {2}",
                    userName, currentUser.Tenant, e));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("grm")]
        public MessageClass[] GetRecentMessages(string calleeUserName, int id)
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUser = CoreContext.UserManager.GetUsers(user.ID);
            var calleeUser = CoreContext.UserManager.GetUserByUserName(calleeUserName);
            if (calleeUserName != string.Empty && calleeUser.Equals(Core.Users.Constants.LostUser))
            {
                TraceMessage(TraceError, String.Format("Can't getUserInfo by calleeUserName = {0}, TenantId = {1}.", calleeUserName, currentUser.Tenant));
                throw new HubException();
            }
            MessageClass[] recentMessages = null;
            var callerUserName = currentUser.UserName.ToLowerInvariant();

            TraceMessage(TraceDebug, String.Format("Get Recent Messages calleeUserName={0}, callerUserName={1}, id={2}", calleeUserName, callerUserName, id));

            try
            {
                recentMessages = JabberServiceClient.GetRecentMessages(currentUser.Tenant,
                    callerUserName, calleeUserName == string.Empty ? null : calleeUserName, id);
                if (recentMessages != null)
                {
                    for (var i = 0; i < recentMessages.Length; i++)
                    {
                        recentMessages[i].DateTime = TenantUtil.DateTimeFromUtc(recentMessages[i].DateTime.AddMilliseconds(1));
                        if (recentMessages[i].UserName == null ||
                            String.Equals(recentMessages[i].UserName, calleeUserName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            recentMessages[i].UserName = calleeUserName;
                        }
                        else
                        {
                            recentMessages[i].UserName = callerUserName;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on receiving recent messages from Jabber service. UserName = {0}, TenantId = {1}. {2}, {3}, {4}",
                    currentUser.UserName, currentUser.Tenant, e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
            return recentMessages;
        }

        [HubMethodName("p")]
        public void Ping()
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var userInfo = CoreContext.UserManager.GetUsers(user.ID);
            TraceMessage(TraceDebug, String.Format("Ping from JS client: {0}", userInfo.ID));
            try
            {
                JabberServiceClient.Ping(userInfo.ID.ToString(), userInfo.Tenant, userInfo.UserName, States.GetState(userInfo.Tenant, userInfo.UserName));
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on Ping to Jabber. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

        

        [HubMethodName("cu")]
        public void ConnectUser()
        {
            var user = Context.Request.Environment["server.User"] as GenericPrincipal;
            var tr = Context.QueryString[transport];
            if (user != null)
            {
                var userAccount = user.Identity as IUserAccount;
                if (userAccount == null)
                {
                    TraceMessage(TraceError, "Unknown user tries to connect to SignalR hub.");
                    throw new SecurityException();
                }

                if (tr == websockets)
                {
                    CoreContext.TenantManager.SetCurrentTenant(userAccount.Tenant);
                }

                byte state;
                try
                {
                    state = Convert.ToByte(Context.QueryString[stateNumber]);
                }
                catch (Exception e)
                {
                    TraceMessage(TraceError, String.Format("Possible wrong state on connecting, state = {0}. {1}",
                                                           Context.QueryString[stateNumber], e));
                    state = userOnline;
                }
                var currentUser = CoreContext.UserManager.GetUsers(userAccount.ID);

                if (!currentUser.Equals(Core.Users.Constants.LostUser))
                {
                    var currentUserName = currentUser.UserName.ToLowerInvariant();

                    Groups.Add(Context.ConnectionId, currentUser.Tenant + currentUserName);
                    Groups.Add(Context.ConnectionId, currentUser.Tenant.ToString(CultureInfo.InvariantCulture));
                    var connectionsCount = Connections.Add(currentUser.Tenant, currentUserName, Context.ConnectionId);
                    TraceMessage(TraceDebug,
                                 String.Format("Add Connection. {0}. Count: {1}", currentUserName, ++allConnectionsCount));

                    States.Add(currentUser.Tenant, currentUserName, state);
                    // setState
                    Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture))
                           .ss(currentUserName, state, false);
                    if (connectionsCount == 1)
                    {
                        try
                        {
                            JabberServiceClient.AddXmppConnection(currentUser.ID.ToString(), currentUserName,
                                                                  state, currentUser.Tenant);
                        }
                        catch (Exception e)
                        {
                            TraceMessage(TraceError,
                                         String.Format(
                                             "Error on adding of Jabber connection. Username={0}. {1} {2} {3}",
                                             currentUserName, e, e.StackTrace,
                                             e.InnerException != null ? e.InnerException.Message : string.Empty));
                        }
                    }
                    else
                    {
                        // setStatus
                        Clients.OthersInGroup(currentUser.Tenant + currentUser.UserName).sst(state);
                    }
                }
                else
                {
                    TraceMessage(TraceError, "Unknown user tries to connect.");
                    throw new SecurityException();
                }
            }
        }

        [HubMethodName("dcu")]
        public void DisconnectUser()
        {
            var user = Context.Request.Environment["server.User"] as GenericPrincipal;
            if (user != null)
            {
                var userAccount = user.Identity as IUserAccount;
                if (userAccount != null)
                {
                    CoreContext.TenantManager.SetCurrentTenant(userAccount.Tenant);
                }
                else
                {
                    TraceMessage(TraceError, String.Format("Unknown request without user.Identity as IUserAccount, url={0}",
                        Context.Request.Url));
                    throw new SecurityException();
                }

                var currentUser = CoreContext.UserManager.GetUsers(userAccount.ID);

                if (!currentUser.Equals(Core.Users.Constants.LostUser))
                {
                    var currentUserName = currentUser.UserName.ToLowerInvariant();
                    Groups.Remove(Context.ConnectionId, currentUser.Tenant + currentUserName);
                    Groups.Remove(Context.ConnectionId, currentUser.Tenant.ToString(CultureInfo.InvariantCulture));
                    // if only one connection
                    if (Connections.GetConnectionsCount(currentUser.Tenant, currentUserName) == 1)
                    {
                        var connectionsCount = Connections.Remove(currentUser.Tenant, currentUserName, Context.ConnectionId);
                        TraceMessage(TraceDebug, String.Format("Remove Connection. {0}. Count: {1}", currentUserName, --allConnectionsCount));
                        if (connectionsCount == 0)
                        {
                            try
                            {
                                if (JabberServiceClient.RemoveXmppConnection(currentUser.ID.ToString(), currentUserName, currentUser.Tenant))
                                {
                                    States.Add(currentUser.Tenant, currentUserName, UserOffline);
                                    // setState
                                    Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture)).ss(currentUserName, UserOffline, false);
                                }
                            }
                            catch (Exception e)
                            {
                                States.Add(currentUser.Tenant, currentUserName, UserOffline);
                                // setState
                                Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture)).ss(currentUserName, UserOffline, false);
                                TraceMessage(TraceError, String.Format("Error on removing of Jabber connection. Username={0}. {1} {2} {3}",
                                    currentUserName, e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                            }
                        }
                    }
                    else
                    {
                        Connections.Remove(currentUser.Tenant, currentUserName, Context.ConnectionId);
                        TraceMessage(TraceDebug, String.Format("Remove Connection. {0}. Count: {1}", currentUserName, --allConnectionsCount));
                    }
                }
                else
                {
                    TraceMessage(TraceError, "Unknown user tries to disconnect.");
                    throw new SecurityException();
                }
            }
            else
            {
                TraceMessage(TraceError, String.Format("Unknown user tries to disconnect from SignalR hub."));
                throw new SecurityException();
            }
        }

        private static void GetStatesFromJabber()
        {
            if (!firstChatLoading) return;
            lock (FirstChatLoadingSyncRoot)
            {
                if (firstChatLoading)
                {
                    try
                    {
                        var jabberStates = JabberServiceClient.GetAllStates();
                        foreach (var tenantStatesPair in jabberStates)
                        {
                            foreach (var statesPair in tenantStatesPair.Value)
                            {
                                States.Add(CoreContext.TenantManager.GetTenant(tenantStatesPair.Key).TenantId, statesPair.Key, statesPair.Value);
                            }
                        }
                        firstChatLoading = false;
                    }
                    catch (Exception e)
                    {
                        TraceMessage(TraceError, String.Format("Can't get all states from Jabber. {0} {1} {2}",
                                                               e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                    }
                }
            }
        }

        private static UserStr[] GetUsers(IReadOnlyDictionary<string, byte> states)
        {
            var users = CoreContext.UserManager.GetUsers().Where(user => !user.IsMe()).SortByUserName();
            var usersArray = new UserStr[users.Count];
            for (var i = 0; i < users.Count; i++)
            {
                byte state;
                var userName = users[i].UserName.ToLowerInvariant();
                if (!states.TryGetValue(userName, out state))
                {
                    state = UserOffline;
                }
                usersArray[i] = new UserStr { UserName = userName, DisplayUserName = users[i].DisplayUserName(), State = state };
            }
            return usersArray;
        }

        public static void TraceMessage(byte messageState, string message, [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            string name;
            try
            {
                name = HttpContext.Current.User.Identity.Name;
            }
            catch
            {
                name = string.Empty;
            }
            switch (messageState)
            {
                case TraceError:
                    ChatLog.ErrorFormat(message + " {0}:{1}:{2}. {3}", filePath, memberName, lineNumber, name);
                    break;
                case TraceDebug:
                    ChatLog.DebugFormat(message + " {0}:{1}:{2}. {3}", filePath, memberName, lineNumber, name);
                    break;
            }
        }
    }
}