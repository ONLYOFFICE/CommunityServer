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

using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Tenants;
using ASC.Core.Users;
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
        public readonly static JabberServiceClient JabberServiceClient = new JabberServiceClient();
        private readonly static ILog log = LogManager.GetLogger(typeof(Chat));
        private readonly static object FirstChatLoadingSyncRoot = new object();
        private volatile static int allConnectionsCount;
        private const string websockets = "webSockets";
        private const string transport = "transport";
        private const byte userOnline = 1;
        public const byte UserOffline = 4;
        public const byte TraceError = 0;
        public const byte TraceDebug = 1;

        

        public override Task OnDisconnected(bool stopCalled)
        {
            try
            {
                var user = Context.Request.Environment["server.User"] as GenericPrincipal;
                if (user == null)
                {
                    AuthorizeHubAttribute.Authorize(Context.Request);
                }
                DisconnectUser();
            }
            catch (Exception)
            {
                
            }
            
            return base.OnDisconnected(stopCalled);
        }

        // Method for JS-clients

        [HubMethodName("s")]
        public void Send(string calleeUserName, string messageText)
        {
            try
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

                 JabberServiceClient.SendMessage(currentUser.Tenant, callerUserName, calleeUserName, messageText);
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on sending message to Jabber service. {0} {1} {2}",
                  e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("gs")]
        public void GetStates()
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var currentUserInfo = CoreContext.UserManager.GetUsers(user.ID);
                var currentUserName = currentUserInfo.UserName.ToLowerInvariant();
                TraceMessage(TraceDebug, String.Format("Get States currentUserName={0}", currentUserName));
                // statesRetrieved
                Clients.Caller.sr(JabberServiceClient.GetAllStates(user.Tenant, currentUserName));
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on GetStates to Jabber service. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("gci")]
        public Tuple<string, byte> GetContactInfo(string userName)
        {
            try
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

                return Tuple.Create(user.DisplayUserName(), JabberServiceClient.GetState(user.Tenant, userName));
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on GetContactInfo to Jabber service. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
            return null;
        }

        [HubMethodName("gid")]
        public void GetInitData()
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var currentUserInfo = CoreContext.UserManager.GetUsers(user.ID);
                TraceMessage(TraceDebug, String.Format("Get Init Data userName={0}", currentUserInfo.UserName));
                // initDataRetrieved
                Clients.Caller.idr(currentUserInfo.UserName.ToLowerInvariant(), currentUserInfo.DisplayUserName(),
                    GetUsers(JabberServiceClient.GetAllStates(currentUserInfo.Tenant, currentUserInfo.UserName.ToLowerInvariant())),
                    currentUserInfo.Tenant, CoreContext.TenantManager.GetCurrentTenant().GetTenantDomain());
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on GetInitData to Jabber service. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("st")]
        public void SendTyping(string calleeUserName)
        {
            try
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
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on sending typing to Jabber service. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("sstt")]
        public void SendStateToTenant(byte state)
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var currentUser = CoreContext.UserManager.GetUsers(user.ID);
                var userName = currentUser.UserName.ToLowerInvariant();
                TraceMessage(TraceDebug, String.Format("Send State To Tenant userName={0}, state={1}", userName, state));
                state = JabberServiceClient.SendState(currentUser.Tenant, userName, state);
                // setState
                Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture)).ss(userName, state, false);
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on SendStateToTenant to Jabber. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("grm")]
        public MessageClass[] GetRecentMessages(string calleeUserName, int id)
        {
            MessageClass[] recentMessages = null;
            try
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
                
                var callerUserName = currentUser.UserName.ToLowerInvariant();
                TraceMessage(TraceDebug, String.Format("Get Recent Messages calleeUserName={0}, callerUserName={1}, id={2}", calleeUserName, callerUserName, id));
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
                TraceMessage(TraceError, String.Format("Error on receiving recent messages from Jabber service. {0}, {1}, {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
            return recentMessages;
        }

        [HubMethodName("p")]
        public void Ping(byte state)
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var userInfo = CoreContext.UserManager.GetUsers(user.ID);
                TraceMessage(TraceDebug, String.Format("Ping from JS client: {0}", userInfo.ID));
                JabberServiceClient.Ping(userInfo.ID.ToString(), userInfo.Tenant, userInfo.UserName, state);
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on Ping to Jabber. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

        [HubMethodName("cu")]
        public void ConnectUser(string stateNumber)
        {
            try
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
                    CoreContext.TenantManager.SetCurrentTenant(userAccount.Tenant);
                    byte state;
                    try
                    {
                        state = Convert.ToByte(stateNumber);
                    }
                    catch (Exception e)
                    {
                        TraceMessage(TraceError, String.Format("Possible wrong state on connecting, state = {0}. {1}", stateNumber, e));
                        state = userOnline;
                    }
                    var currentUser = CoreContext.UserManager.GetUsers(userAccount.ID);

                    if (!currentUser.Equals(Core.Users.Constants.LostUser))
                    {
                        var currentUserName = currentUser.UserName.ToLowerInvariant();

                        Groups.Add(Context.ConnectionId, currentUser.Tenant + currentUserName);
                        Groups.Add(Context.ConnectionId, currentUser.Tenant.ToString(CultureInfo.InvariantCulture));
                        var connectionsCount = Connections.Add(currentUser.Tenant, currentUserName, Context.ConnectionId);
                        TraceMessage(TraceDebug, String.Format("Add Connection. {0}. Count: {1}", currentUserName, ++allConnectionsCount));

                        if (connectionsCount == 1)
                        {
                            state = JabberServiceClient.AddXmppConnection(currentUser.ID.ToString(), currentUserName, state, currentUser.Tenant);
                        }
                        else
                        {
                            state = JabberServiceClient.SendState(currentUser.Tenant, currentUserName, state);
                            if (state != UserOffline)
                            {
                                // setStatus
                                Clients.OthersInGroup(currentUser.Tenant + currentUser.UserName).sst(state);
                            }
                        }
                        // setState
                        Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture)).ss(currentUserName, state, false);
                    }
                    else
                    {
                        TraceMessage(TraceError, "Unknown user tries to connect.");
                        throw new SecurityException("Unknown user tries to connect.");
                    }
                }
                else
                {
                    TraceMessage(TraceError, "Unknown user tries to connect.");
                    throw new SecurityException("Unknown user tries to connect.");
                }
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on ConnectUser to Jabber. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        [HubMethodName("dcu")]
        public void DisconnectUser()
        {
            try
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
                        throw new SecurityException("Unknown request without user.Identity as IUserAccount");
                    }
                    var currentUser = CoreContext.UserManager.GetUsers(userAccount.ID);

                    if (!currentUser.Equals(Core.Users.Constants.LostUser))
                    {
                        var currentUserName = currentUser.UserName.ToLowerInvariant();
                        Groups.Remove(Context.ConnectionId, currentUser.Tenant + currentUserName);
                        Groups.Remove(Context.ConnectionId, currentUser.Tenant.ToString(CultureInfo.InvariantCulture));
                        byte state = UserOffline;
                        bool result;
                        var connectionsCount = Connections.Remove(currentUser.Tenant, currentUserName, Context.ConnectionId, out result);
                        if (result)
                        {
                            TraceMessage(TraceDebug, String.Format("Remove Connection. {0}. Count: {1}", currentUserName, --allConnectionsCount));
                            if (connectionsCount == 0)
                            {
                                state = JabberServiceClient.RemoveXmppConnection(currentUser.ID.ToString(), currentUserName, currentUser.Tenant);
                            }
                            else
                            {
                                state = JabberServiceClient.GetState(currentUser.Tenant, currentUserName);
                                if (state != UserOffline)
                                {
                                    // setStatus
                                    Clients.OthersInGroup(currentUser.Tenant + currentUser.UserName).sst(state);
                                }
                            }

                            // setState
                            Clients.OthersInGroup(currentUser.Tenant.ToString(CultureInfo.InvariantCulture)).ss(currentUserName, state, false);
                        }
                    }
                    else
                    {
                        TraceMessage(TraceError, "Unknown user tries to disconnect.");
                        throw new SecurityException("Unknown user tries to disconnect.");
                    }
                }
                else
                {
                    TraceMessage(TraceError, "Unknown user tries to disconnect from SignalR hub.");
                    throw new SecurityException("Unknown user tries to disconnect from SignalR hub.");
                }
            }
            catch (Exception e)
            {
                TraceMessage(TraceError, String.Format("Error on DisconnectUser to Jabber. {0} {1} {2}",
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
                // error
                Clients.Caller.e();
            }
        }

        private static UserClass[] GetUsers(IReadOnlyDictionary<string, byte> states)
        {
            var users = CoreContext.UserManager.GetUsers().Where(user => !user.IsMe()).SortByUserName();
            var usersArray = new UserClass[users.Count];
            for (var i = 0; i < users.Count; i++)
            {
                byte state;
                var userName = users[i].UserName.ToLowerInvariant();
                if (!states.TryGetValue(userName, out state))
                {
                    state = UserOffline;
                }
                usersArray[i] = new UserClass { UserName = userName, DisplayUserName = users[i].DisplayUserName(), State = state };
            }
            return usersArray;
        }

        public static void TraceMessage(byte messageState, string message, [CallerMemberName] string memberName = "", 
            [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
        {
            switch (messageState)
            {
                case TraceError:
                    log.ErrorFormat(message + " {0}:{1}:{2}", filePath, memberName, lineNumber);
                    break;
                case TraceDebug:
                    log.DebugFormat(message + " {0}:{1}:{2}", filePath, memberName, lineNumber);
                    break;
            }
        }
    }
}