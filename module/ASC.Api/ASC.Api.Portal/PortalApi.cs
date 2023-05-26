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
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
using ASC.Common.Web;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Contracts;
using ASC.Core.Common.Notify.Jabber;
using ASC.Core.Common.Notify.Push;
using ASC.Core.Notify.Jabber;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.ElasticSearch;
using ASC.ElasticSearch.Core;
using ASC.Geolocation;
using ASC.MessagingSystem;
using ASC.Security.Cryptography;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;
using UrlShortener = ASC.Web.Core.Utility.UrlShortener;

namespace ASC.Api.Portal
{
    ///<summary>
    /// Portal information access.
    ///</summary>
    ///<name>portal</name>
    public class PortalApi : IApiEntryPoint
    {
        private readonly IMobileAppInstallRegistrator mobileAppRegistrator;
        private readonly BackupAjaxHandler backupHandler = new BackupAjaxHandler();
        private ILog Log = LogManager.GetLogger("ASC");
        private ILog LogWeb = LogManager.GetLogger("ASC.Web");
        private static HttpClient httpClient = HttpClientFactory.CreateClient(nameof(PortalApi));


        ///<summary>
        /// Api name entry
        ///</summary>
        public string Name
        {
            get { return "portal"; }
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        public PortalApi(ApiContext context)
        {
            mobileAppRegistrator = new CachedMobileAppInstallRegistrator(new MobileAppInstallRegistrator());
        }

        /// <summary>
        /// Returns the current portal.
        /// </summary>
        /// <short>
        /// Get the current portal
        /// </short>
        /// <category>Settings</category>
        /// <returns type="ASC.Core.Tenants.Tenant, ASC.Core.Common">Portal</returns>
        ///  <path>api/2.0/portal</path>
        /// <httpMethod>GET</httpMethod>
        [Read("")]
        public Tenant Get()
        {
            return CoreContext.TenantManager.GetCurrentTenant();
        }

        /// <summary>
        /// Returns a user with the ID specified in the request from the current portal.
        /// </summary>
        /// <short>
        /// Get a user by ID
        /// </short>
        /// <category>Users</category>
        /// <param type="System.Guid, System" name="userID">User ID</param>
        /// <returns type="ASC.Core.Users.UserInfo, ASC.Core.Common">User</returns>
        /// <path>api/2.0/portal/users/{userID}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("users/{userID}")]
        public UserInfo GetUser(Guid userID)
        {
            return CoreContext.UserManager.GetUsers(userID);
        }


        /// <summary>
        /// Returns an invitation link for joining the portal.
        /// </summary>
        /// <short>
        /// Get an invitation link
        /// </short>
        /// <param type="ASC.Core.Users.EmployeeType, ASC.Core.Users" method="url" name="employeeType">
        ///  Employee type (User or Visitor)
        /// </param>
        /// <category>Users</category>
        /// <returns>
        /// Invitation link
        /// </returns>
        /// <path>api/2.0/portal/users/invite/{employeeType}</path>
        /// <httpMethod>GET</httpMethod>
        [Read("users/invite/{employeeType}")]
        public string GeInviteLink(EmployeeType employeeType)
        {
            if (!CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()
                && !WebItemSecurity.IsProductAdministrator(WebItemManager.PeopleProductID, SecurityContext.CurrentAccount.ID))
                throw new SecurityException("Method not available");

            return CommonLinkUtility.GetConfirmationUrl(string.Empty, ConfirmType.LinkInvite, (int)employeeType, SecurityContext.CurrentAccount.ID)
                   + String.Format("&emplType={0}", (int)employeeType);
        }

        /// <summary>
        /// Returns a link specified in the request in the shortened format.
        /// </summary>
        /// <short>Get a shortened link</short>
        /// <param type="System.String, System" name="link">Link</param>
        /// <category>Settings</category>
        /// <returns>Shortened link</returns>
        /// <path>api/2.0/portal/getshortenlink</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update("getshortenlink")]
        public String GetShortenLink(string link)
        {
            try
            {
                return UrlShortener.Instance.GetShortenLink(link);
            }
            catch (Exception ex)
            {
                LogWeb.Error("getshortenlink", ex);
                return link;
            }
        }


        /// <summary>
        /// Returns the used space of the current portal.
        /// </summary>
        /// <short>
        /// Get the used portal space
        /// </short>
        /// <category>Quota</category>
        /// <returns>Used space</returns>
        /// <path>api/2.0/portal/usedspace</path>
        /// <httpMethod>GET</httpMethod>
        [Read("usedspace")]
        public double GetUsedSpace()
        {
            return Math.Round(
                CoreContext.TenantManager.FindTenantQuotaRows(CoreContext.TenantManager.GetCurrentTenant().TenantId)
                           .Where(q => !string.IsNullOrEmpty(q.Tag) && new Guid(q.Tag) != Guid.Empty)
                           .Sum(q => q.Counter) / 1024f / 1024f / 1024f, 2);
        }

        /// <summary>
        /// Returns a number of portal users.
        /// </summary>
        /// <short>
        /// Get a number of portal users
        /// </short>
        /// <category>Users</category>
        /// <returns>User count</returns>
        /// <path>api/2.0/portal/userscount</path>
        /// <httpMethod>GET</httpMethod>
        [Read("userscount")]
        public long GetUsersCount()
        {
            return CoreContext.Configuration.Personal ? 1 : CoreContext.UserManager.GetUserNames(EmployeeStatus.Active).Count();
        }

        /// <summary>
        /// Uploads a portal license specified in the request.
        /// </summary>
        /// <short>
        /// Upload a license
        /// </short>
        /// <param type="System.Collections.Generic.IEnumerable{System.Web.HttpPostedFileBase}, System.Collections.Generic" name="attachments">License attachments</param>
        /// <category>Quota</category>
        /// <returns>License</returns>
        /// <path>api/2.0/portal/uploadlicense</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("uploadlicense")]
        public FileUploadResult UploadLicense(IEnumerable<HttpPostedFileBase> attachments)
        {
            if (!CoreContext.Configuration.Standalone) throw new NotSupportedException();

            var license = attachments.FirstOrDefault();

            if (license == null) throw new Exception(Resource.ErrorEmptyUploadFileSelected);

            var result = new FileUploadResult();

            try
            {
                var dueDate = LicenseReader.SaveLicenseTemp(license.InputStream);

                result.Message = dueDate >= DateTime.UtcNow.Date
                                         ? Resource.LicenseUploaded
                                         : string.Format(Resource.LicenseUploadedOverdue,
                                                         string.Empty,
                                                         string.Empty,
                                                         dueDate.Date.ToLongDateString());
                result.Success = true;
            }
            catch (LicenseExpiredException ex)
            {
                Log.Error("License upload", ex);
                result.Message = Resource.LicenseErrorExpired;
            }
            catch (LicenseQuotaException ex)
            {
                Log.Error("License upload", ex);
                result.Message = Resource.LicenseErrorQuota;
            }
            catch (LicensePortalException ex)
            {
                Log.Error("License upload", ex);
                result.Message = Resource.LicenseErrorPortal;
            }
            catch (Exception ex)
            {
                Log.Error("License upload", ex);
                result.Message = Resource.LicenseError;
            }

            return result;
        }

        /// <summary>
        /// Activates a license for the portal.
        /// </summary>
        /// <short>
        /// Activate a license
        /// </short>
        /// <category>Quota</category>
        /// <returns>License</returns>
        /// <path>api/2.0/portal/activatelicense</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("activatelicense")]
        public FileUploadResult ActivateLicense()
        {
            if (!CoreContext.Configuration.Standalone) throw new NotSupportedException();

            var result = new FileUploadResult();

            try
            {
                LicenseReader.RefreshLicense();
                Web.Studio.UserControls.Management.TariffSettings.LicenseAccept = true;
                MessageService.Send(HttpContext.Current.Request, MessageAction.LicenseKeyUploaded);
                result.Success = true;
            }
            catch (BillingNotFoundException ex)
            {
                Log.Error("License activate", ex);
                result.Message = UserControlsCommonResource.LicenseKeyNotFound;
            }
            catch (BillingNotConfiguredException ex)
            {
                Log.Error("License activate", ex);
                result.Message = UserControlsCommonResource.LicenseKeyNotCorrect;
            }
            catch (BillingException ex)
            {
                Log.Error("License activate", ex);
                result.Message = UserControlsCommonResource.LicenseException;
            }
            catch (Exception ex)
            {
                Log.Error("License activate", ex);
                result.Message = ex.Message;
            }

            return result;
        }


        /// <summary>
        /// Activates a trial license for the portal.
        /// </summary>
        /// <short>
        /// Activate a trial license
        /// </short>
        /// <category>Quota</category>
        /// <returns>Trial license</returns>
        /// <path>api/2.0/portal/activatetrial</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("activatetrial")]
        public bool ActivateTrial()
        {
            if (!CoreContext.Configuration.Standalone) throw new NotSupportedException();
            if (!CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin()) throw new SecurityException();

            var curQuota = TenantExtra.GetTenantQuota();
            if (curQuota.Id != Tenant.DEFAULT_TENANT) return false;
            if (curQuota.Trial) return false;

            var curTariff = TenantExtra.GetCurrentTariff();
            if (curTariff.DueDate.Date != DateTime.MaxValue.Date) return false;

            var quota = new TenantQuota(-1000)
            {
                Name = "apirequest",
                ActiveUsers = curQuota.ActiveUsers,
                MaxFileSize = curQuota.MaxFileSize,
                MaxTotalSize = curQuota.MaxTotalSize,
                Features = curQuota.Features
            };
            quota.Trial = true;

            CoreContext.TenantManager.SaveTenantQuota(quota);

            var DEFAULT_TRIAL_PERIOD = 30;

            var tariff = new Tariff
            {
                QuotaId = quota.Id,
                DueDate = DateTime.Today.AddDays(DEFAULT_TRIAL_PERIOD)
            };

            CoreContext.PaymentManager.SetTariff(-1, tariff);

            MessageService.Send(HttpContext.Current.Request, MessageAction.LicenseKeyUploaded);

            return true;
        }

        /// <summary>
        /// Returns an extra tenant license for the portal.
        /// </summary>
        /// <short>
        /// Get an extra tenant license
        /// </short>
        /// <category>Quota</category>
        /// <returns>Extra tenant license information</returns>
        /// <path>api/2.0/portal/tenantextra</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read("tenantextra")]
        public object GetTenantExtra()
        {
            return new
            {
                customMode = CoreContext.Configuration.CustomMode,
                opensource = TenantExtra.Opensource,
                enterprise = TenantExtra.Enterprise,
                tariff = TenantExtra.GetCurrentTariff(),
                quota = TenantExtra.GetTenantQuota(),
                notPaid = TenantStatisticsProvider.IsNotPaid(),
                licenseAccept = Web.Studio.UserControls.Management.TariffSettings.LicenseAccept,
                enableTariffPage = //TenantExtra.EnableTariffSettings - think about hide-settings for opensource
                    (!CoreContext.Configuration.Standalone || !string.IsNullOrEmpty(LicenseReader.LicensePath))
                    && string.IsNullOrEmpty(SetupInfo.AmiMetaUrl)
                    && !CoreContext.Configuration.CustomMode,
                DocServerUserQuota = DocumentServiceLicense.GetLicenseQuota(),
                DocServerLicense = DocumentServiceLicense.GetLicense()
            };
        }

        /// <summary>
        /// Returns the current portal tariff.
        /// </summary>
        /// <short>
        /// Get a portal tariff
        /// </short>
        /// <category>Quota</category>
        /// <returns type="ASC.Core.Billing.Tariff, ASC.Core.Common">Tariff</returns>
        /// <path>api/2.0/portal/tariff</path>
        /// <httpMethod>GET</httpMethod>
        [Read("tariff")]
        public Tariff GetTariff()
        {
            return CoreContext.PaymentManager.GetTariff(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        /// <summary>
        /// Returns the current portal quota.
        /// </summary>
        /// <short>
        /// Get a portal quota
        /// </short>
        /// <category>Quota</category>
        /// <returns type="ASC.Core.Tenants.TenantQuota, ASC.Core.Common">Quota</returns>
        /// <path>api/2.0/portal/quota</path>
        /// <httpMethod>GET</httpMethod>
        [Read("quota")]
        public TenantQuota GetQuota()
        {
            return CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        /// <summary>
        /// Returns the recommended quota for the current portal.
        /// </summary>
        /// <short>
        /// Get the recommended quota
        /// </short>
        /// <category>Quota</category>
        /// <returns type="ASC.Core.Tenants.TenantQuota, ASC.Core.Common">Quota</returns>
        /// <path>api/2.0/portal/quota/right</path>
        /// <httpMethod>GET</httpMethod>
        [Read("quota/right")]
        public TenantQuota GetRightQuota()
        {
            var usedSpace = GetUsedSpace();
            var needUsersCount = GetUsersCount();

            return CoreContext.TenantManager.GetTenantQuotas().OrderBy(r => r.Price)
                              .FirstOrDefault(quota =>
                                              quota.ActiveUsers > needUsersCount
                                              && quota.MaxTotalSize > usedSpace
                                              && !quota.Year);
        }

        /// <summary>
        /// Returns the full absolute path to the current portal.
        /// </summary>
        /// <short>
        /// Get a path to the portal
        /// </short>
        /// <param type="System.String, System" name="virtualPath">Portal virtual path</param>
        /// <returns>Portal path</returns>
        /// <path>api/2.0/portal/path</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read("path")]
        public string GetFullAbsolutePath(string virtualPath)
        {
            return CommonLinkUtility.GetFullAbsolutePath(virtualPath);
        }

        /// <summary>
        /// Returns a number of unread messages from the portal.
        /// </summary>
        /// <short>
        /// Get a number of unread messages
        /// </short>
        /// <category>Talk</category>
        /// <returns>Number of unread messages</returns>
        /// <path>api/2.0/portal/talk/unreadmessages</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read("talk/unreadmessages")]
        public int GetMessageCount()
        {
            try
            {
                return new JabberServiceClient().GetNewMessagesCount();
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Removes the XMPP connection specified in the request from the inner channel.
        /// </summary>
        /// <short>
        /// Remove the XMPP connection
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.String, System" name="connectionId">Connection ID</param>
        /// <returns>XMPP connection ID</returns>
        /// <path>api/2.0/portal/talk/connection</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <visible>false</visible>
        [Delete("talk/connection")]
        public int RemoveXmppConnection(string connectionId)
        {
            try
            {
                return new JabberServiceClient().RemoveXmppConnection(connectionId);
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Adds the XMPP connection to the inner channel.
        /// </summary>
        /// <short>
        /// Add the XMPP connection
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.String, System" name="connectionId">Connection ID</param>
        /// <param type="System.Byte, System" name="state">Service state</param>
        /// <returns>Updated inner channel</returns>
        /// <path>api/2.0/portal/talk/connection</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("talk/connection")]
        public byte AddXmppConnection(string connectionId, byte state)
        {
            try
            {
                return new JabberServiceClient().AddXmppConnection(connectionId, state);
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Returns the service state for the user with the name specified in the request.
        /// </summary>
        /// <short>
        /// Get the service state
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.String, System" name="userName">User name</param>
        /// <returns>State</returns>
        /// <path>api/2.0/portal/talk/state</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read("talk/state")]
        public int GetState(string userName)
        {
            try
            {
                return new JabberServiceClient().GetState(userName);
            }
            catch
            {
            }
            return 0;
        }

        /// <summary>
        /// Sends the service state specified in the request.
        /// </summary>
        /// <short>
        /// Send the service state
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.Byte, System" name="state">Service state</param>
        /// <returns>State</returns>
        /// <path>api/2.0/portal/talk/state</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("talk/state")]
        public byte SendState(byte state)
        {
            try
            {
                return new JabberServiceClient().SendState(state);
            }
            catch
            {
            }
            return 4;
        }

        /// <summary>
        /// Sends a message to the user specified in the request.
        /// </summary>
        /// <short>
        /// Send a message
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.String, System" name="to">User to whom a message will be sent</param>
        /// <param type="System.String, System" name="text">Message text</param>
        /// <param type="System.String, System" name="subject">Message subject</param>
        /// <path>api/2.0/portal/talk/message</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("talk/message")]
        public void SendMessage(string to, string text, string subject)
        {
            try
            {
                var username = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName;
                new JabberServiceClient().SendMessage(TenantProvider.CurrentTenantID, username, to, text, subject);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Returns a dictionary of all the service states.
        /// </summary>
        /// <short>
        /// Get service states
        /// </short>
        /// <category>Talk</category>
        /// <returns>Dictionary of all the service states</returns>
        /// <path>api/2.0/portal/talk/states</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read("talk/states")]
        public Dictionary<string, byte> GetAllStates()
        {
            try
            {
                return new JabberServiceClient().GetAllStates();
            }
            catch
            {
            }

            return new Dictionary<string, byte>();
        }

        /// <summary>
        /// Returns all the recent messages.
        /// </summary>
        /// <short>
        /// Get recent messages
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.String, System" name="calleeUserName">Callee user name</param>
        /// <param type="System.Int32, System" name="id">ID</param>
        /// <returns>Recent messages</returns>
        /// <path>api/2.0/portal/talk/recentMessages</path>
        /// <httpMethod>GET</httpMethod>
        /// <visible>false</visible>
        [Read("talk/recentMessages")]
        public MessageClass[] GetRecentMessages(string calleeUserName, int id)
        {
            try
            {
                var userName = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName;
                var recentMessages = new JabberServiceClient().GetRecentMessages(calleeUserName, id);

                if (recentMessages == null) return null;

                foreach (var mc in recentMessages)
                {
                    mc.DateTime = TenantUtil.DateTimeFromUtc(mc.DateTime.AddMilliseconds(1));
                    if (mc.UserName == null || string.Equals(mc.UserName, calleeUserName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        mc.UserName = calleeUserName;
                    }
                    else
                    {
                        mc.UserName = userName;
                    }
                }

                return recentMessages;
            }
            catch
            {
            }
            return new MessageClass[0];
        }

        /// <summary>
        /// Pings when a message is received.
        /// </summary>
        /// <short>
        /// Ping
        /// </short>
        /// <category>Talk</category>
        /// <param type="System.Byte, System" name="state">Service state</param>
        /// <path>api/2.0/portal/talk/ping</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("talk/ping")]
        public void Ping(byte state)
        {
            try
            {
                new JabberServiceClient().Ping(state);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Registers the mobile app installation.
        /// </summary>
        /// <short>
        /// Register the mobile app installation
        /// </short>
        /// <category>Mobile</category>
        /// <param type="ASC.Core.Common.Notify.Push.MobileAppType, ASC.Core.Common.Notify.Push" name="type">Mobile app type ("IosProjects", "AndroidProjects", "IosDocuments", "AndroidDocuments", or "DesktopEditor")</param>
        /// <path>api/2.0/portal/mobile/registration</path>
        /// <httpMethod>POST</httpMethod>
        /// <visible>false</visible>
        [Create("mobile/registration")]
        public void RegisterMobileAppInstall(MobileAppType type)
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            mobileAppRegistrator.RegisterInstall(currentUser.Email, type);
        }


        /// <summary>
        /// Returns the backup schedule for the current portal.
        /// </summary>
        /// <short>Get the backup schedule</short>
        /// <category>Backup</category>
        /// <returns type="ASC.Web.Studio.Core.Backup.BackupAjaxHandler.Schedule, ASC.Web.Studio">Backup schedule</returns>
        /// <path>api/2.0/portal/getbackupschedule</path>
        /// <httpMethod>GET</httpMethod>
        [Read("getbackupschedule")]
        public BackupAjaxHandler.Schedule GetBackupSchedule()
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.GetSchedule();
        }

        /// <summary>
        /// Creates the backup schedule for the current portal with the parameters specified in the request.
        /// </summary>
        /// <short>Create the backup schedule</short>
        /// <param type="ASC.Core.Common.Contracts.BackupStorageType, ASC.Core.Common.Contracts" name="storageType">Storage type ("Documents", "ThridpartyDocuments", "CustomCloud", "Local", "DataStore", or "ThirdPartyConsumer")</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.String, System.String}}, System.Collections.Generic" name="storageParams">Storage parameters</param>
        /// <param type="System.Int32, System" name="backupsStored">Maximum number of backup stored copies</param>
        /// <param type="ASC.Web.Studio.Core.Backup.BackupAjaxHandler.CronParams, ASC.Web.Studio.Core.Backup" name="cronParams">Cron parameters</param>
        /// <param type="System.Boolean, System" name="backupMail">Specifies if the mails will be included into the backup or not</param>
        /// <category>Backup</category>
        /// <path>api/2.0/portal/createbackupschedule</path>
        /// <httpMethod>POST</httpMethod>
        /// <returns></returns>
        [Create("createbackupschedule")]
        public void CreateBackupSchedule(BackupStorageType storageType, IEnumerable<ItemKeyValuePair<string, string>> storageParams, int backupsStored, BackupAjaxHandler.CronParams cronParams, bool backupMail)
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }
            else
            {
                if (!TenantExtra.GetTenantQuota().AutoBackup)
                {
                    throw new SecurityException(Resource.ErrorNotAllowedOption);
                }
            }

            backupHandler.CreateSchedule(storageType, storageParams.ToDictionary(r => r.Key, r => r.Value), backupsStored, cronParams, backupMail);
        }

        /// <summary>
        /// Deletes the backup schedule for the current portal.
        /// </summary>
        /// <short>Delete the backup schedule</short>
        /// <category>Backup</category>
        /// <path>api/2.0/portal/deletebackupschedule</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <returns></returns>
        [Delete("deletebackupschedule")]
        public void DeleteBackupSchedule()
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }
            else
            {
                if (!TenantExtra.GetTenantQuota().AutoBackup)
                {
                    throw new SecurityException(Resource.ErrorAccessDenied);
                }
            }

            backupHandler.DeleteSchedule();
        }

        /// <summary>
        /// Starts the backup of the current portal with the parameters specified in the request.
        /// </summary>
        /// <short>Start the backup</short>
        /// <param type="ASC.Core.Common.Contracts.BackupStorageType, ASC.Core.Common.Contracts" name="storageType">Storage type ("Documents", "ThridpartyDocuments", "CustomCloud", "Local", "DataStore", or "ThirdPartyConsumer")</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.String, System.String}}, System.Collections.Generic" name="storageParams">Storage parameters</param>
        /// <param type="System.Boolean, System" name="backupMail">Specifies if the mails will be included into the backup or not</param>
        /// <category>Backup</category>
        /// <returns type="ASC.Core.Common.Contracts.BackupProgress, ASC.Core.Common">Backup progress</returns>
        /// <path>api/2.0/portal/startbackup</path>
        /// <httpMethod>POST</httpMethod>
        [Create("startbackup")]
        public BackupProgress StartBackup(BackupStorageType storageType, IEnumerable<ItemKeyValuePair<string, string>> storageParams, bool backupMail)
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.StartBackup(storageType, storageParams.ToDictionary(r => r.Key, r => r.Value), backupMail);
        }

        /// <summary>
        /// Returns the progress of the started backup.
        /// </summary>
        /// <short>Get the backup progress</short>
        /// <category>Backup</category>
        /// <returns type="ASC.Core.Common.Contracts.BackupProgress, ASC.Core.Common">Backup progress</returns>
        /// <path>api/2.0/portal/getbackupprogress</path>
        /// <httpMethod>GET</httpMethod>
        [Read("getbackupprogress")]
        public BackupProgress GetBackupProgress()
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.GetBackupProgress();
        }

        /// <summary>
        /// Returns the history of the started backup.
        /// </summary>
        /// <short>Get the backup history</short>
        /// <category>Backup</category>
        /// <returns type="ASC.Core.Common.Contracts.BackupHistoryRecord, ASC.Core.Common">Backup history</returns>
        /// <path>api/2.0/portal/getbackuphistory</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("getbackuphistory")]
        public List<BackupHistoryRecord> GetBackupHistory()
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.GetBackupHistory();
        }

        /// <summary>
        /// Deletes the backup with the ID specified in the request.
        /// </summary>
        /// <short>Delete the backup</short>
        /// <param type="System.Guid, System" name="id">Backup ID</param>
        /// <category>Backup</category>
        /// <path>api/2.0/portal/deletebackup/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <returns></returns>
        [Delete("deletebackup/{id}")]
        public void DeleteBackup(Guid id)
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            backupHandler.DeleteBackup(id);
        }

        /// <summary>
        /// Deletes the backup history of the current portal.
        /// </summary>
        /// <short>Delete the backup history</short>
        /// <category>Backup</category>
        /// <path>api/2.0/portal/deletebackuphistory</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <returns></returns>
        [Delete("deletebackuphistory")]
        public void DeleteBackupHistory()
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            backupHandler.DeleteAllBackups();
        }

        /// <summary>
        /// Starts the data restoring process for the current portal with the parameters specified in the request.
        /// </summary>
        /// <short>Start the restoring process</short>
        /// <param type="System.String, System" name="backupId">Backup ID</param>
        /// <param type="ASC.Core.Common.Contracts.BackupStorageType, ASC.Core.Common.Contracts" name="storageType">Storage type ("Documents", "ThridpartyDocuments", "CustomCloud", "Local", "DataStore", or "ThirdPartyConsumer")</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.Api.Collections.ItemKeyValuePair{System.String, System.String}}, System.Collections.Generic" name="storageParams">Storage parameters</param>
        /// <param type="System.Boolean, System" name="notify">Specifies whether to notify the users about the backup or not</param>
        /// <category>Backup</category>
        /// <returns type="ASC.Core.Common.Contracts.BackupProgress, ASC.Core.Common">Restoring progress</returns>
        /// <path>api/2.0/portal/startrestore</path>
        /// <httpMethod>POST</httpMethod>
        [Create("startrestore")]
        public BackupProgress StartBackupRestore(string backupId, BackupStorageType storageType, IEnumerable<ItemKeyValuePair<string, string>> storageParams, bool notify)
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.StartRestore(backupId, storageType, storageParams.ToDictionary(r => r.Key, r => r.Value), notify);
        }

        /// <summary>
        /// Returns the progress of the started restoring process.
        /// </summary>
        /// <short>Get the restoring progress</short>
        /// <category>Backup</category>
        /// <returns type="ASC.Core.Common.Contracts.BackupProgress, ASC.Core.Common">Restoring progress</returns>
        /// <path>api/2.0/portal/getrestoreprogress</path>
        /// <httpMethod>GET</httpMethod>
        [Read("getrestoreprogress", true, false)]  //NOTE: this method doesn't check payment!!!
        public BackupProgress GetRestoreProgress()
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.GetRestoreProgress();
        }

        /// <summary>
        /// Returns the path to the backup temporary directory.
        /// </summary>
        /// <short>Get the path to the backup temporary directory</short>
        /// <category>Backup</category>
        /// <param type="System.String, System" name="alias">Alias</param>
        /// <returns>The path to the backup temporary directory</returns>
        /// <path>api/2.0/portal/backuptmp</path>
        /// <httpMethod>GET</httpMethod>
        ///<visible>false</visible>
        [Read("backuptmp")]
        public string GetTempPath(string alias)
        {
            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            return backupHandler.GetTmpFolder();
        }

        /// <summary>
        /// Deletes the current portal immediately.
        /// </summary>
        /// <short>Delete the current portal</short>
        /// <path>api/2.0/portal/deleteportalimmediately</path>
        /// <httpMethod>DELETE</httpMethod>
        ///<visible>false</visible>
        [Delete("deleteportalimmediately")]
        public void DeletePortalImmediately()
        {
            var tenant = CoreContext.TenantManager.GetCurrentTenant();

            if (SecurityContext.CurrentAccount.ID != tenant.OwnerId)
            {
                throw new Exception(Resource.ErrorAccessDenied);
            }

            CoreContext.TenantManager.RemoveTenant(tenant.TenantId);

            if (!string.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
            {
                ApiSystemHelper.RemoveTenantFromCache(tenant.TenantAlias);
            }

            try
            {
                if (!SecurityContext.IsAuthenticated)
                {
                    SecurityContext.CurrentAccount = ASC.Core.Configuration.Constants.CoreSystem;
                }
                MessageService.Send(HttpContext.Current.Request, MessageAction.PortalDeleted);
            }
            finally
            {
                SecurityContext.Logout();
            }
        }

        /// <summary>
        /// Updates a portal name with a new one specified in the request.
        /// </summary>
        /// <short>Update a portal name</short>
        /// <param type="System.String, System" name="alias">New portal name</param>
        /// <returns>Message about renaming a portal</returns>
        /// <path>api/2.0/portal/portalrename</path>
        /// <httpMethod>PUT</httpMethod>
        ///<visible>false</visible>
        [Update("portalrename")]
        public object UpdatePortalName(string alias)
        {
            var enabled = SetupInfo.IsVisibleSettings("PortalRename");
            if (!enabled)
                throw new SecurityException(Resource.PortalAccessSettingsTariffException);

            if (CoreContext.Configuration.Personal)
                throw new Exception(Resource.ErrorAccessDenied);

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (String.IsNullOrEmpty(alias)) throw new ArgumentException();

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var localhost = CoreContext.Configuration.BaseDomain == "localhost" || tenant.TenantAlias == "localhost";

            var newAlias = alias.ToLowerInvariant();
            var oldAlias = tenant.TenantAlias;
            var oldVirtualRootPath = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');

            if (!String.Equals(newAlias, oldAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!String.IsNullOrEmpty(ApiSystemHelper.ApiSystemUrl))
                {
                    ApiSystemHelper.ValidatePortalName(newAlias);
                }
                else
                {
                    CoreContext.TenantManager.CheckTenantAddress(newAlias.Trim());
                }


                if (!String.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
                {
                    ApiSystemHelper.AddTenantToCache(newAlias);
                }

                tenant.TenantAlias = alias;
                tenant = CoreContext.TenantManager.SaveTenant(tenant);


                if (!String.IsNullOrEmpty(ApiSystemHelper.ApiCacheUrl))
                {
                    ApiSystemHelper.RemoveTenantFromCache(oldAlias);
                }

                if (!localhost || string.IsNullOrEmpty(tenant.MappedDomain))
                {
                    StudioNotifyService.Instance.PortalRenameNotify(oldVirtualRootPath);
                }
            }
            else
            {
                throw new Exception(ResourceJS.ErrorPortalNameWasNotChanged);
            }

            var reference = CreateReference(Request, tenant.TenantDomain, tenant.TenantId, user.Email);

            return new
            {
                message = Resource.SuccessfullyPortalRenameMessage,
                reference = reference
            };
        }

        #region create reference for auth on renamed tenant

        private static string CreateReference(HttpRequest request, string tenantDomain, int tenantId, string email)
        {
            return String.Format("{0}{1}{2}/{3}",
                                 request != null && request.UrlReferrer != null ? request.UrlReferrer.Scheme : Uri.UriSchemeHttp,
                                 Uri.SchemeDelimiter,
                                 tenantDomain,
                                 CommonLinkUtility.GetConfirmationUrlRelative(tenantId, email, ConfirmType.Auth)
                );
        }

        #endregion

        /// <summary>
        /// Sends congratulations to the user after registering the portal.
        /// </summary>
        /// <short>Send congratulations</short>
        /// <param type="System.Guid, System" name="userid">User ID</param>
        /// <param type="System.String, System" name="key">Email key</param>
        /// <path>api/2.0/portal/sendcongratulations</path>
        /// <httpMethod>POST</httpMethod>
        ///<visible>false</visible>
        /// <requiresAuthorization>false</requiresAuthorization>
        [Create("sendcongratulations", false)] //NOTE: this method doesn't require auth!!!
        public void SendCongratulations(Guid userid, string key)
        {
            var authInterval = TimeSpan.FromHours(1);
            var checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(userid.ToString() + ConfirmType.Auth, key, authInterval);

            switch (checkKeyResult)
            {
                case EmailValidationKeyProvider.ValidationResult.Ok:
                    var currentUser = CoreContext.UserManager.GetUsers(userid);

                    StudioNotifyService.Instance.SendCongratulations(currentUser);
                    StudioNotifyService.Instance.SendRegData(currentUser);

                    if (!SetupInfo.IsSecretEmail(currentUser.Email))
                    {
                        if (SetupInfo.TfaRegistration == "sms")
                        {
                            StudioSmsNotificationSettings.Enable = true;
                        }
                        else if (SetupInfo.TfaRegistration == "code")
                        {
                            TfaAppAuthSettings.Enable = true;
                        }
                    }
                    break;
                default:
                    throw new SecurityException("Access Denied.");
            }
        }


        /// <summary>
        /// Removes a comment with the ID specified in the request.
        /// </summary>
        /// <short>Remove a comment</short>
        /// <category>Comments</category>
        /// <param type="System.String, System" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="domain">Domain name</param>
        /// <returns>Operation status</returns>
        /// <path>api/2.0/portal/fcke/comment/removecomplete</path>
        /// <httpMethod>PUT</httpMethod>
        ///<visible>false</visible>
        [Update("fcke/comment/removecomplete")]
        public object RemoveCommentComplete(string commentid, string domain)
        {
            try
            {
                CommonControlsConfigurer.FCKUploadsRemoveForItem(domain, commentid);
                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Cancels editing a comment with the ID specified in the request.
        /// </summary>
        /// <short>Cancel comment editing</short>
        /// <category>Comments</category>
        /// <param type="System.String, System" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="domain">Domain name</param>
        /// <param type="System.Boolean System" name="isedit">Specifies if a comment was edited or not</param>
        /// <returns>Operation status</returns>
        /// <path>api/2.0/portal/fcke/comment/cancelcomplete</path>
        /// <httpMethod>PUT</httpMethod>
        ///<visible>false</visible>
        [Update("fcke/comment/cancelcomplete")]
        public object CancelCommentComplete(string commentid, string domain, bool isedit)
        {
            try
            {
                if (isedit)
                    CommonControlsConfigurer.FCKEditingCancel(domain, commentid);
                else
                    CommonControlsConfigurer.FCKEditingCancel(domain);

                return 1;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Edits a comment with the ID specified in the request.
        /// </summary>
        /// <short>Edit a comment</short>
        /// <category>Comments</category>
        /// <param type="System.String, System" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="domain">Domain name</param>
        /// <param type="System.String, System" name="html">New comment in the HTML format</param>
        /// <param type="System.Boolean, System" name="isedit">Specifies if a comment was edited or not</param>
        /// <returns>Operation status</returns>
        /// <path>api/2.0/portal/fcke/comment/editcomplete</path>
        /// <httpMethod>PUT</httpMethod>
        ///<visible>false</visible>
        [Update("fcke/comment/editcomplete")]
        public object EditCommentComplete(string commentid, string domain, string html, bool isedit)
        {
            try
            {
                CommonControlsConfigurer.FCKEditingComplete(domain, commentid, html, isedit);
                return 1;
            }

            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the promotion bar.
        /// </summary>
        /// <short>Get the promotion bar</short>
        /// <category>Promotions</category>
        /// <param type="System.String, System" name="domain">Domain name</param>
        /// <param type="System.String, System" name="page">Page</param>
        /// <param type="System.Boolean, System" name="desktop">Specifies if the bar will be displayed in the desktop app or not</param>
        /// <returns>Promotion bar</returns>
        /// <path>api/2.0/portal/bar/promotions</path>
        /// <httpMethod>GET</httpMethod>
        ///<visible>false</visible>
        [Read("bar/promotions")]
        public async Task<string> GetBarPromotions(string domain, string page, bool desktop)
        {
            try
            {
                var showPromotions = PromotionsSettings.Load().Show;

                if (!showPromotions)
                    return null;

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                var uriBuilder = new UriBuilder(SetupInfo.NotifyAddress + "promotions/Get");

                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                if (string.IsNullOrEmpty(domain))
                {
                    domain = Request.UrlReferrer != null ? Request.UrlReferrer.Host : string.Empty;
                }

                if (string.IsNullOrEmpty(page))
                {
                    page = Request.UrlReferrer != null ? Request.UrlReferrer.PathAndQuery : string.Empty;
                }

                query["userId"] = user.ID.ToString();
                query["language"] = Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant();
                query["version"] = tenant.Version.ToString(CultureInfo.InvariantCulture);
                query["tariff"] = TenantExtra.GetTenantQuota().Id.ToString(CultureInfo.InvariantCulture);
                query["admin"] = user.IsAdmin().ToString();
                query["userCreated"] = user.CreateDate.ToString(CultureInfo.InvariantCulture);
                query["promo"] = true.ToString();
                query["domain"] = domain;
                query["page"] = page;
                query["agent"] = Request.UserAgent ?? Request.Headers["User-Agent"];
                query["desktop"] = desktop.ToString();

                uriBuilder.Query = query.ToString();

                Func<Task<string>> requestFunc = async () =>
                {
                    return await httpClient.GetStringAsync(uriBuilder.Uri);
                };

                return await ResiliencePolicyManager.GetStringWithPoliciesAsync("promotions", requestFunc);

            }
            catch (Exception ex)
            {
                LogWeb.Error("GetBarTips", ex);
                return null;
            }
        }

        /// <summary>
        /// Marks the promotion bar as read.
        /// </summary>
        /// <short>Mark the promotion bar as read</short>
        /// <category>Promotions</category>
        /// <param type="System.String, System" name="id">ID</param>
        /// <path>api/2.0/portal/bar/promotions/mark/{id}</path>
        /// <httpMethod>POST</httpMethod>
        ///<visible>false</visible>
        [Create("bar/promotions/mark/{id}")]
        public void MarkBarPromotion(string id)
        {
            try
            {
                var url = string.Format("{0}promotions/Complete", SetupInfo.NotifyAddress);

                using (var client = new WebClient())
                {
                    client.UploadValues(url, "POST", new NameValueCollection
                        {
                            {"id", id},
                            {"userId", SecurityContext.CurrentAccount.ID.ToString()}
                        });
                }
            }
            catch (Exception ex)
            {
                LogWeb.Error("MarkBarPromotion", ex);
            }
        }

        /// <summary>
        /// Returns the promotion bar tips.
        /// </summary>
        /// <short>Get the promotion bar tips</short>
        /// <category>Promotions</category>
        /// <param type="System.String, System" name="domain">Domain name</param>
        /// <param type="System.String, System" name="page">Page</param>
        /// <param type="System.Boolean, System" name="productAdmin">Specifies if the current user is a product administrator or not</param>
        /// <param type="System.Boolean, System" name="desktop">Specifies if the bar will be displayed in the desktop app or not</param>
        /// <returns>Promotion bar tips</returns>
        /// <path>api/2.0/portal/bar/tips</path>
        /// <httpMethod>GET</httpMethod>
        ///<visible>false</visible>
        [Read("bar/tips")]
        public async Task<string> GetBarTips(string domain, string page, bool productAdmin, bool desktop)
        {
            try
            {
                if (string.IsNullOrEmpty(page))
                    return null;

                if (!TipsSettings.LoadForCurrentUser().Show)
                    return null;

                var tenant = CoreContext.TenantManager.GetCurrentTenant();
                var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

                var uriBuilder = new UriBuilder(SetupInfo.TipsAddress + "tips/Get");

                var query = HttpUtility.ParseQueryString(uriBuilder.Query);

                query["userId"] = user.ID.ToString();
                query["tenantId"] = tenant.TenantId.ToString(CultureInfo.InvariantCulture);
                query["domain"] = domain;
                query["page"] = page;
                query["language"] = Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant();
                query["admin"] = user.IsAdmin().ToString();
                query["productAdmin"] = productAdmin.ToString();
                query["visitor"] = user.IsVisitor().ToString();
                query["userCreatedDate"] = user.CreateDate.ToString(CultureInfo.InvariantCulture);
                query["tenantCreatedDate"] = tenant.CreatedDateTime.ToString(CultureInfo.InvariantCulture);
                query["desktop"] = desktop.ToString();

                uriBuilder.Query = query.ToString();

                Func<Task<string>> requestFunc = async () =>
                {
                    return await httpClient.GetStringAsync(uriBuilder.Uri);
                };

                return await ResiliencePolicyManager.GetStringWithPoliciesAsync("tips", requestFunc);
            }
            catch (Exception ex)
            {
                LogWeb.Error("GetBarTips", ex);
                return null;
            }
        }

        /// <summary>
        /// Marks the promotion bar tips as read.
        /// </summary>
        /// <short>Mark the promotion bar tips as read</short>
        /// <category>Promotions</category>
        /// <param type="System.String, System" name="id">ID</param>
        /// <path>api/2.0/portal/bar/tips/mark/{id}</path>
        /// <httpMethod>POST</httpMethod>
        ///<visible>false</visible>
        [Create("bar/tips/mark/{id}")]
        public void MarkBarTip(string id)
        {
            try
            {
                var url = string.Format("{0}tips/MarkRead", SetupInfo.TipsAddress);

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.UploadValues(url, "POST", new NameValueCollection
                        {
                            {"id", id},
                            {"userId", SecurityContext.CurrentAccount.ID.ToString()},
                            {"tenantId", CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(CultureInfo.InvariantCulture)}
                        });
                }
            }
            catch (Exception ex)
            {
                LogWeb.Error("MarkBarTip", ex);
            }
        }

        /// <summary>
        /// Deletes the promotion bar tips.
        /// </summary>
        /// <short>Delete the promotion bar tips</short>
        /// <category>Promotions</category>
        /// <path>api/2.0/portal/bar/tips</path>
        /// <httpMethod>DELETE</httpMethod>
        ///<visible>false</visible>
        [Delete("bar/tips")]
        public void DeleteBarTips()
        {
            try
            {
                var url = string.Format("{0}tips/DeleteReaded", SetupInfo.TipsAddress);

                using (var client = new WebClient())
                {
                    client.UploadValues(url, "POST", new NameValueCollection
                        {
                            {"userId", SecurityContext.CurrentAccount.ID.ToString()},
                            {"tenantId", CoreContext.TenantManager.GetCurrentTenant().TenantId.ToString(CultureInfo.InvariantCulture)}
                        });
                }

            }
            catch (Exception ex)
            {
                LogWeb.Error("DeleteBarTips", ex);
            }
        }

        /// <summary>
        /// Returns the search settings.
        /// </summary>
        /// <short>Get the search settings</short>
        /// <category>Search</category>
        /// <returns>Search settings</returns>
        ///<path>api/2.0/portal/search</path>
        ///<httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read("search")]
        public IEnumerable<object> GetSearchSettings()
        {
            TenantExtra.DemandControlPanelPermission();

            return SearchSettings.GetAllItems().Select(r => new
            {
                id = r.ID,
                title = r.Title,
                enabled = r.Enabled
            });
        }

        /// <summary>
        /// Checks if the search process is available or not.
        /// </summary>
        /// <short>Check the search availability</short>
        /// <category>Search</category>
        /// <returns>Search information</returns>
        ///<path>api/2.0/portal/search/state</path>
        ///<httpMethod>GET</httpMethod>
        [Read("search/state")]
        public object CheckSearchAvailable()
        {
            TenantExtra.DemandControlPanelPermission();

            return FactoryIndexer.GetState();
        }

        /// <summary>
        /// Reindexes a page during the search process.
        /// </summary>
        /// <short>Reindex a page</short>
        /// <category>Search</category>
        /// <param type="System.String, System" name="name">Index name</param>
        /// <returns>Search information</returns>
        /// <path>api/2.0/portal/search/reindex</path>
        /// <httpMethod>POST</httpMethod>
        [Create("search/reindex")]
        public object Reindex(string name)
        {
            TenantExtra.DemandControlPanelPermission();

            FactoryIndexer.Reindex(name);
            return CheckSearchAvailable();
        }

        /// <summary>
        /// Sets the search settings specified in the request.
        /// </summary>
        /// <short>Set the search settings</short>
        /// <category>Search</category>
        /// <param type="System.Collections.Generic.List{ASC.ElasticSearch.Core.SearchSettingsItem}, System.Collections.Generic" name="items">Search settings</param>
        /// <path>api/2.0/portal/search</path>
        /// <httpMethod>POST</httpMethod>
        /// <returns></returns>
        [Create("search")]
        public void SetSearchSettings(List<SearchSettingsItem> items)
        {
            TenantExtra.DemandControlPanelPermission();

            SearchSettings.Set(items);
        }

        /// <summary>
        /// Returns a random password.
        /// </summary>
        /// <short>Get a random password</short>
        /// <returns>Random password</returns>
        /// <path>api/2.0/portal/randompwd</path>
        /// <httpMethod>GET</httpMethod>
        ///<visible>false</visible>
        [Read(@"randompwd")]
        public string GetRandomPassword()
        {
            var Noise = "1234567890mnbasdflkjqwerpoiqweyuvcxnzhdkqpsdk_#()$";

            var ps = PasswordSettings.Load();

            var maxLength = ps.MaxLength
                            - (ps.Digits ? 1 : 0)
                            - (ps.UpperCase ? 1 : 0)
                            - (ps.SpecSymbols ? 1 : 0);
            var minLength = Math.Min(ps.MinLength, maxLength);

            var password = String.Format("{0}{1}{2}{3}",
                             GeneratePassword(minLength, minLength, Noise.Substring(0, Noise.Length - 4)),
                             ps.Digits ? GeneratePassword(1, 1, Noise.Substring(0, 10)) : String.Empty,
                             ps.UpperCase ? GeneratePassword(1, 1, Noise.Substring(10, 20).ToUpper()) : String.Empty,
                             ps.SpecSymbols ? GeneratePassword(1, 1, Noise.Substring(Noise.Length - 4, 4).ToUpper()) : String.Empty);

            return password;
        }

        private static readonly Random Rnd = new Random();

        private static string GeneratePassword(int minLength, int maxLength, string noise)
        {
            var length = Rnd.Next(minLength, maxLength + 1);

            var pwd = string.Empty;
            while (length-- > 0)
            {
                pwd += noise.Substring(Rnd.Next(noise.Length - 1), 1);
            }
            return pwd;
        }

        /// <summary>
        /// Returns the information about the IP address specified in the request.
        /// </summary>
        /// <short>Get the IP information</short>
        /// <param type="System.String, System" name="ipAddress">IP address</param>
        /// <returns>IP information</returns>
        /// <path>api/2.0/portal/ip/{ipAddress}</path>
        /// <httpMethod>GET</httpMethod>
        ///<visible>false</visible>
        [Read("ip/{ipAddress}")]
        public object GetIPInformation(string ipAddress)
        {
            GeolocationHelper helper = new GeolocationHelper("teamlabsite");
            return helper.GetIPGeolocation(ipAddress);

        }

        /// <summary>
        /// Marks a gift message as read.
        /// </summary>
        /// <short>Mark a gift message as read</short>
        /// <path>api/2.0/portal/gift/mark</path>
        /// <httpMethod>POST</httpMethod>
        ///<visible>false</visible>
        [Create("gift/mark")]
        public void MarkGiftAsReaded()
        {
            try
            {
                var settings = OpensourceGiftSettings.LoadForCurrentUser();
                settings.Readed = true;
                settings.SaveForCurrentUser();
            }
            catch (Exception ex)
            {
                LogWeb.Error("MarkGiftAsReaded", ex);
            }
        }
    }
}