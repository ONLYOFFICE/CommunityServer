/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Web.Core.Utility;
using ASC.Web.Studio;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Api.Security
{
    /// <summary>
    /// Security API.
    /// </summary>
    /// <name>security</name>
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

        /// <summary>
        /// Returns all the latest user login activity including successful logins and failed attempts with an indication of reasons.
        /// </summary>
        /// <short>
        /// Get login history
        /// </short>
        /// <category>Login history</category>
        /// <returns>List of login events</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/security/audit/login/last</path>
        /// <httpMethod>GET</httpMethod>
        [Read("/audit/login/last")]
        public IEnumerable<LoginEventWrapper> GetLastLoginEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandBaseAuditPermission();

            return LoginEventsRepository.GetByFilter(startIndex: 0, limit: 20).Select(x => new LoginEventWrapper(x));
        }

        /// <summary>
        /// Returns a list of the latest changes (creation, modification, deletion, etc.) made by users to the entities (tasks, opportunities, files, etc.) on the portal.
        /// </summary>
        /// <short>
        /// Get audit trail data
        /// </short>
        /// <category>Audit trail data</category>
        /// <returns>List of audit trail data</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/security/audit/events/last</path>
        /// <httpMethod>GET</httpMethod>
        [Read("/audit/events/last")]
        public IEnumerable<AuditEventWrapper> GetLastAuditEvents()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandBaseAuditPermission();

            return AuditEventsRepository.GetByFilter(startIndex: 0, limit: 20).Select(x => new AuditEventWrapper(x));
        }

        /// <summary>
        /// Returns a list of the login events by the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Get filtered login events
        /// </short>
        /// <category>Login history</category>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <param type="ASC.MessagingSystem.MessageAction, ASC.MessagingSystem" name="action">Action</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="from">Start date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="to">End date</param>
        /// <returns>List of filtered login events</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/security/audit/login/filter</path>
        /// <httpMethod>GET</httpMethod>
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
        /// Returns a list of the audit events by the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Get filtered audit trail data
        /// </short>
        /// <category>Audit trail data</category>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <param type="ASC.AuditTrail.Types.ProductType, ASC.AuditTrail.Types" name="productType">Product</param>
        /// <param type="ASC.AuditTrail.Types.ModuleType, ASC.AuditTrail.Types" name="moduleType">Module</param>
        /// <param type="ASC.AuditTrail.Types.ActionType, ASC.AuditTrail.Types" name="actionType">Action type</param>
        /// <param type="ASC.MessagingSystem.MessageAction, ASC.MessagingSystem" name="action">Action</param>
        /// <param type="ASC.AuditTrail.Types.EntryType, ASC.AuditTrail.Types" name="entryType">Entry</param>
        /// <param type="System.String, System" name="target">Target</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="from">Start date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="to">End date</param>
        /// <returns>List of filtered audit trail data</returns>
        /// <collection>list</collection>
        /// <path>api/2.0/security/audit/events/filter</path>
        /// <httpMethod>GET</httpMethod>
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

        /// <summary>
        /// Returns all the available audit trail types.
        /// </summary>
        /// <short>
        /// Get audit trail types
        /// </short>
        /// <category>Audit trail data</category>
        /// <returns>Audit trail types</returns>
        /// <path>api/2.0/security/audit/types</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <httpMethod>GET</httpMethod>
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

        /// <summary>
        /// Returns the mappers for the audit trail types.
        /// </summary>
        /// <short>
        /// Get audit trail mappers
        /// </short>
        /// <category>Audit trail data</category>
        /// <param type="System.Nullable{ASC.AuditTrail.Types.ProductType}, Systems" name="productType">Product</param>
        /// <param type="System.Nullable{ASC.AuditTrail.Types.ModuleType}, System" name="moduleType">Module</param>
        /// <returns>Audit trail mappers</returns>
        /// <path>api/2.0/security/audit/mappers</path>
        /// <requiresAuthorization>false</requiresAuthorization>
        /// <httpMethod>GET</httpMethod>
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

        /// <summary>
        /// Generates the login history report.
        /// </summary>
        /// <short>
        /// Generate the login history report
        /// </short>
        /// <category>Login history</category>
        /// <returns>URL to the xlsx report file</returns>
        /// <path>api/2.0/security/audit/login/report</path>
        /// <httpMethod>POST</httpMethod>
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

        /// <summary>
        /// Generates the audit trail report.
        /// </summary>
        /// <short>
        /// Generate the audit trail report
        /// </short>
        /// <category>Audit trail data</category>
        /// <returns>URL to the xlsx report file</returns>
        /// <path>api/2.0/security/audit/events/report</path>
        /// <httpMethod>POST</httpMethod>
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

        /// <summary>
        /// Returns the audit trail settings.
        /// </summary>
        /// <short>
        /// Get the audit trail settings
        /// </short>
        /// <category>Audit trail data</category>
        /// <returns>Audit settings</returns>
        /// <path>api/2.0/security/audit/settings/lifetime</path>
        /// <httpMethod>GET</httpMethod>
        [Read("/audit/settings/lifetime")]
        public TenantAuditSettings GetAuditSettings()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandBaseAuditPermission();

            return TenantAuditSettings.LoadForTenant(TenantProvider.CurrentTenantID);
        }

        /// <summary>
        /// Sets the audit trail settings for the current portal.
        /// </summary>
        /// <short>
        /// Set the audit trail settings
        /// </short>
        /// <category>Audit trail data</category>
        /// <param type="ASC.Core.Tenants.TenantAuditSettings, ASC.Core.Tenants" name="settings">Audit trail settings</param>
        /// <returns>Audit trail settings</returns>
        /// <path>api/2.0/security/audit/settings/lifetime</path>
        /// <httpMethod>POST</httpMethod>
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

        /// <summary>
        /// Returns all the active connections to the portal.
        /// </summary>
        /// <short>
        /// Get active connections
        /// </short>
        /// <category>Active connections</category>
        /// <returns>Active portal connections</returns>
        /// <path>api/2.0/security/activeconnections</path>
        /// <httpMethod>GET</httpMethod>
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
                    var ip = MessageSettings.GetFullIPAddress(request);

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

        /// <summary>
        /// Logs out from all the active connections for the current user and changes their password.
        /// </summary>
        /// <short>
        /// Log out and change password
        /// </short>
        /// <category>Active connections</category>
        /// <returns>URL to the confirmation message for changing a password</returns>
        /// <path>api/2.0/security/activeconnections/logoutallchangepassword</path>
        /// <httpMethod>PUT</httpMethod>
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

        /// <summary>
        /// Logs out from all the active connections for the user with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Log out for the user by ID
        /// </short>
        /// <category>Active connections</category>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <path>api/2.0/security/activeconnections/logoutall/{userId}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <returns></returns>
        [Update("/activeconnections/logoutall/{userId}")]
        public void LogOutAllActiveConnectionsForUser(Guid userId)
        {
            if (!WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID))
                throw new SecurityException("Method not available");

            LogOutAllActiveConnections(userId);
        }

        /// <summary>
        /// Logs out from all the active connections except the current connection.
        /// </summary>
        /// <short>
        /// Log out from all connections
        /// </short>
        /// <category>Active connections</category>
        /// <returns>Current user name</returns>
        /// <path>api/2.0/security/activeconnections/logoutallexceptthis</path>
        /// <httpMethod>PUT</httpMethod>
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

        /// <summary>
        /// Logs out from the connection with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Log out from the connection
        /// </short>
        /// <category>Active connections</category>
        /// <param type="System.Int32, System" name="loginEventId">Login event ID</param>
        /// <returns>Boolean value: true if the operation is successful</returns>
        /// <path>api/2.0/security/activeconnections/logout/{loginEventId}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("/activeconnections/logout/{loginEventId}")]
        public bool LogOutActiveConnection(int loginEventId)
        {
            try
            {
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                var loginEvent = DbLoginEventsManager.GetLoginEvent(user.Tenant, loginEventId);

                if (loginEvent == null)
                {
                    return false;
                }

                if (loginEvent.UserId != user.ID && !WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, user.ID))
                {
                    throw new SecurityException("Method not available");
                }

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

        /// <summary>
        /// Updates the login settings with the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Update login settings
        /// </short>
        /// <category>Login history</category>
        /// <param type="System.Int32, System" name="attemptsCount">Maximum number of the user attempts to log in</param>
        /// <param type="System.Int32, System" name="blockTime">The time for which the user will be blocked after unsuccessful login attempts</param>
        /// <param type="System.Int32, System" name="checkPeriod">The time to wait for a response from the server</param>
        /// <returns>Updated login settings</returns>
        /// <path>api/2.0/security/loginsettings</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("/loginsettings")]
        public LoginSettings UpdateLoginSettings(int attemptsCount, int blockTime, int checkPeriod)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (attemptsCount < 1)
            {
                throw new ArgumentOutOfRangeException("attemptsCount");
            }
            if (checkPeriod < 1)
            {
                throw new ArgumentOutOfRangeException("checkPeriod");
            }
            if (blockTime < 1)
            {
                throw new ArgumentOutOfRangeException("blockTime");
            }


            var settings = new LoginSettings { CheckPeriod = checkPeriod, AttemptCount = attemptsCount, BlockTime = blockTime };
            settings.Save();

            return settings;
        }

        /// <summary>
        /// Returns the impersonation settings for the current portal.
        /// </summary>
        /// <short>
        /// Get impersonation settings
        /// </short>
        /// <category>Impersonation</category>
        /// <returns>Impersonation settings</returns>
        /// <path>api/2.0/security/impersonate/settings</path>
        /// <httpMethod>GET</httpMethod>
        [Read("/impersonate/settings")]
        public ImpersonationSettings GetImpersonateSettings()
        {
            CheckImpersonateSettingsPermissions();

            var settings = ImpersonationSettings.LoadAndRefresh();
            return settings;
        }

        /// <summary>
        /// Uppdates the impersonation settings with the parameters specified in the request.
        /// </summary>
        /// <short>
        /// Update impersonation settings
        /// </short>
        /// <category>Impersonation</category>
        /// <param type="System.Boolean, System" name="enable">Specifies whether impersonation is enabled or not</param>
        /// <param type="ASC.Web.Core.Utility.ImpersonateEnableType, ASC.Web.Core.Utility" name="enableType">Specifies for whom impersonation is enabled (DisableForAdmins, EnableForAllFullAdmins, or EnableWithLimits)</param>
        /// <param type="System.Boolean, System" name="onlyForOwnGroups">Specifies if impersonation is enabled only for the current user groups or not</param>
        /// <param type="System.Collections.Generic.List{System.Guid}, System" name="allowedAdmins">List of admins who can be impersonated</param>
        /// <param type="System.Collections.Generic.List{System.Guid}, System" name="restrictionUsers">List of users who cannot be impersonated</param>
        /// <param type="System.Collections.Generic.List{System.Guid}, System" name="restrictionGroups">List of groups who cannot be impersonated</param>
        /// <returns>Updated impersonation settings</returns>
        /// <path>api/2.0/security/impersonate/settings</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("/impersonate/settings")]
        public ImpersonationSettings UpdateImpersonateSettings(bool enable, ImpersonateEnableType enableType, bool onlyForOwnGroups, List<Guid> allowedAdmins, List<Guid> restrictionUsers, List<Guid> restrictionGroups)
        {
            CheckImpersonateSettingsPermissions();

            var settings = !enable ?
                new ImpersonationSettings() :
                new ImpersonationSettings
                {
                    Enabled = enable,
                    EnableType = enableType,
                    OnlyForOwnGroups = onlyForOwnGroups,
                    AllowedAdmins = allowedAdmins,
                    RestrictionUsers = restrictionUsers,
                    RestrictionGroups = restrictionGroups
                };

            settings.Save();

            return settings;
        }

        /// <summary>
        /// Checks if a user with the ID specified in the request can be impersonated or not.
        /// </summary>
        /// <short>
        /// Check user impersonation
        /// </short>
        /// <category>Impersonation</category>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <returns>Boolean value: true - the user can be impersonated, false - the user cannot be impersonated</returns>
        /// <path>api/2.0/security/impersonate/{userId}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("/impersonate/{userId}")]
        public bool CanImpersonateUser(Guid userId)
        {
            return ImpersonationSettings.CanImpersonateUser(userId);
        }

        /// <summary>
        /// Impersonates a user with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Impersonate a user
        /// </short>
        /// <category>Impersonation</category>
        /// <param type="System.Guid, System" name="userId">User ID</param>
        /// <returns>Cookies</returns>
        /// <path>api/2.0/security/impersonate/{userId}</path>
        /// <httpMethod>POST</httpMethod>
        [Create("/impersonate/{userId}")]
        public string ImpersonateUser(Guid userId)
        {
            if (!ImpersonationSettings.CanImpersonateUser(userId))
            {
                throw new SecurityException("Impossible to impersonate this user");
            }

            var currentTenantId = TenantProvider.CurrentTenantID;

            var currentUserId = SecurityContext.CurrentAccount.ID;
            var currentUser = CoreContext.UserManager.GetUsers(currentUserId);
            var currentUserName = currentUser.DisplayUserName(false);

            var targetUser = CoreContext.UserManager.GetUsers(userId);
            var targetUserName = targetUser.DisplayUserName(false);

            if (!ImpersonationSettings.IsImpersonator())
            {
                var currentAuthCookies = CookiesManager.GetCookies(CookiesType.AuthKey);
                CookiesManager.SetCookies(CookiesType.ComebackAuthKey, currentAuthCookies);
            }

            var cookies = CookiesManager.AuthenticateMeAndSetCookies(currentTenantId, userId, MessageAction.LoginSuccess);

            var userData = new MessageUserData(currentTenantId, currentUser.ID);

            var httpHeaders = HttpContext.Current.Request.Headers.AllKeys.ToDictionary(key => key, key => HttpContext.Current.Request.Headers[key]);

            MessageService.Send(userData, httpHeaders, MessageAction.ImpersonateUserLogin, MessageTarget.Create(userId), currentUserName, targetUserName);

            return cookies;
        }

        /// <summary>
        /// Log out from the account of the impersonated user.
        /// </summary>
        /// <short>
        /// Log out impersonated user
        /// </short>
        /// <category>Impersonation</category>
        /// <path>api/2.0/security/impersonate/logout</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("/impersonate/logout")]
        public void ImpersonateLogout()
        {
            var targetUserId = SecurityContext.CurrentAccount.ID;
            var targetUser = CoreContext.UserManager.GetUsers(targetUserId);
            var targetUserName = targetUser.DisplayUserName(false);

            var currentAuthCookies = CookiesManager.GetCookies(CookiesType.AuthKey);
            var comebackAuthCookies = CookiesManager.GetCookies(CookiesType.ComebackAuthKey);

            var loginEventId = CookieStorage.GetLoginEventIdFromCookie(currentAuthCookies);
            DbLoginEventsManager.LogOutEvent(loginEventId);

            MessageService.Send(HttpContext.Current.Request, targetUserName, MessageAction.Logout);

            CookiesManager.ClearCookies(CookiesType.ComebackAuthKey);

            Auth.ProcessLogout();

            if (SecurityContext.AuthenticateMe(comebackAuthCookies))
            {
                CookiesManager.SetCookies(CookiesType.AuthKey, comebackAuthCookies);

                var currentUserId = SecurityContext.CurrentAccount.ID;
                var currentUser = CoreContext.UserManager.GetUsers(currentUserId);
                var currentUserName = currentUser.DisplayUserName(false);

                MessageService.Send(HttpContext.Current.Request, currentUserName, MessageAction.ImpersonateUserLogout, MessageTarget.Create(targetUserId), targetUserName);
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

        private void CheckImpersonateSettingsPermissions()
        {
            if (!ImpersonationSettings.Available)
            {
                throw new SecurityException("Setting is not available");
            }

            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!currentUser.IsOwner())
            {
                throw new SecurityException("Setting available only for the owner");
            }
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