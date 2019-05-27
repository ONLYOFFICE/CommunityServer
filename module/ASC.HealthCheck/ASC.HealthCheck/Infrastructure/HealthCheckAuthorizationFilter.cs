/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.HealthCheck.Settings;
using log4net;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ASC.HealthCheck.Infrastructure
{
    public class HealthCheckAuthorizationFilter : AuthorizationFilterAttribute
    {
        private readonly ILog log = LogManager.GetLogger(typeof(HealthCheckAuthorizationFilter));

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            JsonMediaTypeFormatter jsonMediaTypeFormatter = new JsonMediaTypeFormatter();
            try
            {
                log.Debug("HealthCheckAuthorizationFilterAttribute OnActionExecuted");
                /*
                var healthCheckSettingsAccessor = new HealthCheckSettingsAccessor();
                var healthCheckSettings = healthCheckSettingsAccessor.GetHealthCheckSettings();
                
                var tenants = CoreContext.TenantManager.GetTenants().Where(t => t.TenantId != healthCheckSettings.FakeTenantId).ToList();

                if (!CoreContext.TenantManager.GetTenantQuota(tenants.First().TenantId).HealthCheck)
                {
                    log.Debug("There is no correct license for HealthCheck.");

                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        Content = new ObjectContent<object>(HealthCheckResource.ErrorNotAllowedOption, jsonMediaTypeFormatter)
                    };

                    return;
                }
                */
                string authorizationHeaderValue = null;
                foreach (var header in actionContext.Request.Headers)
                {
                    if (header.Key == "Authorization")
                    {
                        if (header.Value == null)
                        {
                            log.Error("User Unauthorized, Authorization header.Value is null");
                            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                            return;
                        }
                        foreach (var headerValue in header.Value)
                        {
                            authorizationHeaderValue = headerValue;
                        }
                        break;
                    }
                }
                if (authorizationHeaderValue == null)
                {
                    log.Error("User Unauthorized, authorizationHeaderValue is null");
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return;
                }
                var authorization = authorizationHeaderValue.Split(',');
                if (authorization.Length != 2)
                {
                    log.ErrorFormat("User Unauthorized, authorization is null or authorization.Length = {0}", authorization.Length);
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
                    return;
                }
                var portalInfo = authorization[0].Split(' ');
                if (portalInfo.Length == 2)
                {
                    authorization[0] = portalInfo[1];
                }
                CoreContext.TenantManager.SetCurrentTenant(authorization[0]);
                SecurityContext.AuthenticateMe(authorization[1]);
                ResolveUserCulture();
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Unexpected error on HealthCheckAuthorizationFilterAttribute: {0} {1}",
                    ex.ToString(), ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new ObjectContent<object>(ex.ToString(), jsonMediaTypeFormatter)
                };
            }
        }

        private void ResolveUserCulture()
        {
            CultureInfo culture = null;

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null)
            {
                culture = tenant.GetCulture();
            }

            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            if (!string.IsNullOrEmpty(user.CultureName))
            {
                culture = CultureInfo.GetCultureInfo(user.CultureName);
            }

            if (culture != null && !Equals(Thread.CurrentThread.CurrentCulture, culture))
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
            if (culture != null && !Equals(Thread.CurrentThread.CurrentUICulture, culture))
            {
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }
    }
}