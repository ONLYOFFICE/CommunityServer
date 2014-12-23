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

using ASC.Common.Utils;
using ASC.Core;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin;
using System;
using System.Security.Principal;
using System.Threading;

namespace ASC.SignalR.Base.Hubs
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class AuthorizeHubAttribute : AuthorizeAttribute
    {
        private readonly static ILog log = LogManager.GetLogger(typeof(AuthorizeHubAttribute));

        public override bool AuthorizeHubConnection(HubDescriptor hubDescriptor, IRequest request)
        {
            try
            {
                return Authorize(request);
            }
            catch (Exception e)
            {
                log.ErrorFormat("AuthorizeHubConnection error: {0}, {1}, {2}", e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
                return false;
            }
        }

        public override bool AuthorizeHubMethodInvocation(IHubIncomingInvokerContext hubIncomingInvokerContext, bool appliesToMethod)
        {
            try
            {
                var connectionId = hubIncomingInvokerContext.Hub.Context.ConnectionId;

                var environment = hubIncomingInvokerContext.Hub.Context.Request.Environment;

                var principal = environment["server.User"] as GenericPrincipal;

                if (principal != null && principal.Identity.IsAuthenticated)
                {
                    hubIncomingInvokerContext.Hub.Context = new HubCallerContext(new ServerRequest(environment), connectionId);
                    return true;
                }
            }
            catch (Exception e)
            {
                log.ErrorFormat("AuthorizeHubMethodInvocation error: {0}, {1}, {2}", e, e.StackTrace, e.InnerException != null ? e.InnerException.Message : string.Empty);
                return false;
            }
            return false;
        }

        public static bool Authorize(IRequest request)
        {
            var data = Signature.Read<string>(request.QueryString["token"]);

            if (string.IsNullOrEmpty(data))
            {
                return false;
            }
            var dataSplit = data.Split(',');
            log.DebugFormat("domain={0}, userId={1}", dataSplit[2], dataSplit[1]);
            var tenant = CoreContext.TenantManager.GetTenant(dataSplit[2]);

            CoreContext.TenantManager.SetCurrentTenant(tenant);
            var cookie = SecurityContext.AuthenticateMe(new Guid(dataSplit[1]));
            request.Environment["server.User"] = Thread.CurrentPrincipal;
            request.Environment["server.UserCookie"] = cookie;
            return true;
        }
    }
}
