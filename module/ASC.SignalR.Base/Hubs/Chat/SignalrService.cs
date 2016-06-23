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


using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Notify.Signalr;
using ASC.Core.Users;
using ASC.Feed.Data;
using ASC.Mail.Aggregator;
using ASC.Mail.Aggregator.Common;
using ASC.Mail.Aggregator.Common.Logging;
using ASC.SignalR.Base.Hubs.Counters;
using log4net;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.ServiceModel;

namespace ASC.SignalR.Base.Hubs.Chat
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public class SignalrService : ISignalrService
    {
        private static readonly IHubContext chatContext = GlobalHost.ConnectionManager.GetHubContext<Chat>();
        private static readonly IHubContext countersContext = GlobalHost.ConnectionManager.GetHubContext<CountersHub>();
        private static readonly FeedReadedDataProvider feedReadedProvider = new FeedReadedDataProvider();
        private static readonly ILog log = LogManager.GetLogger(typeof(SignalrService));

        public void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId, string domain)
        {
            try
            {
                var tenant = tenantId == -1 ? CoreContext.TenantManager.GetTenant(domain) : CoreContext.TenantManager.GetTenant(tenantId);

                log.DebugFormat("Message is received. tenantId = {0}, callee = {1}, caller = {2}",
                    tenant.TenantId, calleeUserName, callerUserName);

                var isTenantUser = callerUserName == string.Empty;
                var message = new MessageClass
                {
                    UserName = isTenantUser ? tenant.GetTenantDomain() : callerUserName,
                    Text = messageText
                };
                // send
                chatContext.Clients.Group(tenant.TenantId + calleeUserName).s(message, calleeUserName, isTenantUser);
                if (!isTenantUser)
                {
                    // send
                    chatContext.Clients.Group(tenant.TenantId + callerUserName).s(message, calleeUserName, isTenantUser);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. callerUserName = {0}, calleeUserName = {1}, {2}, {3}", callerUserName,
                   calleeUserName, e.ToString(), e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendInvite(string chatRoomName, string calleeUserName, string domain)
        {
            try
            {
                var tenant = CoreContext.TenantManager.GetTenant(domain);

                log.DebugFormat("Invite is received. chatRoomName = {0}, calleeUserName = {1}, domain {2}, tenantId={3}",
                    chatRoomName, calleeUserName, domain, tenant.TenantId);

                var message = new MessageClass
                {
                    UserName = tenant.GetTenantDomain(),
                    Text = chatRoomName
                };
                // sendInvite
                chatContext.Clients.Group(tenant.TenantId + calleeUserName).si(message);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. calleeUserName = {0}, {1}, {2}",
                    calleeUserName, e.ToString(), e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendState(string from, byte state, int tenantId, string domain)
        {
            try
            {
                if (tenantId == -1)
                {
                    tenantId = CoreContext.TenantManager.GetTenant(domain).TenantId;
                }

                log.DebugFormat("State is received. from = {0}, state = {1}, tenantId = {2}, domain = {3}",
                    from, state, tenantId, domain);

                if (state == Chat.UserOffline && Chat.Connections.GetConnectionsCount(tenantId, from) > 0)
                {
                    return;
                }
                // setState
                chatContext.Clients.Group(tenantId.ToString(CultureInfo.InvariantCulture)).ss(from, state, true);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. from = {0}, {1}, {2}", from,
                    e.ToString(), e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendOfflineMessages(string callerUserName, List<string> users, int tenantId)
        {
            try
            {
                log.DebugFormat("Offline messages is received. tenantId = {0}", tenantId);
                // sendOfflineMessages
                chatContext.Clients.Group(tenantId.ToString(CultureInfo.InvariantCulture) + callerUserName).som(users);
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. callerUserName = {0}, {1}, {2}", callerUserName,
                    e.ToString(), e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendUnreadCounts(Dictionary<string, int> unreadCounts, string domain)
        {
            try
            {
                log.Debug("SenUnreadCounts.");
                var tenant = CoreContext.TenantManager.GetTenant(domain);
                foreach(var pair in unreadCounts)
                {
                    // sendMessagesCount
                    countersContext.Clients.Group(tenant.TenantId + pair.Key).smec(pair.Value);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. {0}, {1}", e.ToString(),
                    e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendUnreadUsers(Dictionary<int, HashSet<Guid>> unreadUsers)
        {
            try
            {
                foreach (var tenantsUsers in unreadUsers)
                {
                    CoreContext.TenantManager.SetCurrentTenant(tenantsUsers.Key);
                    foreach (var userId in tenantsUsers.Value)
                    {
                        var userInfo = CoreContext.UserManager.GetUsers(userId);
                        string currentUserName = userInfo.UserName.ToLowerInvariant();
                        if (userInfo.ID != Constants.LostUser.ID &&
                            CountersHub.Connections.GetConnectionsCount(userInfo.Tenant, currentUserName) > 0)
                        {
                            SecurityContext.AuthenticateMe(userInfo.ID);

                            var lastTimeReaded = feedReadedProvider.GetTimeReaded();
                            int count = FeedAggregateDataProvider.GetNewFeedsCount(lastTimeReaded);
                            // sendFeedsCount
                            countersContext.Clients.Group(tenantsUsers.Key + currentUserName).sfc(count);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. {0}, {1}", e.ToString(),
                    e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendUnreadUser(int tenant, string userId)
        {
            try
            {
                int count = 0;
                var mailBoxManager = new MailBoxManager(
                    LoggerFactory.GetLogger(LoggerFactory.LoggerType.Log4Net, "SignalrService"));

                List<MailBoxManager.MailFolderInfo> mailFolderInfos =
                    mailBoxManager.GetFolders(tenant, userId, true);
                foreach (var mailFolderInfo in mailFolderInfos)
                {
                    if (mailFolderInfo.id == MailFolder.Ids.inbox)
                    {
                        count = mailFolderInfo.unread;
                        break;
                    }
                }
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                var userInfo = CoreContext.UserManager.GetUsers(Guid.Parse(userId));
                if (userInfo.ID != Constants.LostUser.ID)
                {
                    // sendMailsCount
                    countersContext.Clients.Group(tenant + userInfo.UserName.ToLowerInvariant()).uf(count);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. {0}, {1}", e.ToString(),
                    e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }

        public void SendMailNotification(int tenant, string userId, int state)
        {
            try
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                var userInfo = CoreContext.UserManager.GetUsers(Guid.Parse(userId));
                if (userInfo.ID != Constants.LostUser.ID)
                {
                    // sendMailNotification
                    countersContext.Clients.Group(tenant + userInfo.UserName.ToLowerInvariant()).smn(state);
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("Unknown Error. {0}, {1}", e.ToString(),
                    e.InnerException != null ? e.InnerException.Message : string.Empty);
            }
        }
    }
}