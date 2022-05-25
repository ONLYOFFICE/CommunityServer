/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.AuditTrail;
using ASC.AuditTrail.Data;
using ASC.AuditTrail.Mappers;
using ASC.AuditTrail.Types;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Data;
using ASC.Core.Security.Authentication;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Geolocation;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Web.Core;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Security
{
    public class SecurityApi : IApiEntryPoint
    {
        ILog Log = LogManager.GetLogger("ASC.Api");
        GeolocationHelper geoLocHelper = new GeolocationHelper("teamlabsite");
        protected internal ApiContext Context { get; set; }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        public string Name
        {
            get { return "security"; }
        }

        public SecurityApi(ApiContext apiContext)
        {
            Context = apiContext;
        }

        [Read("/audit/login/last")]
        public IEnumerable<LoginEventWrapper> GetLastLoginEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandBaseAuditPermission();

            return LoginEventsRepository.GetByFilter(startIndex: 0, limit: 20).Select(x => new LoginEventWrapper(x));
        }

        [Read("/audit/events/last")]
        public IEnumerable<AuditEventWrapper> GetLastAuditEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandBaseAuditPermission();

            return AuditEventsRepository.GetByFilter(startIndex: 0, limit: 20).Select(x => new AuditEventWrapper(x));
        }

        /// <summary>
        /// Returns a list of login events by filter
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="action">Action</param>
        /// <param name="from">From date</param>
        /// <param name="to">To date</param>
        /// <returns>Events</returns>
        [Read("/audit/login/filter")]
        public IEnumerable<LoginEventWrapper> GetLoginEventsByFilter(Guid userId,
            MessageAction action,
            ApiDateTime from,
            ApiDateTime to)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var startIndex = (int)Context.StartIndex;
            var limit = (int)Context.Count;
            Context.SetDataPaginated();

            action = action == 0 ? MessageAction.None : action;

            if (!TenantExtra.GetTenantQuota().Audit || !SetupInfo.IsVisibleSettings(ManagementType.LoginHistory.ToString()))
            {
                return GetLastLoginEvents();
            }
            else
            {
                DemandAuditPermission();

                return LoginEventsRepository.GetByFilter(userId, action, from, to, startIndex, limit).Select(x => new LoginEventWrapper(x));
            }
        }
        /// <summary>
        /// Returns a list of audit events by filter
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="productType">Product</param>
        /// <param name="moduleType">Module</param>
        /// <param name="actionType">Action type</param>
        /// <param name="action">Action</param>
        /// <param name="entryType">Entry</param>
        /// <param name="target">Target</param>
        /// <param name="from">From date</param>
        /// <param name="to">To date</param>
        /// <returns>Actions</returns>
        [Read("/audit/events/filter")]
        public IEnumerable<AuditEventWrapper> GetAuditEventsByFilter(Guid userId,
            ProductType productType,
            ModuleType moduleType,
            ActionType actionType,
            MessageAction action,
            EntryType entryType,
            string target,
            ApiDateTime from,
            ApiDateTime to)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var startIndex = (int)Context.StartIndex;
            var limit = (int)Context.Count;
            Context.SetDataPaginated();

            action = action == 0 ? MessageAction.None : action;

            if (!TenantExtra.GetTenantQuota().Audit || !SetupInfo.IsVisibleSettings(ManagementType.LoginHistory.ToString()))
            {
                return GetLastAuditEvents();
            }
            else
            {
                DemandAuditPermission();

                return AuditEventsRepository.GetByFilter(userId, productType, moduleType, actionType, action, entryType, target, from, to, startIndex, limit).Select(x => new AuditEventWrapper(x));
            }
        }

        [Read("/audit/types", false)]
        public ModelTypes GetTypes()
        {
            return new ModelTypes()
            {
                Actions = Enum.GetValues(typeof(MessageAction)).Cast<MessageAction>().Select(x => x.ToString()),
                ActionTypes = Enum.GetValues(typeof(ActionType)).Cast<ActionType>().Select(x => x.ToString()),
                ProductTypes = Enum.GetValues(typeof(ProductType)).Cast<ProductType>().Select(x => x.ToString()),
                ModuleTypes = Enum.GetValues(typeof(ModuleType)).Cast<ModuleType>().Select(x => x.ToString()),
                EntryTypes = Enum.GetValues(typeof(EntryType)).Cast<EntryType>().Select(x => x.ToString())
            };
        }

        [Read("/audit/mappers", false)]
        public object GetMappers(ProductType? productType, ModuleType? moduleType)
        {
            return AuditActionMapper.Mappers
                .Where(r => !productType.HasValue || r.Product == productType.Value)
                .Select(r => new
                {
                    ProductType = r.Product.ToString(),
                    Modules = r.Mappers
                    .Where(m => !moduleType.HasValue || m.Module == moduleType.Value)
                    .Select(x => new
                    {
                        ModuleType = x.Module.ToString(),
                        Actions = x.Actions.Select(a => new
                        {
                            MessageAction = a.Key.ToString(),
                            ActionType = a.Value.ActionType.ToString(),
                            Entity = a.Value.EntryType1.ToString()
                        })
                    })
                });
        }

        [Create("/audit/login/report")]
        public string CreateLoginHistoryReport()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandAuditPermission();

            var tenantId = TenantProvider.CurrentTenantID;

            var settings = TenantAuditSettings.LoadForTenant(tenantId);

            var to = DateTime.UtcNow;
            var from = to.Subtract(TimeSpan.FromDays(settings.LoginHistoryLifeTime));

            var reportName = string.Format(AuditReportResource.LoginHistoryReportName + ".csv", from.ToShortDateString(), to.ToShortDateString());
            var events = LoginEventsRepository.GetByFilter(from: from, to: to);
            var result = AuditReportCreator.CreateCsvReport(events, reportName);

            MessageService.Send(Request, MessageAction.LoginHistoryReportDownloaded);
            return result;
        }

        [Create("/audit/events/report")]
        public string CreateAuditTrailReport()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandAuditPermission();

            var tenantId = TenantProvider.CurrentTenantID;

            var settings = TenantAuditSettings.LoadForTenant(tenantId);

            var to = DateTime.UtcNow;
            var from = to.Subtract(TimeSpan.FromDays(settings.AuditTrailLifeTime));

            var reportName = string.Format(AuditReportResource.AuditTrailReportName + ".csv", from.ToShortDateString(), to.ToShortDateString());

            var events = AuditEventsRepository.GetByFilter(from: from, to: to);
            var result = AuditReportCreator.CreateCsvReport(events, reportName);

            MessageService.Send(Request, MessageAction.AuditTrailReportDownloaded);
            return result;
        }

        [Read("/audit/settings/lifetime")]
        public TenantAuditSettings GetAuditSettings()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandBaseAuditPermission();

            return TenantAuditSettings.LoadForTenant(TenantProvider.CurrentTenantID);
        }

        [Create("/audit/settings/lifetime")]
        public TenantAuditSettings SetAuditSettings(TenantAuditSettings settings)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandAuditPermission();

            if (settings.LoginHistoryLifeTime <= 0 || settings.LoginHistoryLifeTime > TenantAuditSettings.MaxLifeTime)
                throw new ArgumentException("LoginHistoryLifeTime");

            if (settings.AuditTrailLifeTime <= 0 || settings.AuditTrailLifeTime > TenantAuditSettings.MaxLifeTime)
                throw new ArgumentException("AuditTrailLifeTime");

            settings.SaveForTenant(TenantProvider.CurrentTenantID);

            MessageService.Send(Request, MessageAction.AuditSettingsUpdated);

            return settings;
        }

        private static void DemandAuditPermission()
        {
            if (!CoreContext.Configuration.Standalone
                && (!SetupInfo.IsVisibleSettings(ManagementType.LoginHistory.ToString())
                    || !TenantExtra.GetTenantQuota().Audit))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Audit");
            }
        }

        private static void DemandBaseAuditPermission()
        {
            if (!CoreContext.Configuration.Standalone
                && !SetupInfo.IsVisibleSettings(ManagementType.LoginHistory.ToString()))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Audit");
            }
        }

        [Read("/activeconnections")]
        public object GetAllActiveConnections()
        {
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            var loginEvents = DbLoginEventsManager.GetLoginEvents(user.Tenant, user.ID);
            var listLoginEvents = loginEvents.ConvertAll(Convert);
            var loginEventId = GetLoginEventIdFromCookie();
            if (loginEventId != 0)
            {
                var loginEvent = listLoginEvents.FirstOrDefault(x => x.Id == loginEventId);
                if (loginEvent != null)
                {
                    listLoginEvents.Remove(loginEvent);
                    listLoginEvents.Insert(0, loginEvent);
                }
            }
            else
            {
                if (listLoginEvents.Count == 0)
                {
                    var request = HttpContext.Current.Request;
                    var uaHeader = MessageSettings.GetUAHeader(request);
                    var clientInfo = MessageSettings.GetClientInfo(uaHeader);
                    var platformAndDevice = MessageSettings.GetPlatformAndDevice(clientInfo);
                    var browser = MessageSettings.GetBrowser(clientInfo);
                    var ip = MessageSettings.GetIP(request);

                    var baseEvent = new CustomEvent
                    {
                        Id = 0,
                        Platform = platformAndDevice,
                        Browser = browser,
                        Date = DateTime.Now,
                        IP = ip
                    };

                    listLoginEvents.Add(Convert(baseEvent));
                }
            }
            
            var result = new
            {
                Items = listLoginEvents,
                LoginEvent = loginEventId
            };
            return result;
        }

        [Update("/activeconnections/logoutallchangepassword")]
        public string LogOutAllActiveConnectionsChangePassword()
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var userName = user.DisplayUserName(false);

                LogOutAllActiveConnections(user.ID);

                Auth.ProcessLogout();

                var auditEventDate = DateTime.UtcNow;
                var hash = auditEventDate.ToString("s");
                var confirmationUrl = CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.PasswordChange, hash);
                MessageService.Send(Request, auditEventDate, MessageAction.UserSentPasswordChangeInstructions, MessageTarget.Create(user.ID), userName);

                return confirmationUrl;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        [Update("/activeconnections/logoutall/{userId}")]
        public void LogOutAllActiveConnectionsForUser(Guid userId)
        {
            if (!CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()
                && !WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID))
                throw new SecurityException("Method not available");

            LogOutAllActiveConnections(userId);
        }

        [Update("/activeconnections/logoutallexceptthis")]
        public string LogOutAllExceptThisConnection()
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var userName = user.DisplayUserName(false);
                var loginEventFromCookie = GetLoginEventIdFromCookie();

                DbLoginEventsManager.LogOutAllActiveConnectionsExceptThis(loginEventFromCookie, user.Tenant, user.ID);

                MessageService.Send(Request, MessageAction.UserLogoutActiveConnections, userName);
                return userName;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return null;
            }
        }

        [Update("/activeconnections/logout/{loginEventId}")]
        public bool LogOutActiveConnection(int loginEventId)
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var userName = user.DisplayUserName(false);

                DbLoginEventsManager.LogOutEvent(loginEventId);

                MessageService.Send(Request, MessageAction.UserLogoutActiveConnection, userName);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }

        public void LogOutAllActiveConnections(Guid? userId = null)
        {
            var currentUserId = SecurityContext.CurrentAccount.ID;
            var user = CoreContext.UserManager.GetUsers(userId ?? currentUserId);
            var userName = user.DisplayUserName(false);
            var auditEventDate = DateTime.UtcNow;

            MessageService.Send(Request, auditEventDate,
                (currentUserId.Equals(user.ID)) ? MessageAction.UserLogoutActiveConnections : MessageAction.UserLogoutActiveConnectionsForUser,
                MessageTarget.Create(user.ID), userName);
            CookiesManager.ResetUserCookie(user.ID);
        }

        public int GetLoginEventIdFromCookie()
        {
            var cookie = CookiesManager.GetCookies(CookiesType.AuthKey);
            int loginEventId = CookieStorage.GetLoginEventIdFromCookie(cookie);
            return loginEventId;
        }

        private CustomEvent Convert(BaseEvent baseEvent)
        {
            var location = GetGeolocation(baseEvent.IP);
            return new CustomEvent
            {
                Id = baseEvent.Id,
                IP = baseEvent.IP,
                Platform = baseEvent.Platform,
                Browser = baseEvent.Browser,
                Date = baseEvent.Date,
                Country = location[0],
                City = location[1]
            };
        }

        private string[] GetGeolocation(string ip)
        {
            try
            {
                var location = geoLocHelper.GetIPGeolocation(ip);
                if (string.IsNullOrEmpty(location.Key))
                {
                    return new string[] { string.Empty, string.Empty };
                }
                var regionInfo = new RegionInfo(location.Key).EnglishName;
                return new string[] { regionInfo, location.City };
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return new string[] { string.Empty, string.Empty };
            }
        }

        private class CustomEvent : BaseEvent
        {
            public string Country { get; set; }

            public string City { get; set; }
        }
    }
}