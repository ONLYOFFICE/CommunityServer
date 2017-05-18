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


using ASC.Common.Utils;
using ASC.Core;
using log4net;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Owin;
using System;
using System.Security.Authentication;
using System.Security.Principal;
using System.Threading;

namespace ASC.SignalR.Base.Hubs
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class AuthorizeHubAttribute : AuthorizeAttribute
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AuthorizeHubAttribute));
        const string tokenKey = "token";

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
            var token = request.QueryString[tokenKey];

            if (string.IsNullOrEmpty(token))
            {
                token = request.Headers[tokenKey];
            }

            var data = Signature.Read<string>(token);

            if (string.IsNullOrEmpty(data))
            {
                log.Debug("data is empty");
                return false;
            }
            var dataSplit = data.Split(',');
            log.DebugFormat("domain={0}, userId={1}", dataSplit[2], dataSplit[1]);
            var tenant = CoreContext.TenantManager.GetTenant(dataSplit[2]);

            CoreContext.TenantManager.SetCurrentTenant(tenant);

            string cookie;
            var userId = new Guid(dataSplit[1]);
            try
            {
                cookie = SecurityContext.AuthenticateMe(userId);
            }
            catch (InvalidCredentialException ex)
            {
                log.DebugFormat("InvalidCredentialException ex = {0}. Clear caching of users", ex.ToString());
                CoreContext.UserManager.ClearCache();
                cookie = SecurityContext.AuthenticateMe(userId);
            }
            request.Environment["server.User"] = Thread.CurrentPrincipal;
            request.Environment["server.UserCookie"] = cookie;
            return true;
        }
    }
}
