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

using System;
using System.Security.Principal;
using System.Threading;
using System.Web;
using ASC.Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin;

namespace ASC.SignalR.Base.Hubs
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class AuthorizeHubAttribute : AuthorizeAttribute
    {
        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            return Authorize(request);
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        { 
            var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;

            var environment = hubIncomingInvokerContext.Hub.Context.Request.Environment;

            var principal = environment["server.User"] as GenericPrincipal;

            if (principal != null && principal.Identity.IsAuthenticated)
            {
                hubIncomingInvokerContext.Hub.Context = new HubCallerContext(new ServerRequest(environment), connectionId);
                return true;
            }

            return false;
        }

        public static bool Authorize(IRequest request)
        {
            try
            {
                var data = Common.Utils.Signature.Read<string>(request.QueryString["token"]);
                if (string.IsNullOrEmpty(data))
                    return false;
                var dataSplit = data.Split(',');
                var tenant = CoreContext.TenantManager.GetTenant(dataSplit[2]);
                CoreContext.TenantManager.SetCurrentTenant(tenant);
                var cookie = SecurityContext.AuthenticateMe(new Guid(dataSplit[1]));
                request.Environment["server.UserCookie"] = cookie;
                request.Environment["server.User"] = Thread.CurrentPrincipal;
                return true;
            }
            catch (Exception e)
            {
                Chat.Chat.TraceMessage(Chat.Chat.TraceError, string.Format("Message:{0}, StackTrace:{1}", e.Message, e.StackTrace));
                return false;
            }
        }
    }
}
