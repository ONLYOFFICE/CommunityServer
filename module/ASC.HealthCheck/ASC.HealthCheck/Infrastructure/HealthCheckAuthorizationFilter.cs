/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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