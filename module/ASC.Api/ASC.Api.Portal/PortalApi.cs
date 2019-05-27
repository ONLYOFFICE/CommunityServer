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


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Common.Logging;
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
using ASC.Security.Cryptography;
using ASC.Web.Core.Helpers;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.FirstTime;
using ASC.Web.Studio.Utility;
using Resources;
using SecurityContext = ASC.Core.SecurityContext;
using UrlShortener = ASC.Web.Core.Utility.UrlShortener;

namespace ASC.Api.Portal
{
    ///<summary>
    /// Portal info access
    ///</summary>
    public class PortalApi : IApiEntryPoint
    {
        private readonly IMobileAppInstallRegistrator mobileAppRegistrator;
        private readonly BackupAjaxHandler backupHandler = new BackupAjaxHandler();


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

        ///<summary>
        ///Returns the current portal
        ///</summary>
        ///<short>
        ///Current portal
        ///</short>
        ///<returns>Portal</returns>
        [Read("")]
        public Tenant Get()
        {
            return CoreContext.TenantManager.GetCurrentTenant();
        }

        ///<summary>
        ///Returns the user with specified userID from the current portal
        ///</summary>
        ///<short>
        ///User with specified userID
        ///</short>
        /// <category>Users</category>
        ///<returns>User</returns>
        [Read("users/{userID}")]
        public UserInfo GetUser(Guid userID)
        {
            return CoreContext.UserManager.GetUsers(userID);
        }


        ///<summary>
        /// Returns invitational link to the portal
        ///</summary>
        ///<short>
        /// Returns invitational link to the portal
        ///</short>
        /// <param name="employeeType">
        ///  User or Visitor
        /// </param>
        ///<category>Users</category>
        ///<returns>
        /// Invite link
        ///</returns>
        [Read("users/invite/{employeeType}")]
        public string GeInviteLink(EmployeeType employeeType)
        {
            return CommonLinkUtility.GetConfirmationUrl(string.Empty, ConfirmType.LinkInvite, (int)employeeType, SecurityContext.CurrentAccount.ID)
                   + String.Format("&emplType={0}", (int)employeeType);
        }

        /// <summary>
        /// Returns shorten link
        /// </summary>
        /// <param name="link">Link for shortening</param>
        ///<returns>link</returns>
        ///<visible>false</visible>
        [Update("getshortenlink")]
        public String GetShortenLink(string link)
        {
            try
            {
                return UrlShortener.Instance.GetShortenLink(link);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web").Error("getshortenlink", ex);
                return link;
            }
        }


        ///<summary>
        ///Returns the used space of the current portal
        ///</summary>
        ///<short>
        ///Used space of the current portal
        ///</short>
        /// <category>Quota</category>
        ///<returns>Used space</returns>
        [Read("usedspace")]
        public double GetUsedSpace()
        {
            return Math.Round(
                CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(CoreContext.TenantManager.GetCurrentTenant().TenantId))
                           .Where(q => !string.IsNullOrEmpty(q.Tag) && new Guid(q.Tag) != Guid.Empty)
                           .Sum(q => q.Counter) / 1024f / 1024f / 1024f, 2);
        }

        ///<summary>
        ///Returns the users count of the current portal
        ///</summary>
        ///<short>
        ///Users count of the current portal
        ///</short>
        /// <category>Users</category>
        ///<returns>Users count</returns>
        [Read("userscount")]
        public long GetUsersCount()
        {
            return CoreContext.UserManager.GetUserNames(EmployeeStatus.Active).Count();
        }

        ///<summary>
        ///Returns the current tariff of the current portal
        ///</summary>
        ///<short>
        ///Tariff of the current portal
        ///</short>
        /// <category>Quota</category>
        ///<returns>Tariff</returns>
        [Read("tariff")]
        public Tariff GetTariff()
        {
            return CoreContext.PaymentManager.GetTariff(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        ///<summary>
        ///Returns the current quota of the current portal
        ///</summary>
        ///<short>
        ///Quota of the current portal
        ///</short>
        /// <category>Quota</category>
        ///<returns>Quota</returns>
        [Read("quota")]
        public TenantQuota GetQuota()
        {
            return CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
        }

        ///<summary>
        ///Returns the recommended quota of the current portal
        ///</summary>
        ///<short>
        ///Quota of the current portal
        ///</short>
        /// <category>Quota</category>
        ///<returns>Quota</returns>
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

        ///<summary>
        ///Returns path
        ///</summary>
        ///<short>
        ///path
        ///</short>
        ///<returns>path</returns>
        ///<visible>false</visible>
        [Read("path")]
        public string GetFullAbsolutePath(string virtualPath)
        {
            return CommonLinkUtility.GetFullAbsolutePath(virtualPath);
        }

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
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

        ///<visible>false</visible>
        [Create("mobile/registration")]
        public void RegisterMobileAppInstall(MobileAppType type)
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
            mobileAppRegistrator.RegisterInstall(currentUser.Email, type);
        }


        /// <summary>
        /// Returns the backup schedule of the current portal
        /// </summary>
        /// <category>Backup</category>
        /// <returns>Backup Schedule</returns>
        [Read("getbackupschedule")]
        public BackupAjaxHandler.Schedule GetBackupSchedule()
        {
            return backupHandler.GetSchedule();
        }

        /// <summary>
        /// Create the backup schedule of the current portal
        /// </summary>
        /// <param name="storageType">Storage type</param>
        /// <param name="storageParams">Storage parameters</param>
        /// <param name="backupsStored">Max of the backup's stored copies</param>
        /// <param name="cronParams">Cron parameters</param>
        /// <param name="backupMail">Include mail in the backup</param>
        /// <category>Backup</category>
        [Create("createbackupschedule")]
        public void CreateBackupSchedule(BackupStorageType storageType, IEnumerable<ItemKeyValuePair<string, string>> storageParams, int backupsStored, BackupAjaxHandler.CronParams cronParams, bool backupMail)
        {
            backupHandler.CreateSchedule(storageType, storageParams.ToDictionary(r=> r.Key, r=> r.Value), backupsStored, cronParams, backupMail);
        }

        /// <summary>
        /// Delete the backup schedule of the current portal
        /// </summary>
        /// <category>Backup</category>
        [Delete("deletebackupschedule")]
        public void DeleteBackupSchedule()
        {
            backupHandler.DeleteSchedule();
        }

        /// <summary>
        /// Start a backup of the current portal
        /// </summary>
        /// <param name="storageType">Storage Type</param>
        /// <param name="storageParams">Storage Params</param>
        /// <param name="backupMail">Include mail in the backup</param>
        /// <category>Backup</category>
        /// <returns>Backup Progress</returns>
        [Create("startbackup")]
        public BackupProgress StartBackup(BackupStorageType storageType, IEnumerable<ItemKeyValuePair<string, string>> storageParams, bool backupMail)
        {
            return backupHandler.StartBackup(storageType, storageParams.ToDictionary(r=> r.Key, r=> r.Value), backupMail);
        }

        /// <summary>
        /// Returns the progress of the started backup
        /// </summary>
        /// <category>Backup</category>
        /// <returns>Backup Progress</returns>
        [Read("getbackupprogress")]
        public BackupProgress GetBackupProgress()
        {
            return backupHandler.GetBackupProgress();
        }

        /// <summary>
        /// Returns the backup history of the started backup
        /// </summary>
        /// <category>Backup</category>
        /// <returns>Backup History</returns>
        [Read("getbackuphistory")]
        public List<BackupHistoryRecord> GetBackupHistory()
        {
            return backupHandler.GetBackupHistory();
        }

        /// <summary>
        /// Delete the backup with the specified id
        /// </summary>
        /// <category>Backup</category>
        [Delete("deletebackup/{id}")]
        public void DeleteBackup(Guid id)
        {
            backupHandler.DeleteBackup(id);
        }

        /// <summary>
        /// Delete all backups of the current portal
        /// </summary>
        /// <category>Backup</category>
        /// <returns>Backup History</returns>
        [Delete("deletebackuphistory")]
        public void DeleteBackupHistory()
        {
            backupHandler.DeleteAllBackups();
        }

        /// <summary>
        /// Start a data restore of the current portal
        /// </summary>
        /// <param name="backupId">Backup Id</param>
        /// <param name="storageType">Storage Type</param>
        /// <param name="storageParams">Storage Params</param>
        /// <param name="notify">Notify about backup to users</param>
        /// <category>Backup</category>
        /// <returns>Restore Progress</returns>
        [Create("startrestore")]
        public BackupProgress StartBackupRestore(string backupId, BackupStorageType storageType, IEnumerable<ItemKeyValuePair<string, string>> storageParams, bool notify)
        {
            return backupHandler.StartRestore(backupId, storageType, storageParams.ToDictionary(r => r.Key, r => r.Value), notify);
        }

        /// <summary>
        /// Returns the progress of the started restore
        /// </summary>
        /// <category>Backup</category>
        /// <returns>Restore Progress</returns>
        [Read("getrestoreprogress", true, false)]  //NOTE: this method doesn't check payment!!!
        public BackupProgress GetRestoreProgress()
        {
            return backupHandler.GetRestoreProgress();
        }

        ///<visible>false</visible>
        [Read("backuptmp")]
        public string GetTempPath(string alias)
        {
            return backupHandler.GetTmpFolder();
        }


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

            return new {
                message = Resource.SuccessfullyPortalRenameMessage,
                reference = reference
            };
        }

        ///<visible>false</visible>
        [Update("portalanalytics")]
        public bool UpdatePortalAnalytics(bool enable)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!(TenantExtra.Opensource || (TenantExtra.Saas && SetupInfo.CustomScripts.Length != 0)) || CoreContext.Configuration.CustomMode)
                throw new SecurityException();

            if (TenantExtra.Opensource)
            {
                var wizardSettings = WizardSettings.Load();
                wizardSettings.Analytics = enable;
                wizardSettings.Save();
            }
            else if (TenantExtra.Saas)
            {
                var analyticsSettings = TenantAnalyticsSettings.Load();
                analyticsSettings.Analytics = enable;
                analyticsSettings.Save();
            }

            return enable;
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

        ///<visible>false</visible>
        [Create("sendcongratulations", false)] //NOTE: this method doesn't requires auth!!!
        public void SendCongratulations(Guid userid, string key)
        {
            var authInterval = TimeSpan.FromHours(1);
            var checkKeyResult = EmailValidationKeyProvider.ValidateEmailKey(userid.ToString() + ConfirmType.Auth, key, authInterval);

            switch (checkKeyResult)
            {
                case EmailValidationKeyProvider.ValidationResult.Ok:
                    var currentUser = CoreContext.UserManager.GetUsers(userid);
                    StudioNotifyService.Instance.SendCongratulations(currentUser);
                    FirstTimeTenantSettings.SendInstallInfo(currentUser);

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

        ///<visible>false</visible>
        [Read("bar/promotions")]
        public string GetBarPromotions(string domain, string page, bool desktop)
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

                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    return client.DownloadString(uriBuilder.Uri);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web").Error("GetBarTips", ex);
                return null;
            }
        }

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
                LogManager.GetLogger("ASC.Web").Error("MarkBarPromotion", ex);
            }
        }

        ///<visible>false</visible>
        [Read("bar/tips")]
        public string GetBarTips(string page, bool productAdmin, bool desktop)
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
                query["page"] = page;
                query["language"] = Thread.CurrentThread.CurrentCulture.Name.ToLowerInvariant();
                query["admin"] = user.IsAdmin().ToString();
                query["productAdmin"] = productAdmin.ToString();
                query["visitor"] = user.IsVisitor().ToString();
                query["userCreatedDate"] = user.CreateDate.ToString(CultureInfo.InvariantCulture);
                query["tenantCreatedDate"] = tenant.CreatedDateTime.ToString(CultureInfo.InvariantCulture);
                query["desktop"] = desktop.ToString();

                uriBuilder.Query = query.ToString();

                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    return client.DownloadString(uriBuilder.Uri);
                }
            }
            catch (Exception ex)
            {
                LogManager.GetLogger("ASC.Web").Error("GetBarTips", ex);
                return null;
            }
        }

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
                LogManager.GetLogger("ASC.Web").Error("MarkBarTip", ex);
            }
        }

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
                LogManager.GetLogger("ASC.Web").Error("DeleteBarTips", ex);
            }
        }

        [Read("search")]
        public IEnumerable<object> GetSearchSettings()
        {
            return SearchSettings.GetAllItems().Select(r => new
            {
                id =r.ID,
                title = r.Title,
                enabled = r.Enabled
            });
        }

        [Read("search/state")]
        public object CheckSearchAvailable()
        {
            return FactoryIndexer.GetState();
        }

        [Create("search/reindex")]
        public object Reindex(string name)
        {
            FactoryIndexer.Reindex(name);
            return CheckSearchAvailable();
        }

        [Create("search")]
        public void SetSearchSettings(List<SearchSettingsItem> items)
        {
            SearchSettings.Set(items);
        }

        /// <summary>
        ///    Get random password
        /// </summary>
        /// <short>Get random password</short>
        ///<visible>false</visible>
        [Read(@"randompwd")]
        public string GetRandomPassword()
        {
            var password = UserManagerWrapper.GeneratePassword();
            return password;
        }
    }
}