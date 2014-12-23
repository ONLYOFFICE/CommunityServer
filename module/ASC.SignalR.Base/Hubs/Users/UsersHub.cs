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
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using ASC.Common.Security.Authentication;
using ASC.MessagingSystem;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using log4net;
using System.Linq;

namespace ASC.SignalR.Base.Hubs.OnlineUsers
{
    [AuthorizeHub]
    [HubName("users")]
    public class UsersHub : Hub
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UsersHub));

        private static readonly Dictionary<int, ConcurrentDictionary<string, UserPresence>> onlineUsers =
            new Dictionary<int, ConcurrentDictionary<string, UserPresence>>();

        private static readonly ConcurrentDictionary<string, System.Timers.Timer> pretenderTimers =
            new ConcurrentDictionary<string, System.Timers.Timer>();

        private static readonly TimeSpan offlinePretendersVerifyTimeout = TimeSpan.FromSeconds(10);

        private readonly TimeSpan lostConnectionTimeout = TimeSpan.FromHours(2);
        private static readonly Dictionary<int, DateTime> lostConnectionsVerifyTime = new Dictionary<int, DateTime>();

        public static CancelableTask GetUserOfflineInspector(int tenantId, string userId)
        {
            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            var task = new Task<bool>(() =>
                {
                    var interval = (int)offlinePretendersVerifyTimeout.TotalMilliseconds + 1000;
                    for (var i = 0; i < interval; i += 1000)
                    {
                        Thread.Sleep(1000);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return false;
                        }
                    }
                    
                    return !onlineUsers[tenantId].ContainsKey(userId);
                }, cancellationToken);

            return new CancelableTask(task, tokenSource);
        }

        public override Task OnConnected()
        {
            AddOnlineUser();
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            RemoveOnlineUser();
            return base.OnDisconnected(stopCalled);
        }

        [HubMethodName("pushOnlineUsersToAll")]
        public void PushOnlineUserToAll(string user)
        {
            Clients.Group(ObserversOnlineUsersGroupId).renderOnlineUser(user);
        }

        [HubMethodName("pushOnlineUsersToAll")]
        public void PushOfflineUserToAll(string user)
        {
            Clients.Group(ObserversOnlineUsersGroupId).renderOfflineUser(user);
        }

        [HubMethodName("pushOnlineUsers")]
        public void PushOnlineUsers()
        {
            VerifyLostConnections();
            Clients.Client(Context.ConnectionId).renderOnlineUsers(onlineUsers[CurrentTenantId]);
        }

        [HubMethodName("addToObserversOnlineUsers")]
        public void AddToObserversOnlineUsers()
        {
            Groups.Add(Context.ConnectionId, ObserversOnlineUsersGroupId);
            PushOnlineUsers();
        }

        [HubMethodName("removeFromObserversOnlineUsers")]
        public void RemoveFromObserversOnlineUsers()
        {
            Groups.Remove(Context.ConnectionId, ObserversOnlineUsersGroupId);
        }

        private void AddOnlineUser()
        {
            try
            {
                var now = DateTime.UtcNow;
                var newOnlineUser = false;

                InitDictionary(onlineUsers, CurrentTenantId);
                onlineUsers[CurrentTenantId]
                    .AddOrUpdate(CurrentUserId,
                                 user =>
                                     {
                                         newOnlineUser = true;
                                         return new UserPresence(now);
                                     },
                                 (user, presence) =>
                                     {
                                         presence.OfflinePretender = false;
                                         presence.Counter++;
                                         presence.LastConnection = DateTime.UtcNow;

                                         return presence;
                                     });

                if (newOnlineUser)
                {
                    PushOnlineUserToAll(CurrentUserId);
                    MessageService.Send(Context.Headers.ToDictionary(x => x.Key, y => y.Value), MessageAction.SessionStarted);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        private void RemoveOnlineUser()
        {
            try
            {
                var lostUser = false;

                InitDictionary(onlineUsers, CurrentTenantId);
                onlineUsers[CurrentTenantId]
                    .AddOrUpdate(CurrentUserId,
                                 user =>
                                     {
                                         lostUser = true;
                                         return null;
                                     },
                                 (user, presence) =>
                                     {
                                         if (presence.Counter == 1)
                                         {
                                             presence.OfflinePretender = true;
                                             SetOfflinePretenderTimer();
                                         }
                                         presence.Counter--;

                                         return presence;
                                     });

                if (lostUser)
                {
                    UserPresence p;
                    onlineUsers[CurrentTenantId].TryRemove(CurrentUserId, out p);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        private void SetOfflinePretenderTimer()
        {
            Dictionary<string, string> headers;
            try
            {
                headers = Context.Headers.ToDictionary(x => x.Key, y => y.Value);
            }
            catch(ObjectDisposedException)
            {
                // bug: Hub OnDisconnected - Context throwing an ObjectDisposedException in IE
                // will be fix in signalr 2.2
                headers = null;
            }

            var state = new Tuple<MessageUserData, Dictionary<string, string>>(
                new MessageUserData(CurrentTenantId, new Guid(CurrentUserId)), headers);

            var timer = new System.Timers.Timer();
            timer.Elapsed += (sender, e) => CheckOfflinePretender(state);
            timer.Interval = offlinePretendersVerifyTimeout.TotalMilliseconds;
            timer.AutoReset = false;

            var updated = false;
            pretenderTimers.AddOrUpdate(CurrentUserId, timer,
                                        (user, tmr) =>
                                            {
                                                updated = true;
                                                tmr.Stop();
                                                tmr.Start();
                                                return tmr;
                                            });

            if (!updated)
            {
                timer.Start();
            }
        }

        private void CheckOfflinePretender(object state)
        {
            try
            {
                var data = (Tuple<MessageUserData, Dictionary<string, string>>)state;
                var userData = data.Item1;
                var headers = data.Item2;

                var userId = userData.UserId.ToString();
                var tenantId = userData.TenantId;

                var lostUser = false;
                var newOfflineUser = false;

                onlineUsers[tenantId]
                    .AddOrUpdate(userId,
                                 user =>
                                     {
                                         lostUser = true;
                                         return null;
                                     },
                                 (user, presence) =>
                                     {
                                         if (presence.OfflinePretender && presence.Counter == 0)
                                         {
                                             newOfflineUser = true;
                                         }
                                         return presence;
                                     });

                if (lostUser)
                {
                    UserPresence p;
                    onlineUsers[CurrentTenantId].TryRemove(CurrentUserId, out p);
                }

                System.Timers.Timer timer;
                pretenderTimers.TryRemove(userId, out timer);
                if (timer != null)
                {
                    timer.Dispose();
                }

                if (newOfflineUser)
                {
                    UserPresence p;
                    onlineUsers[tenantId].TryRemove(userId, out p);

                    PushOfflineUserToAll(userId);
                    MessageService.Send(userData, headers, MessageAction.SessionCompleted);
                }
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        private void VerifyLostConnections()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(lostConnectionsVerifyTime, ref lockTaken);
                if (!lockTaken) return;

                if (!lostConnectionsVerifyTime.ContainsKey(CurrentTenantId))
                {
                    lostConnectionsVerifyTime[CurrentTenantId] = DateTime.UtcNow;
                }
                else
                {
                    if (ConnectionLost(lostConnectionsVerifyTime[CurrentTenantId]))
                    {
                        InitDictionary(onlineUsers, CurrentTenantId);
                        var usersToRemove = onlineUsers[CurrentTenantId].Where(x => ConnectionLost(x.Value.LastConnection)).ToList();
                        foreach (var user in usersToRemove)
                        {
                            UserPresence p;
                            onlineUsers[CurrentTenantId].TryRemove(user.Key, out p);
                        }

                        lostConnectionsVerifyTime[CurrentTenantId] = DateTime.UtcNow;
                    }
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(lostConnectionsVerifyTime);
                }
            }
        }

        private static void InitDictionary(Dictionary<int, ConcurrentDictionary<string, UserPresence>> dictionary, int key)
        {
            lock (dictionary)
            {
                if (dictionary.ContainsKey(key)) return;
                dictionary.Add(key, new ConcurrentDictionary<string, UserPresence>());
            }
        }

        private bool ConnectionLost(DateTime lastPresence)
        {
            return DateTime.UtcNow - lastPresence > lostConnectionTimeout;
        }

        private string CurrentUserId
        {
            get { return new CustomUserIdProvider().GetUserId(Context.Request); }
        }

        private int CurrentTenantId
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

            var user = Context.Request.Environment["server.User"] as GenericPrincipal ??
                       Context.Request.GetHttpContext().User as GenericPrincipal;
            if (user != null) return user.Identity as IUserAccount;

            return null;
        }

        private string ObserversOnlineUsersGroupId
        {
            get { return "observersOnlineUsers" + CurrentTenantId; }
        }


        private class UserPresence
        {
            public int Counter { get; set; }
            public DateTime FirstConnection { get; set; }
            public DateTime LastConnection { get; set; }

            public bool OfflinePretender { get; set; }

            public UserPresence(DateTime date)
            {
                Counter = 1;

                FirstConnection = date;
                LastConnection = date;
            }
        }
    }
}