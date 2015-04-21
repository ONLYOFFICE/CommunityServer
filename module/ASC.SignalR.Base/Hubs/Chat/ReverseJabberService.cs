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


using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Notify.Jabber;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.ServiceModel;

namespace ASC.SignalR.Base.Hubs.Chat
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public class ReverseJabberService : IReverseJabberService
    {
        private const string TOKEN = "token";
        private readonly string token = ConfigurationManager.AppSettings["web.chat-token"] ?? "95739c2e-e001-4b50-a179-9950678b2bb0";
        private readonly static IHubContext context = GlobalHost.ConnectionManager.GetHubContext<Chat>();

        public void SendMessage(string callerUserName, string calleeUserName, string messageText, int tenantId, string domain)
        {
            try
            {
                var index = OperationContext.Current.IncomingMessageHeaders.FindHeader(TOKEN, string.Empty);
                if (index == -1 || OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(index) != token)
                {
                    Chat.TraceMessage(Chat.TraceError, string.Format("Wrong header. index={0}", index));
                    return;
                }
                var tenant = tenantId == -1 ? CoreContext.TenantManager.GetTenant(domain) : CoreContext.TenantManager.GetTenant(tenantId);

                Chat.TraceMessage(Chat.TraceDebug, string.Format("Message is received. tenantId = {0},callee={1},caller={2}",
                    tenant.TenantId, calleeUserName, callerUserName));

                var isTenantUser = callerUserName == string.Empty;
                var message = new MessageClass
                {
                    UserName = isTenantUser ? tenant.GetTenantDomain() : callerUserName,
                    Text = messageText
                };
                // send
                context.Clients.Group(tenant.TenantId + calleeUserName).s(message, calleeUserName, isTenantUser);
                if (!isTenantUser)
                {
                    // send
                    context.Clients.Group(tenant.TenantId + callerUserName).s(message, calleeUserName, isTenantUser);
                }
            }
            catch (Exception e)
            {
                Chat.TraceMessage(Chat.TraceError, string.Format("Unknown Error. callerUserName = {0}, calleeUserName = {1}, {2}, {3}, {4}", callerUserName,
                   calleeUserName, e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

        public void SendInvite(string chatRoomName, string calleeUserName, string domain)
        {
            try
            {
                var index = OperationContext.Current.IncomingMessageHeaders.FindHeader(TOKEN, string.Empty);
                if (index == -1 || OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(index) != token)
                {
                    Chat.TraceMessage(Chat.TraceError, string.Format("Wrong header. index={0}", index));
                    return;
                }
                var tenant = CoreContext.TenantManager.GetTenant(domain);

                Chat.TraceMessage(Chat.TraceDebug, string.Format("Invite is received. chatRoomName={0}, calleeUserName={1}, domain {2}, tenantId={3}",
                    chatRoomName, calleeUserName, domain, tenant.TenantId));

                var message = new MessageClass
                {
                    UserName = tenant.GetTenantDomain(),
                    Text = chatRoomName
                };
                // sendInvite
                context.Clients.Group(tenant.TenantId + calleeUserName).si(message);
            }
            catch (Exception e)
            {
                Chat.TraceMessage(Chat.TraceError, string.Format("Unknown Error. calleeUserName ={0}, {1}, {2}, {3}",
                    calleeUserName, e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

        public void SendState(string from, byte state, int tenantId, string domain)
        {
            try
            {
                var index = OperationContext.Current.IncomingMessageHeaders.FindHeader(TOKEN, string.Empty);
                if (index == -1 || OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(index) != token)
                {
                    Chat.TraceMessage(Chat.TraceError, string.Format("Wrong header. index={0}", index));
                    return;
                }
                if (tenantId == -1)
                {
                    tenantId = CoreContext.TenantManager.GetTenant(domain).TenantId;
                }

                Chat.TraceMessage(Chat.TraceDebug, string.Format("State is received. from={0}, state={1}, tenantId={2}, domain={3}",
                    from, state, tenantId, domain));

                if (state == Chat.UserOffline && Chat.Connections.GetConnectionsCount(tenantId, from) > 0)
                {
                    return;
                }
                // setState
                context.Clients.Group(tenantId.ToString(CultureInfo.InvariantCulture)).ss(from, state, true);
            }
            catch (Exception e)
            {
                Chat.TraceMessage(Chat.TraceError, string.Format("Unknown Error. from={0}, {1}, {2}, {3}", from,
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

        public void SendOfflineMessages(string callerUserName, List<string> users, int tenantId)
        {
            try
            {
                var index = OperationContext.Current.IncomingMessageHeaders.FindHeader(TOKEN, string.Empty);
                if (index == -1 || OperationContext.Current.IncomingMessageHeaders.GetHeader<string>(index) != token)
                {
                    Chat.TraceMessage(Chat.TraceError, string.Format("Wrong header. index={0}", index));
                    return;
                }
                Chat.TraceMessage(Chat.TraceDebug, string.Format("Offline messages is received. tenantId={0}", tenantId));
                //sendOfflineMessages
                context.Clients.Group(tenantId.ToString(CultureInfo.InvariantCulture) + callerUserName).som(users);
            }
            catch (Exception e)
            {
                Chat.TraceMessage(Chat.TraceError, string.Format("Unknown Error. callerUserName={0}, {1}, {2}, {3}", callerUserName,
                    e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

    }
}