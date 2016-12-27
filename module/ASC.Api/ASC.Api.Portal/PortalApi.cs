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


using System.Collections.Generic;
using ASC.Api.Attributes;
using ASC.Api.Impl;
using ASC.Api.Interfaces;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Contracts;
using ASC.Core.Common.Notify.Push;
using ASC.Core.Notify.Jabber;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Mobile;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Utility;
using System;
using System.Linq;
using Resources;
using System.Security;
using SecurityContext = ASC.Core.SecurityContext;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.PublicResources;

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
            return ASC.Common.Utils.LinkShorterUtil.GetShortenLink(link, log4net.LogManager.GetLogger("ASC.Web"));
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
                var username = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).UserName;
                return new JabberServiceClient().GetNewMessagesCount(TenantProvider.CurrentTenantID, username);
            }
            catch
            {
            }
            return 0;
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
        public void CreateBackupSchedule(BackupStorageType storageType, BackupAjaxHandler.StorageParams storageParams, int backupsStored, BackupAjaxHandler.CronParams cronParams, bool backupMail)
        {
            backupHandler.CreateSchedule(storageType, storageParams, backupsStored, cronParams, backupMail);
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
        public BackupProgress StartBackup(BackupStorageType storageType, BackupAjaxHandler.StorageParams storageParams, bool backupMail)
        {
            return backupHandler.StartBackup(storageType, storageParams, backupMail);
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
        public BackupProgress StartBackupRestore(string backupId, BackupStorageType storageType, BackupAjaxHandler.StorageParams storageParams, bool notify)
        {
            return backupHandler.StartRestore(backupId, storageType, storageParams, notify);
        }

        /// <summary>
        /// Returns the progress of the started restore
        /// </summary>
        /// <category>Backup</category>
        /// <returns>Restore Progress</returns>
        [Read("getrestoreprogress")]
        public BackupProgress GetRestoreProgress()
        {
            return backupHandler.GetRestoreProgress();
        }



        ///<visible>false</visible>
        [Update("portalrename")]
        public object UpdatePortalName(string alias)
        {
            var enabled = SetupInfo.IsVisibleSettings("PortalRename");
            if (!enabled)
                throw new SecurityException(Resources.Resource.PortalAccessSettingsTariffException);

            if (CoreContext.Configuration.Personal)
                throw new Exception(Resource.ErrorAccessDenied);

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (String.IsNullOrEmpty(alias)) throw new ArgumentException();


            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var user = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var newAlias = alias.ToLowerInvariant();
            var oldAlias = tenant.TenantAlias;
            var oldVirtualRootPath = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');

            if (!String.Equals(newAlias, oldAlias, StringComparison.InvariantCultureIgnoreCase))
            {
                var hostedSolution = new HostedSolution(ConfigurationManager.ConnectionStrings["default"]);
                if (!String.IsNullOrEmpty(SetupInfo.ApiSystemUrl))
                {
                    ValidatePortalName(newAlias);
                }
                else
                {
                    hostedSolution.CheckTenantAddress(newAlias.Trim());
                }


                if (!String.IsNullOrEmpty(SetupInfo.ApiCacheUrl))
                {
                    AddTenantToCache(newAlias);
                }

                tenant.TenantAlias = alias;
                tenant = hostedSolution.SaveTenant(tenant);


                if (!String.IsNullOrEmpty(SetupInfo.ApiCacheUrl))
                {
                    RemoveTenantFromCache(oldAlias);
                }

                StudioNotifyService.Instance.PortalRenameNotify(oldVirtualRootPath);
            }
            else
            {
                throw new Exception(ResourceJS.ErrorPortalNameWasNotChanged);
            }

            var reference = CreateReference(Request, tenant.TenantDomain, tenant.TenantId, user.Email);

            return new {
                message = Resources.Resource.SuccessfullyPortalRenameMessage,
                reference = reference
            };
        }


        private void ValidatePortalName(string domain)
        {
            var absoluteApiSystemUrl = SetupInfo.ApiSystemUrl;
            Uri uri;
            if (!Uri.TryCreate(absoluteApiSystemUrl, UriKind.Absolute, out uri))
            {
                var appUrl = CommonLinkUtility.GetFullAbsolutePath("/");
                absoluteApiSystemUrl = string.Format("{0}/{1}", appUrl.TrimEnd('/'), absoluteApiSystemUrl.TrimStart('/')).TrimEnd('/');
            }

            var data = string.Format("portalName={0}", domain);
            var url = String.Format("{0}/registration/validateportalname", absoluteApiSystemUrl);

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = WebRequestMethods.Http.Post;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            using (var writer = new StreamWriter(webRequest.GetRequestStream()))
            {
                writer.Write(data);
            }

            var result = "";

            using (var response = webRequest.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();

                var resObj = JObject.Parse(result);
                if (resObj["errors"] != null && resObj["errors"].HasValues)
                {
                    throw new Exception(result);
                }
            }
        }

        #region api cache

        private static string CreateApiCacheAuthToken(string pkey)
        {
            var skey = ConfigurationManager.AppSettings["core.machinekey"];

            using (var hasher = new HMACSHA1(Encoding.UTF8.GetBytes(skey)))
            {
                var now = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                var hash = HttpServerUtility.UrlTokenEncode(hasher.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n", now, pkey))));
                return string.Format("ASC {0}:{1}:{2}", pkey, now, hash);
            }
        }

        private void AddTenantToCache(string domain)
        {
            SendApiToCache("addportal", WebRequestMethods.Http.Post, string.Format("={0}", domain));
        }

        private void RemoveTenantFromCache(string domain)
        {
            SendApiToCache("removeportal", WebRequestMethods.Http.Post, string.Format("={0}", domain));
        }

        private void SendApiToCache(string apiPath, string httpMethod, string data)
        {
            var absoluteApiCacheUrl = SetupInfo.ApiCacheUrl;
            Uri uri;
            if (!Uri.TryCreate(absoluteApiCacheUrl, UriKind.Absolute, out uri)) {
                var appUrl = CommonLinkUtility.GetFullAbsolutePath("/");
                absoluteApiCacheUrl = string.Format("{0}/{1}", appUrl.TrimEnd('/'), absoluteApiCacheUrl.TrimStart('/')).TrimEnd('/');
            }

            var url = String.Format("{0}/cache/{1}", absoluteApiCacheUrl, apiPath);

            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = httpMethod;
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;

            webRequest.Headers.Add(HttpRequestHeader.Authorization, CreateApiCacheAuthToken(SecurityContext.CurrentAccount.ID.ToString()));

            using (var writer = new StreamWriter(webRequest.GetRequestStream()))
            {
                writer.Write(data);
            }

            using (var webResponse = webRequest.GetResponse())
            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                var response = reader.ReadToEnd();
                var resObj = JObject.Parse(response);
                if (resObj["errors"] != null && resObj["errors"].HasValues)
                {
                    throw new Exception(response);
                }
            }
        }

        #endregion

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
    }
}