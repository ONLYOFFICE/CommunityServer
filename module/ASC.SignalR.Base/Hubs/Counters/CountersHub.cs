/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using ASC.Common.Security.Authentication;
using ASC.Core;
using ASC.Core.Users;
using ASC.Feed.Data;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.SignalR.Base.Hubs.Chat;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Principal;
using System.Threading.Tasks;

namespace ASC.SignalR.Base.Hubs.Counters
{
    [AuthorizeHub]
    [HubName("ch")]
    public class CountersHub : Hub
    {
        public static readonly ConnectionMapping Connections = new ConnectionMapping();
        private static readonly JabberServiceClient jabberServiceClient = new JabberServiceClient();
        private static readonly FeedReadedDataProvider feedReadedProvider = new FeedReadedDataProvider();
        private static readonly MailBoxManager mailBoxManager =
            new MailBoxManager(LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "CountersHub"));
        private static readonly ILog log = LogManager.GetLogger(typeof(CountersHub));

        public override Task OnConnected()
        {
            try
            {
                var user = Context.Request.Environment["server.User"] as GenericPrincipal;
                if (user == null)
                {
                    AuthorizeHubAttribute.Authorize(Context.Request);
                }
                ConnectUser();
            }
            catch
            {

            }
            return base.OnConnected();
        }

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
            catch
            {

            }

            return base.OnDisconnected(stopCalled);
        }

        [HubMethodName("gnmc")]
        public Counts GetNewMessagesCount()
        {
            var user = (IUserAccount)Context.User.Identity;
            CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
            var currentUser = CoreContext.UserManager.GetUsers(user.ID);
            var counts = new Counts();
            counts.MessagesCount = GetMessagesCount(currentUser);
            counts.FeedsCount = GetUserFeedsCount();
            counts.MailsCount = GetMailsCount(currentUser);

            return counts;
        }

        [HubMethodName("smec")]
        public void SendMessagesCount(int count)
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var currentUser = CoreContext.UserManager.GetUsers(user.ID);
                // sendMessagesCount
                Clients.OthersInGroup(currentUser.Tenant + currentUser.UserName.ToLowerInvariant()).smec(count);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error on SendMessagesCount. {0} {1} {2}",
                    e.ToString(), e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        [HubMethodName("sfc")]
        public void SendFeedsCount()
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var currentUser = CoreContext.UserManager.GetUsers(user.ID);
                // sendFeedsCount
                Clients.OthersInGroup(currentUser.Tenant + currentUser.UserName.ToLowerInvariant()).sfc(0);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error on SendFeedsCount. {0} {1} {2}",
                    e.ToString(), e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        [HubMethodName("uf")]
        public void UpdateFolders(bool shouldUpdateMailBox)
        {
            try
            {
                var user = (IUserAccount)Context.User.Identity;
                CoreContext.TenantManager.SetCurrentTenant(user.Tenant);
                var currentUser = CoreContext.UserManager.GetUsers(user.ID);
                string currentUserName = currentUser.UserName.ToLowerInvariant();
                if (Connections.GetConnectionsCount(currentUser.Tenant, currentUserName) > 1)
                {
                    int count = GetMailsCount(currentUser);
                    // updateFolders
                    Clients.OthersInGroup(currentUser.Tenant + currentUserName).uf(count, shouldUpdateMailBox);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error on UpdateFolders. {0} {1} {2}",
                    e.ToString(), e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        private void ConnectUser()
        {
            try
            {
                var user = Context.Request.Environment["server.User"] as GenericPrincipal;
                if (user != null)
                {
                    var userAccount = user.Identity as IUserAccount;
                    if (userAccount == null)
                    {
                        log.Error("Unknown user tries to connect to SignalR hub.");
                        throw new SecurityException();
                    }
                    CoreContext.TenantManager.SetCurrentTenant(userAccount.Tenant);
                    var currentUser = CoreContext.UserManager.GetUsers(userAccount.ID);

                    if (!currentUser.Equals(Core.Users.Constants.LostUser))
                    {
                        string currentUserName = currentUser.UserName.ToLowerInvariant();
                        var connectionsCount = Connections.Add(currentUser.Tenant, currentUserName, Context.ConnectionId);
                        Groups.Add(Context.ConnectionId, currentUser.Tenant + currentUserName);

                        // if user became active we should set user_online flag to true in mailbox table
                        if (connectionsCount == 1)
                        {
                            mailBoxManager.UpdateUserActivity(currentUser.Tenant, currentUser.ID.ToString(), true);
                        }
                    }
                    else
                    {
                        log.Error("Unknown user tries to connect.");
                        throw new SecurityException("Unknown user tries to connect.");
                    }
                }
                else
                {
                    log.Error("Unknown user tries to connect.");
                    throw new SecurityException("Unknown user tries to connect.");
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error on ConnectUser. {0} {1} {2}",
                    e.ToString(), e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        private void DisconnectUser()
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
                        log.ErrorFormat("Unknown request without user.Identity as IUserAccount, url={0}",
                            Context.Request.Url);
                        throw new SecurityException("Unknown request without user.Identity as IUserAccount");
                    }
                    var currentUser = CoreContext.UserManager.GetUsers(userAccount.ID);

                    if (!currentUser.Equals(Core.Users.Constants.LostUser))
                    {
                        string currentUserName = currentUser.UserName.ToLowerInvariant();
                        bool result;
                        var connectionsCount = Connections.Remove(currentUser.Tenant, currentUserName, Context.ConnectionId, out result);
                        Groups.Remove(Context.ConnectionId, currentUser.Tenant + currentUserName);

                        // if user became inactive we should set user_online flag to false in mailbox table
                        if (connectionsCount == 0)
                        {
                            mailBoxManager.UpdateUserActivity(currentUser.Tenant, currentUser.ID.ToString(), false);
                        }
                    }
                    else
                    {
                        log.Error("Unknown user tries to disconnect.");
                        throw new SecurityException("Unknown user tries to disconnect.");
                    }
                }
                else
                {
                    log.Error("Unknown user tries to disconnect from SignalR hub.");
                    throw new SecurityException("Unknown user tries to disconnect from SignalR hub.");
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Error on DisconnectUser. {0} {1} {2}",
                    e.ToString(), e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        private int GetMessagesCount(UserInfo currentUser)
        {
            int count = 0;
            try
            {
                count = jabberServiceClient.GetNewMessagesCount(currentUser.Tenant, currentUser.UserName.ToLowerInvariant());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetNewMessagesCount error: {0}, {1}, tenantId = {2}, userName = {3}",
                    ex.StackTrace, ex.ToString(), currentUser.Tenant, currentUser.UserName.ToLowerInvariant());
            }

            return count;
        }

        private int GetUserFeedsCount()
        {
            int count = 0;
            try
            {
                var lastTimeReaded = feedReadedProvider.GetTimeReaded();
                count = FeedAggregateDataProvider.GetNewFeedsCount(lastTimeReaded);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetNewFeedsCount error: {0}, {1}", ex.StackTrace, ex.ToString());
            }

            return count;
        }

        private int GetMailsCount(UserInfo currentUser)
        {
            int count = 0;
            try
            {
                List<MailBoxManager.MailFolderInfo> mailFolderInfos =
                    mailBoxManager.GetFoldersList(currentUser.Tenant, currentUser.ID.ToString(), true);
                foreach (var mailFolderInfo in mailFolderInfos)
                {
                    if (mailFolderInfo.id == MailFolder.Ids.inbox)
                    {
                        count = mailFolderInfo.unread;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetMailsCount error: {0}, {1}, tenantId = {2}, userName = {3}",
                    ex.StackTrace, ex.ToString(), currentUser.Tenant, currentUser.UserName.ToLowerInvariant());
            }

            return count;
        }
    }
}
