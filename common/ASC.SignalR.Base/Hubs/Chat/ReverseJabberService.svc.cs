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

using System.Globalization;
using ASC.Core;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Notify.Jabber;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Activation;
using ASC.Core.Tenants;

namespace ASC.SignalR.Base.Hubs.Chat
{
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, IncludeExceptionDetailInFaults = true,
        InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
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
                    UserName = isTenantUser ? tenant.GetTenantDomain(false) : callerUserName,
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
                    UserName = tenant.GetTenantDomain(false),
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
                Chat.States.Add(tenantId, from, state);
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