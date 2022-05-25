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
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security;
using System.ServiceModel.Security;
using System.Web;
using System.Web.Optimization;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Employee;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Logging;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Common.Configuration;
using ASC.Core.Common.Contracts;
using ASC.Core.Common.Notify;
using ASC.Core.Encryption;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Data.Storage.Configuration;
using ASC.Data.Storage.Encryption;
using ASC.Data.Storage.Migration;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Web.Core;
using ASC.Web.Core.Sms;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WebZones;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Quota;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Core.Statistic;
using ASC.Web.Studio.Core.TFA;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.Utility;

using SecurityContext = ASC.Core.SecurityContext;
using StorageHelper = ASC.Web.Studio.UserControls.CustomNavigation.StorageHelper;

namespace ASC.Api.Settings
{
    ///<summary>
    /// Portal settings.
    ///</summary>
    public partial class SettingsApi : IApiEntryPoint
    {
        private const int ONE_THREAD = 1;
        private ILog Log = LogManager.GetLogger("ASC");
        private static readonly DistributedTaskQueue ldapTasks = new DistributedTaskQueue("ldapOperations");
        private static readonly DistributedTaskQueue quotaTasks = new DistributedTaskQueue("quotaOperations", ONE_THREAD);
        private static readonly DistributedTaskQueue smtpTasks = new DistributedTaskQueue("smtpOperations");

        public string Name
        {
            get { return "settings"; }
        }

        private static HttpRequest Request
        {
            get { return HttpContext.Current.Request; }
        }

        private static int CurrentTenant
        {
            get { return CoreContext.TenantManager.GetCurrentTenant().TenantId; }
        }

        private static Guid CurrentUser
        {
            get { return SecurityContext.CurrentAccount.ID; }
        }

        private static DistributedTaskQueue LDAPTasks
        {
            get { return ldapTasks; }
        }

        private static DistributedTaskQueue SMTPTasks
        {
            get { return smtpTasks; }
        }

        ///<summary>
        /// Returns a list of all the available portal settings with the current values for each parameter.
        ///</summary>
        ///<short>
        /// Get the portal settings
        ///</short>
        ///<category>Common settings</category>
        ///<returns>Settings</returns>
        [Read("")]
        public SettingsWrapper GetSettings()
        {
            var settings = new SettingsWrapper();
            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            settings.Timezone = tenant.TimeZone.ToSerializedString();
            settings.UtcOffset = tenant.TimeZone.GetUtcOffset(DateTime.UtcNow);
            settings.UtcHoursOffset = tenant.TimeZone.GetUtcOffset(DateTime.UtcNow).TotalHours;
            settings.TrustedDomains = tenant.TrustedDomains;
            settings.TrustedDomainsType = tenant.TrustedDomainsType;
            settings.Culture = tenant.GetCulture().ToString();
            return settings;
        }

        ///<summary>
        /// Returns the space usage quota for the portal with the space usage of each module.
        ///</summary>
        ///<short>
        /// Get the space usage
        ///</short>
        ///<category>Quota</category>
        ///<returns>Space usage and limits for upload</returns>
        [Read("quota")]
        public QuotaWrapper GetQuotaUsed()
        {
            return QuotaWrapper.GetCurrent();
        }

        ///<summary>
        /// Starts the process of recalculating quota.
        ///</summary>
        ///<short>
        /// Recalculates quota 
        ///</short>
        ///<category>Quota</category>
        [Read("recalculatequota")]
        public void RecalculateQuota()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var operations = quotaTasks.GetTasks()
                .Where(t => t.GetProperty<int>(QuotaSync.TenantIdKey) == TenantProvider.CurrentTenantID);

            if (operations.Any(o => o.Status <= DistributedTaskStatus.Running))
            {
                throw new InvalidOperationException(Resource.LdapSettingsTooManyOperations);
            }

            var op = new QuotaSync(TenantProvider.CurrentTenantID);

            quotaTasks.QueueTask(op.RunJob, op.GetDistributedTask());
        }

        ///<summary>
        /// Checks the process of recalculating quota.
        ///</summary>
        ///<short>
        /// Check quota recalculating
        ///</short>
        ///<category>Quota</category>
        ///<returns>Boolean value: True - quota recalculating process is enabled, False - quota recalculating process is disabled</returns>
        [Read("checkrecalculatequota")]
        public bool CheckRecalculateQuota()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var task = quotaTasks.GetTasks().FirstOrDefault(t => t.GetProperty<int>(QuotaSync.TenantIdKey) == TenantProvider.CurrentTenantID);

            if (task != null && task.Status == DistributedTaskStatus.Completed)
            {
                quotaTasks.RemoveTask(task.Id);
                return false;
            }

            return task != null;
        }

        /// <summary>
        /// Returns the current build version.
        /// </summary>
        /// <short>Get the current build version</short>
        /// <category>Versions</category>
        /// <visible>false</visible>
        /// <returns>Current onlyoffice, editor, mailserver versions</returns>
        [Read("version/build", false, false)] //NOTE: this method doesn't require auth!!!  //NOTE: this method doesn't check payment!!!
        public BuildVersion GetBuildVersions()
        {
            return BuildVersion.GetCurrentBuildVersion();
        }

        /// <summary>
        /// Returns a list of the availibe portal versions including the current version.
        /// </summary>
        /// <short>
        /// Get the portal versions
        /// </short>
        /// <category>Versions</category>
        /// <visible>false</visible>
        /// <returns>List of availibe portal versions including the current version</returns>
        [Read("version")]
        public TenantVersionWrapper GetVersions()
        {
            return new TenantVersionWrapper(CoreContext.TenantManager.GetCurrentTenant().Version, CoreContext.TenantManager.GetTenantVersions());
        }

        /// <summary>
        /// Sets the current portal version to the one with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Change the portal version
        /// </short>
        /// <category>Versions</category>
        /// <param name="versionId">Version ID</param>
        /// <visible>false</visible>
        /// <returns>List of availibe portal versions including the current version</returns>
        [Update("version")]
        public TenantVersionWrapper SetVersion(int versionId)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            CoreContext.TenantManager.GetTenantVersions().FirstOrDefault(r => r.Id == versionId).NotFoundIfNull();

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            CoreContext.TenantManager.SetTenantVersion(tenant, versionId);
            return GetVersions();
        }

        /// <summary>
        /// Returns the security settings for the product, module or add-ons specified in the request.
        /// </summary>
        /// <short>
        /// Get the security settings
        /// </short>
        /// <category>Security</category>
        /// <param name="ids">List of module IDs</param>
        /// <returns>Security settings</returns>
        [Read("security")]
        public IEnumerable<SecurityWrapper> GetWebItemSecurityInfo(IEnumerable<string> ids)
        {
            if (ids == null || !ids.Any())
            {
                ids = WebItemManager.Instance.GetItemsAll().Select(i => i.ID.ToString());
            }

            var subItemList = WebItemManager.Instance.GetItemsAll().Where(item => item.IsSubItem()).Select(i => i.ID.ToString());

            return ids.Select(WebItemSecurity.GetSecurityInfo)
                      .Select(i => new SecurityWrapper
                      {
                          WebItemId = i.WebItemId,
                          Enabled = i.Enabled,
                          Users = i.Users.Select(EmployeeWraper.Get),
                          Groups = i.Groups.Select(g => new GroupWrapperSummary(g)),
                          IsSubItem = subItemList.Contains(i.WebItemId),
                      }).ToList();
        }

        /// <summary>
        /// Returns the availability of the module with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get the module availability
        /// </short>
        /// <category>Security</category>
        /// <param name="id">Module ID</param>
        /// <returns>Boolean value: True - module is enabled, False - module is disabled</returns>
        [Read("security/{id}")]
        public bool GetWebItemSecurityInfo(Guid id)
        {
            var module = WebItemManager.Instance[id];

            return module != null && !module.IsDisabled();
        }

        /// <summary>
        /// Returns a list of all the enabled modules.
        /// </summary>
        /// <short>
        /// Get the enabled modules
        /// </short>
        /// <category>Security</category>
        /// <returns>List of enabled modules</returns>
        /// <visible>false</visible>
        [Read("security/modules")]
        public object GetEnabledModules()
        {
            var EnabledModules = WebItemManager.Instance.GetItems(WebZoneType.All, ItemAvailableState.Normal)
                                        .Where(item => !item.IsSubItem() && item.Visible)
                                        .ToList()
                                        .Select(item => new
                                        {
                                            id = item.ProductClassName.HtmlEncode(),
                                            title = item.Name.HtmlEncode()
                                        });

            return EnabledModules;
        }

        /// <summary>
        /// Returns the portal password settings.
        /// </summary>
        /// <short>
        /// Get the password settings
        /// </short>
        /// <category>Security</category>
        /// <returns>Password settings</returns>
        [Read("security/password")]
        public object GetPasswordSettings()
        {
            var UserPasswordSettings = PasswordSettings.Load();

            return UserPasswordSettings;
        }

        /// <summary>
        /// Sets the security settings to the product, module or add-ons.
        /// </summary>
        /// <short>
        /// Set the security settings
        /// </short>
        /// <category>Security</category>
        /// <param name="id">Module ID</param>
        /// <param name="enabled">Specifies if the module is enabled or not</param>
        /// <param name="subjects">List of user/group IDs</param>
        /// <returns>Security settings</returns>
        [Update("security")]
        public IEnumerable<SecurityWrapper> SetWebItemSecurity(string id, bool enabled, IEnumerable<Guid> subjects)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            WebItemSecurity.SetSecurity(id, enabled, subjects != null ? subjects.ToArray() : null);
            var securityInfo = GetWebItemSecurityInfo(new List<string> { id });

            if (subjects == null) return securityInfo;

            var productName = GetProductName(new Guid(id));

            if (!subjects.Any())
            {
                MessageService.Send(Request, MessageAction.ProductAccessOpened, productName);
            }
            else
            {
                foreach (var info in securityInfo)
                {
                    if (info.Groups.Any())
                    {
                        MessageService.Send(Request, MessageAction.GroupsOpenedProductAccess, productName, info.Groups.Select(x => x.Name));
                    }
                    if (info.Users.Any())
                    {
                        MessageService.Send(Request, MessageAction.UsersOpenedProductAccess, productName, info.Users.Select(x => HttpUtility.HtmlDecode(x.DisplayName)));
                    }
                }
            }

            return securityInfo;
        }

        /// <summary>
        /// Sets access to the products, modules or add-ons specified in the request.
        /// </summary>
        /// <short>
        /// Set access to web items
        /// </short>
        /// <category>Security</category>
        /// <param name="items">Modules, products or add-ons with security information</param>
        /// <returns>Security settings</returns>
        [Update("security/access")]
        public IEnumerable<SecurityWrapper> SetAccessToWebItems(IEnumerable<ItemKeyValuePair<String, Boolean>> items)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var itemList = new ItemDictionary<String, Boolean>();

            foreach (ItemKeyValuePair<String, Boolean> item in items)
            {
                if (!itemList.ContainsKey(item.Key))
                    itemList.Add(item.Key, item.Value);
            }

            var defaultPageSettings = StudioDefaultPageSettings.Load();

            foreach (var item in itemList)
            {
                Guid[] subjects = null;
                var productId = new Guid(item.Key);

                if (item.Value)
                {
                    var webItem = WebItemManager.Instance[productId] as IProduct;
                    if (webItem != null || productId == WebItemManager.MailProductID)
                    {
                        var productInfo = WebItemSecurity.GetSecurityInfo(item.Key);
                        var selectedGroups = productInfo.Groups.Select(group => group.ID).ToList();
                        var selectedUsers = productInfo.Users.Select(user => user.ID).ToList();
                        selectedUsers.AddRange(selectedGroups);
                        if (selectedUsers.Count > 0)
                        {
                            subjects = selectedUsers.ToArray();
                        }
                    }
                }
                else if (productId == defaultPageSettings.DefaultProductID)
                {
                    ((StudioDefaultPageSettings)defaultPageSettings.GetDefault()).Save();
                }

                WebItemSecurity.SetSecurity(item.Key, item.Value, subjects);
            }

            MessageService.Send(Request, MessageAction.ProductsListUpdated);

            return GetWebItemSecurityInfo(itemList.Keys.ToList());
        }

        /// <summary>
        /// Returns a list of all the administrators of the product with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get the product administrators
        /// </short>
        /// <category>Security</category>
        /// <param name="productid">Product ID</param>
        /// <returns>List of product administrators</returns>
        [Read("security/administrator/{productid}")]
        public IEnumerable<EmployeeWraper> GetProductAdministrators(Guid productid)
        {
            return WebItemSecurity.GetProductAdministrators(productid)
                                  .Select(EmployeeWraper.Get)
                                  .ToList();
        }

        /// <summary>
        /// Checks if the selected user is an administrator of a product with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Check the product administrator
        /// </short>
        /// <category>Security</category>
        /// <param name="productid">Product ID</param>
        /// <param name="userid">User ID</param>
        /// <returns>Object with user security information</returns>
        [Read("security/administrator")]
        public object IsProductAdministrator(Guid productid, Guid userid)
        {
            var result = WebItemSecurity.IsProductAdministrator(productid, userid);
            return new { ProductId = productid, UserId = userid, Administrator = result, };
        }

        /// <summary>
        /// Sets the selected user as an administrator of a product with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Set the product administrator
        /// </short>
        /// <category>Security</category>
        /// <param name="productid">Product ID</param>
        /// <param name="userid">User ID</param>
        /// <param name="administrator">Specifies if the user will be added to the product as an administrator or deleted from it</param>
        /// <returns>Object with user security information</returns>
        [Update("security/administrator")]
        public object SetProductAdministrator(Guid productid, Guid userid, bool administrator)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var isStartup = !CoreContext.Configuration.CustomMode && TenantExtra.Saas && TenantExtra.GetTenantQuota().Free;
            if (isStartup)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "Administrator");
            }

            WebItemSecurity.SetProductAdministrator(productid, userid, administrator);

            var admin = CoreContext.UserManager.GetUsers(userid);

            if (productid == Guid.Empty)
            {
                var messageAction = administrator ? MessageAction.AdministratorOpenedFullAccess : MessageAction.AdministratorDeleted;
                MessageService.Send(Request, messageAction, MessageTarget.Create(admin.ID), admin.DisplayUserName(false));
            }
            else
            {
                var messageAction = administrator ? MessageAction.ProductAddedAdministrator : MessageAction.ProductDeletedAdministrator;
                MessageService.Send(Request, messageAction, MessageTarget.Create(admin.ID), GetProductName(productid), admin.DisplayUserName(false));
            }

            return new { ProductId = productid, UserId = userid, Administrator = administrator };
        }

        /// <summary>
        /// Returns the portal logo image URL.
        /// </summary>
        /// <short>
        /// Get a portal logo
        /// </short>
        /// <category>Common settings</category>
        /// <returns>Portal logo image URL</returns>
        [Read("logo")]
        public string GetLogo()
        {
            return TenantInfoSettings.Load().GetAbsoluteCompanyLogoPath();
        }


        /// <summary>
        /// Saves the white label settings specified in the request.
        /// </summary>
        /// <short>
        /// Save the white label settings
        /// </short>
        /// <category>Rebranding</category>
        /// <param name="logoText">Logo text</param>
        /// <param name="logo">Tenant IDs with their logos</param>
        /// <param name="isDefault">Specifies if the default settings will be saved or not</param>
        ///<visible>false</visible>
        [Create("whitelabel/save")]
        public void SaveWhiteLabelSettings(string logoText, IEnumerable<ItemKeyValuePair<int, string>> logo, bool isDefault)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandWhiteLabelPermission();

            if (isDefault)
            {
                DemandRebrandingPermission();
                SaveWhiteLabelSettingsForDefaultTenant(logoText, logo);
            }
            else
            {
                SaveWhiteLabelSettingsForCurrentTenant(logoText, logo);
            }
        }

        private void SaveWhiteLabelSettingsForCurrentTenant(string logoText, IEnumerable<ItemKeyValuePair<int, string>> logo)
        {
            var settings = TenantWhiteLabelSettings.Load();

            SaveWhiteLabelSettingsForTenant(settings, null, TenantProvider.CurrentTenantID, logoText, logo);
        }

        private void SaveWhiteLabelSettingsForDefaultTenant(string logoText, IEnumerable<ItemKeyValuePair<int, string>> logo)
        {
            var settings = TenantWhiteLabelSettings.LoadForDefaultTenant();
            var storage = StorageFactory.GetStorage(string.Empty, "static_partnerdata");

            SaveWhiteLabelSettingsForTenant(settings, storage, Tenant.DEFAULT_TENANT, logoText, logo);
        }

        private static void SaveWhiteLabelSettingsForTenant(TenantWhiteLabelSettings settings, IDataStore storage, int tenantId, string logoText, IEnumerable<ItemKeyValuePair<int, string>> logo)
        {
            if (logo != null)
            {
                var logoDict = new Dictionary<int, string>();
                logo.ToList().ForEach(n => logoDict.Add(n.Key, n.Value));

                settings.SetLogo(logoDict, storage);
            }

            settings.LogoText = logoText;
            settings.Save(tenantId);
        }


        /// <summary>
        /// Saves the white label settings from files specified in the request.
        /// </summary>
        /// <short>
        /// Save the white label settings from files
        /// </short>
        /// <category>Rebranding</category>
        /// <param name="attachments">Files with white label settings</param>
        /// <param name="isDefault">Specifies if the default settings will be saved or not</param>
        ///<visible>false</visible>
        [Create("whitelabel/savefromfiles")]
        public void SaveWhiteLabelSettingsFromFiles(IEnumerable<HttpPostedFileBase> attachments, bool isDefault)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandWhiteLabelPermission();

            if (attachments == null || !attachments.Any())
            {
                throw new InvalidOperationException("No input files");
            }

            if (isDefault)
            {
                DemandRebrandingPermission();
                SaveWhiteLabelSettingsFromFilesForDefaultTenant(attachments);
            }
            else
            {
                SaveWhiteLabelSettingsFromFilesForCurrentTenant(attachments);
            }
        }

        public void SaveWhiteLabelSettingsFromFilesForCurrentTenant(IEnumerable<HttpPostedFileBase> attachments)
        {
            var settings = TenantWhiteLabelSettings.Load();

            SaveWhiteLabelSettingsFromFilesForTenant(settings, null, TenantProvider.CurrentTenantID, attachments);
        }

        public void SaveWhiteLabelSettingsFromFilesForDefaultTenant(IEnumerable<HttpPostedFileBase> attachments)
        {
            var settings = TenantWhiteLabelSettings.LoadForDefaultTenant();
            var storage = StorageFactory.GetStorage(string.Empty, "static_partnerdata");

            SaveWhiteLabelSettingsFromFilesForTenant(settings, storage, Tenant.DEFAULT_TENANT, attachments);
        }

        public void SaveWhiteLabelSettingsFromFilesForTenant(TenantWhiteLabelSettings settings, IDataStore storage, int tenantId, IEnumerable<HttpPostedFileBase> attachments)
        {
            foreach (var f in attachments)
            {
                var parts = f.FileName.Split('.');
                var logoType = (WhiteLabelLogoTypeEnum)(Convert.ToInt32(parts[0]));
                var fileExt = parts[1];
                settings.SetLogoFromStream(logoType, fileExt, f.InputStream, storage);
            }

            settings.Save(tenantId);
        }


        /// <summary>
        /// Returns the white label sizes.
        /// </summary>
        /// <short>
        /// Get the white label sizes
        /// </short>
        /// <category>Rebranding</category>
        /// <returns>White label sizes</returns>
        ///<visible>false</visible>
        [Read("whitelabel/sizes")]
        public object GetWhiteLabelSizes()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandWhiteLabelPermission();

            return
            new[]
            {
                new {type = (int)WhiteLabelLogoTypeEnum.LightSmall, name = WhiteLabelLogoTypeEnum.LightSmall.ToString(), height = TenantWhiteLabelSettings.logoLightSmallSize.Height, width = TenantWhiteLabelSettings.logoLightSmallSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.Dark, name = WhiteLabelLogoTypeEnum.Dark.ToString(), height = TenantWhiteLabelSettings.logoDarkSize.Height, width = TenantWhiteLabelSettings.logoDarkSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.Favicon, name = WhiteLabelLogoTypeEnum.Favicon.ToString(), height = TenantWhiteLabelSettings.logoFaviconSize.Height, width = TenantWhiteLabelSettings.logoFaviconSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.DocsEditor, name = WhiteLabelLogoTypeEnum.DocsEditor.ToString(), height = TenantWhiteLabelSettings.logoDocsEditorSize.Height, width = TenantWhiteLabelSettings.logoDocsEditorSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.DocsEditorEmbed, name = WhiteLabelLogoTypeEnum.DocsEditorEmbed.ToString(), height = TenantWhiteLabelSettings.logoDocsEditorEmbedSize.Height, width = TenantWhiteLabelSettings.logoDocsEditorEmbedSize.Width}
            };
        }



        /// <summary>
        /// Returns the white label logos.
        /// </summary>
        /// <short>
        /// Get the white label logos
        /// </short>
        /// <category>Rebranding</category>
        /// <param name="retina">Specifies if the logos will be for the retina screens or not</param>
        /// <param name="isDefault">Specifies if the default settings will be saved or not</param>
        /// <returns>White label logos</returns>
        ///<visible>false</visible>
        [Read("whitelabel/logos")]
        public Dictionary<int, string> GetWhiteLabelLogos(bool retina, bool isDefault)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandWhiteLabelPermission();

            var result = new Dictionary<int, string>();

            if (isDefault)
            {
                result.Add((int)WhiteLabelLogoTypeEnum.LightSmall, CommonLinkUtility.GetFullAbsolutePath(TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.LightSmall, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.Dark, CommonLinkUtility.GetFullAbsolutePath(TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.Favicon, CommonLinkUtility.GetFullAbsolutePath(TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Favicon, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.DocsEditor, CommonLinkUtility.GetFullAbsolutePath(TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.DocsEditorEmbed, CommonLinkUtility.GetFullAbsolutePath(TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.DocsEditorEmbed, !retina)));
            }
            else
            {
                var settings = TenantWhiteLabelSettings.Load();

                result.Add((int)WhiteLabelLogoTypeEnum.LightSmall, CommonLinkUtility.GetFullAbsolutePath(settings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.LightSmall, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.Dark, CommonLinkUtility.GetFullAbsolutePath(settings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Dark, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.Favicon, CommonLinkUtility.GetFullAbsolutePath(settings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.DocsEditor, CommonLinkUtility.GetFullAbsolutePath(settings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, !retina)));
                result.Add((int)WhiteLabelLogoTypeEnum.DocsEditorEmbed, CommonLinkUtility.GetFullAbsolutePath(settings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditorEmbed, !retina)));
            }

            return result;
        }

        /// <summary>
        /// Returns the white label logo text.
        /// </summary>
        /// <short>
        /// Get the white label logo text
        /// </short>
        /// <category>Rebranding</category>
        /// <param name="isDefault">Specifies if the default settings will be saved or not</param>
        /// <returns>Logo text</returns>
        ///<visible>false</visible>
        [Read("whitelabel/logotext")]
        public string GetWhiteLabelLogoText(bool isDefault)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandWhiteLabelPermission();

            var settings = isDefault ? TenantWhiteLabelSettings.LoadForDefaultTenant() : TenantWhiteLabelSettings.Load();

            return settings.LogoText ?? TenantWhiteLabelSettings.DefaultLogoText;
        }


        /// <summary>
        /// Restores the white label options.
        /// </summary>
        /// <short>
        /// Restore the white label options
        /// </short>
        /// <category>Rebranding</category>
        /// <param name="isDefault">Specifies if the default settings will be saved or not</param>
        ///<visible>false</visible>
        [Update("whitelabel/restore")]
        public void RestoreWhiteLabelOptions(bool isDefault)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            DemandWhiteLabelPermission();

            if (isDefault)
            {
                DemandRebrandingPermission();
                RestoreWhiteLabelOptionsForDefaultTenant();
            }
            else
            {
                RestoreWhiteLabelOptionsForCurrentTenant();
            }
        }

        public void RestoreWhiteLabelOptionsForCurrentTenant()
        {
            var settings = TenantWhiteLabelSettings.Load();

            RestoreWhiteLabelOptionsForTenant(settings, null, TenantProvider.CurrentTenantID);

            var tenantInfoSettings = TenantInfoSettings.Load();
            tenantInfoSettings.RestoreDefaultLogo();
            tenantInfoSettings.Save();
        }

        public void RestoreWhiteLabelOptionsForDefaultTenant()
        {
            var settings = TenantWhiteLabelSettings.LoadForDefaultTenant();
            var storage = StorageFactory.GetStorage(string.Empty, "static_partnerdata");

            RestoreWhiteLabelOptionsForTenant(settings, storage, Tenant.DEFAULT_TENANT);
        }

        public void RestoreWhiteLabelOptionsForTenant(TenantWhiteLabelSettings settings, IDataStore storage, int tenantId)
        {
            settings.RestoreDefault(tenantId, storage);
        }

        /// <summary>
        /// Returns the IP portal restrictions.
        /// </summary>
        /// <short>Get the IP portal restrictions</short>
        /// <category>IP restrictions</category>
        /// <returns>IP restrictions</returns>
        [Read("/iprestrictions")]
        public IEnumerable<IPRestriction> GetIpRestrictions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return IPRestrictionsService.Get(CurrentTenant);
        }

        /// <summary>
        /// Saves the new portal IP restrictions specified in the request.
        /// </summary>
        /// <short>Save the IP restrictions</short>
        /// <category>IP restrictions</category>
        /// <param name="ips">New IP restrictions</param>
        /// <returns>New IP restrictions</returns>
        [Update("iprestrictions")]
        public IEnumerable<string> SaveIpRestrictions(IEnumerable<string> ips)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return IPRestrictionsService.Save(ips, CurrentTenant);
        }

        /// <summary>
        /// Updates the IP restriction settings with a parameter specified in the request.
        /// </summary>
        /// <short>Update the IP restriction settings</short>
        /// <category>IP restrictions</category>
        /// <param name="enable">Enables IP restrictions</param>
        /// <returns>Updated IP restriction settings</returns>
        [Update("iprestrictions/settings")]
        public IPRestrictionsSettings UpdateIpRestrictionsSettings(bool enable)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = new IPRestrictionsSettings { Enable = enable };
            settings.Save();

            return settings;
        }

        /// <summary>
        /// Updates the tip settings with a parameter specified in the request.
        /// </summary>
        /// <short>Update the tip settings</short>
        /// <category>Tips</category>
        /// <param name="show">Shows tips for the user</param>
        /// <returns>Updated tip settings</returns>
        [Update("tips")]
        public TipsSettings UpdateTipsSettings(bool show)
        {
            var settings = new TipsSettings { Show = show };
            settings.SaveForCurrentUser();

            if (!show && !string.IsNullOrEmpty(SetupInfo.TipsAddress))
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        var data = new NameValueCollection();
                        data["userId"] = CurrentUser.ToString();
                        data["tenantId"] = CurrentTenant.ToString(CultureInfo.InvariantCulture);

                        client.UploadValues(string.Format("{0}/tips/deletereaded", SetupInfo.TipsAddress), data);
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.Message, e);
                }
            }

            return settings;
        }

        /// <summary>
        /// Updates the tip subscription.
        /// </summary>
        /// <short>Update the tip subscription</short>
        /// <category>Tips</category>
        /// <returns>Updated tip subscription</returns>
        [Update("tips/change/subscription")]
        public bool UpdateTipsSubscription()
        {
            return StudioPeriodicNotify.ChangeSubscription(SecurityContext.CurrentAccount.ID);
        }

        /// <summary>
        /// Completes the Wizard settings.
        /// </summary>
        /// <short>Complete the Wizard settings</short>
        /// <category>Wizard</category>
        /// <returns>Wizard settings</returns>
        /// <visible>false</visible>
        [Update("wizard/complete")]
        public WizardSettings CompleteWizard()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = WizardSettings.Load();

            if (settings.Completed)
                return settings;

            settings.Completed = true;
            settings.Save();

            return settings;
        }

        /// <summary>
        /// Updates the two-factor authentication settings with the type specified in the request.
        /// </summary>
        /// <short>Update the TFA settings</short>
        /// <category>TFA settings</category>
        /// <param name="type">TFA type: sms, app or none</param>
        /// <returns>True if an operation is successful</returns>
        [Update("tfaapp")]
        public bool TfaSettings(string type)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var result = false;

            MessageAction action;
            switch (type)
            {
                case "sms":
                    if (!StudioSmsNotificationSettings.IsVisibleAndAvailableSettings)
                        throw new Exception(Resource.SmsNotAvailable);

                    if (!SmsProviderManager.Enabled())
                        throw new MethodAccessException();

                    StudioSmsNotificationSettings.Enable = true;
                    action = MessageAction.TwoFactorAuthenticationEnabledBySms;

                    if (TfaAppAuthSettings.Enable)
                    {
                        TfaAppAuthSettings.Enable = false;
                    }

                    result = true;

                    break;

                case "app":
                    if (!TfaAppAuthSettings.IsVisibleSettings)
                    {
                        throw new Exception(Resource.TfaAppNotAvailable);
                    }

                    TfaAppAuthSettings.Enable = true;
                    action = MessageAction.TwoFactorAuthenticationEnabledByTfaApp;

                    if (StudioSmsNotificationSettings.IsVisibleAndAvailableSettings && StudioSmsNotificationSettings.Enable)
                    {
                        StudioSmsNotificationSettings.Enable = false;
                    }

                    result = true;

                    break;

                default:
                    if (TfaAppAuthSettings.Enable)
                    {
                        TfaAppAuthSettings.Enable = false;
                    }

                    if (StudioSmsNotificationSettings.IsVisibleAndAvailableSettings && StudioSmsNotificationSettings.Enable)
                    {
                        StudioSmsNotificationSettings.Enable = false;
                    }

                    action = MessageAction.TwoFactorAuthenticationDisabled;

                    break;
            }

            if (result)
            {
                CookiesManager.ResetTenantCookie();
            }

            MessageService.Send(Request, action);
            return result;
        }

        /// <summary>
        /// Returns the two-factor authentication application codes.
        /// </summary>
        /// <short>Get the TFA codes</short>
        /// <category>TFA settings</category>
        /// <returns>List of TFA application codes</returns>
        ///<visible>false</visible>
        [Read("tfaappcodes")]
        public IEnumerable<object> TfaAppGetCodes()
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!TfaAppAuthSettings.IsVisibleSettings || !TfaAppUserSettings.EnableForUser(currentUser.ID))
                throw new Exception(Resource.TfaAppNotAvailable);

            if (currentUser.IsVisitor() || currentUser.IsOutsider())
                throw new NotSupportedException("Not available.");

            return TfaAppUserSettings.LoadForCurrentUser().CodesSetting.Select(r => new { r.IsUsed, r.Code }).ToList();
        }

        /// <summary>
        /// Requests the new backup codes for the two-factor authentication application.
        /// </summary>
        /// <short>Request the new TFA codes</short>
        /// <category>TFA settings</category>
        /// <returns>New backup codes</returns>
        [Update("tfaappnewcodes")]
        public IEnumerable<object> TfaAppRequestNewCodes()
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            if (!TfaAppAuthSettings.IsVisibleSettings || !TfaAppUserSettings.EnableForUser(currentUser.ID))
                throw new Exception(Resource.TfaAppNotAvailable);

            if (currentUser.IsVisitor() || currentUser.IsOutsider())
                throw new NotSupportedException("Not available.");

            var codes = currentUser.GenerateBackupCodes().Select(r => new { r.IsUsed, r.Code }).ToList();
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserConnectedTfaApp, MessageTarget.Create(currentUser.ID), currentUser.DisplayUserName(false));
            return codes;
        }

        /// <summary>
        /// Unlinks the current two-factor authentication application from the user account specified in the request.
        /// </summary>
        /// <short>Unlink the TFA application</short>
        /// <category>TFA settings</category>
        /// <param name="id">User ID</param>
        /// <returns>Login URL</returns>
        [Update("tfaappnewapp")]
        public string TfaAppNewApp(Guid id)
        {
            var isMe = id.Equals(Guid.Empty);
            var user = CoreContext.UserManager.GetUsers(isMe ? SecurityContext.CurrentAccount.ID : id);

            if (!isMe && !SecurityContext.CheckPermissions(new UserSecurityProvider(user.ID), Core.Users.Constants.Action_EditUser))
                throw new SecurityAccessDeniedException(Resource.ErrorAccessDenied);

            if (!TfaAppAuthSettings.IsVisibleSettings || !TfaAppUserSettings.EnableForUser(user.ID))
                throw new Exception(Resource.TfaAppNotAvailable);

            if (user.IsVisitor() || user.IsOutsider())
                throw new NotSupportedException("Not available.");

            TfaAppUserSettings.DisableForUser(user.ID);
            MessageService.Send(HttpContext.Current.Request, MessageAction.UserDisconnectedTfaApp, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            if (isMe)
            {
                return CommonLinkUtility.GetConfirmationUrl(user.Email, ConfirmType.TfaActivation);
            }

            StudioNotifyService.Instance.SendMsgTfaReset(user);
            return string.Empty;
        }


        /// <summary>
        /// Returns a link that will connect Telegram Bot to your account.
        /// </summary>
        /// <short>Get the Telegram link</short>
        /// <category>Telegram</category>
        /// <returns>Telegram link</returns>
        /// <visible>false</visible>
        [Read("telegramlink")]
        public string TelegramLink()
        {
            var currentLink = TelegramHelper.Instance.CurrentRegistrationLink(CurrentUser, CurrentTenant);

            if (string.IsNullOrEmpty(currentLink))
            {
                return TelegramHelper.Instance.RegisterUser(CurrentUser, CurrentTenant);
            }
            else
            {
                return currentLink;
            }
        }

        /// <summary>
        /// Checks if the current user is connected to Telegram Bot or not.
        /// </summary>
        /// <short>Check the Telegram connection</short>
        /// <category>Telegram</category>
        /// <returns>Integer value: 0 - not connected, 1 - connected, 2 - awaiting confirmation</returns>
        [Read("telegramisconnected")]
        public int TelegramIsConnected()
        {
            return (int)TelegramHelper.Instance.UserIsConnected(CurrentUser, CurrentTenant);
        }

        /// <summary>
        /// Unlinks Telegram Bot from your account.
        /// </summary>
        /// <short>Unlink Telegram Bot</short>
        /// <category>Telegram</category>
        [Delete("telegramdisconnect")]
        public void TelegramDisconnect()
        {
            TelegramHelper.Instance.Disconnect(CurrentUser, CurrentTenant);
        }

        /// <summary>
        /// Closes the welcome pop-up notification.
        /// </summary>
        /// <short>Close the welcome pop-up notification</short>
        /// <category>Common settings</category>
        [Update("welcome/close")]
        public void CloseWelcomePopup()
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var collaboratorPopupSettings = CollaboratorSettings.LoadForCurrentUser();

            if (!(currentUser.IsVisitor() && collaboratorPopupSettings.FirstVisit && !currentUser.IsOutsider()))
                throw new NotSupportedException("Not available.");

            collaboratorPopupSettings.FirstVisit = false;
            collaboratorPopupSettings.SaveForCurrentUser();
        }

        ///<summary>
        /// Close the admin helper notification.
        /// </summary>
        /// <short>Close the admin helper notification</short>
        /// <category>Common settings</category>
        [Update("closeadminhelper")]
        public void CloseAdminHelper()
        {
            if(!CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin() || CoreContext.Configuration.CustomMode || !CoreContext.Configuration.Standalone)
                throw new NotSupportedException("Not available.");

            var adminHelperSettings = AdminHelperSettings.LoadForCurrentUser();
            adminHelperSettings.Viewed = true;
            adminHelperSettings.SaveForCurrentUser();
        }


        /// <summary>
        /// Saves a portal color theme specified in the request.
        /// </summary>
        /// <short>Save a color theme</short>
        /// <category>Common settings</category>
        /// <param name="theme">Portal theme</param>
        ///<visible>false</visible>
        [Update("colortheme")]
        public void SaveColorTheme(string theme)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            ColorThemesSettings.SaveColorTheme(theme);
            MessageService.Send(HttpContext.Current.Request, MessageAction.ColorThemeChanged);
        }

        /// <summary>
        /// Sets the portal time zone and language specified in the request.
        /// </summary>
        /// <short>Set time zone and language</short>
        /// <category>Common settings</category>
        /// <param name="lng">Language</param>
        /// <param name="timeZoneID">Time zone ID</param>
        /// <returns>Operation result</returns>
        ///<visible>false</visible>
        [Update("timeandlanguage")]
        public string TimaAndLanguage(string lng, string timeZoneID)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            var culture = CultureInfo.GetCultureInfo(lng);

            var changelng = false;
            if (SetupInfo.EnabledCultures.Find(c => String.Equals(c.Name, culture.Name, StringComparison.InvariantCultureIgnoreCase)) != null)
            {
                if (!String.Equals(tenant.Language, culture.Name, StringComparison.InvariantCultureIgnoreCase))
                {
                    tenant.Language = culture.Name;
                    changelng = true;
                }
            }

            var oldTimeZone = tenant.TimeZone;
            var timeZones = TimeZoneInfo.GetSystemTimeZones().ToList();
            if (timeZones.All(tz => tz.Id != "UTC"))
            {
                timeZones.Add(TimeZoneInfo.Utc);
            }
            tenant.TimeZone = timeZones.FirstOrDefault(tz => tz.Id == timeZoneID) ?? TimeZoneInfo.Utc;

            CoreContext.TenantManager.SaveTenant(tenant);

            if (!tenant.TimeZone.Id.Equals(oldTimeZone.Id) || changelng)
            {
                if (!tenant.TimeZone.Id.Equals(oldTimeZone.Id))
                {
                    MessageService.Send(HttpContext.Current.Request, MessageAction.TimeZoneSettingsUpdated);
                }
                if (changelng)
                {
                    MessageService.Send(HttpContext.Current.Request, MessageAction.LanguageSettingsUpdated);
                }
            }

            return Resource.SuccessfullySaveSettingsMessage;
        }

        /// <summary>
        /// Sets the default product page.
        /// </summary>
        /// <short>Set the default product page</short>
        /// <category>Common settings</category>
        /// <param name="defaultProductID">Default product ID</param>
        /// <returns>Operation result</returns>
        ///<visible>false</visible>
        [Update("defaultpage")]
        public string SaveDefaultPageSettings(string defaultProductID)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            new StudioDefaultPageSettings { DefaultProductID = new Guid(defaultProductID) }.Save();

            MessageService.Send(HttpContext.Current.Request, MessageAction.DefaultStartPageSettingsUpdated);

            return Resource.SuccessfullySaveSettingsMessage;
        }


        private static string GetProductName(Guid productId)
        {
            var product = WebItemManager.Instance[productId];
            return productId == Guid.Empty ? "All" : product != null ? product.Name : productId.ToString();
        }

        /// <summary>
        /// Refreshes the license.
        /// </summary>
        /// <short>Refresh the license</short>
        /// <category>Common settings</category>
        /// <returns>Boolean value: True - an operation is successful, False - an operation is unsuccessful</returns>
        /// <visible>false</visible>
        [Read("license/refresh")]
        public bool RefreshLicense()
        {
            if (!CoreContext.Configuration.Standalone) return false;
            LicenseReader.RefreshLicense();
            return true;
        }


        /// <summary>
        /// Returns a list of the custom navigation items.
        /// </summary>
        /// <short>Get the custom navigation items</short>
        /// <category>Custom navigation</category>
        /// <returns>List of the custom navigation items</returns>
        [Read("customnavigation/getall")]
        public List<CustomNavigationItem> GetCustomNavigationItems()
        {
            return CustomNavigationSettings.Load().Items;
        }

        /// <summary>
        /// Returns a custom navigation item sample.
        /// </summary>
        /// <short>Get a custom navigation item sample</short>
        /// <category>Custom navigation</category>
        /// <returns>Custom navigation item</returns>
        [Read("customnavigation/getsample")]
        public CustomNavigationItem GetCustomNavigationItemSample()
        {
            return CustomNavigationItem.GetSample();
        }

        /// <summary>
        /// Returns a custom navigation item by the ID specified in the request.
        /// </summary>
        /// <short>Get a custom navigation item by ID</short>
        /// <category>Custom navigation</category>
        /// <param name="id">Item ID</param>
        /// <returns>Custom navigation item</returns>
        [Read("customnavigation/get/{id}")]
        public CustomNavigationItem GetCustomNavigationItem(Guid id)
        {
            return CustomNavigationSettings.Load().Items.FirstOrDefault(item => item.Id == id);
        }

        /// <summary>
        /// Adds a custom navigation item with the parameters specified in the request.
        /// </summary>
        /// <short>Add a custom navigation item</short>
        /// <category>Custom navigation</category>
        /// <param name="item">Item parameters</param>
        /// <returns>Custom navigation item</returns>
        [Create("customnavigation/create")]
        public CustomNavigationItem CreateCustomNavigationItem(CustomNavigationItem item)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = CustomNavigationSettings.Load();

            var exist = false;

            foreach (var existItem in settings.Items)
            {
                if (existItem.Id != item.Id) continue;

                existItem.Label = item.Label;
                existItem.Url = item.Url;
                existItem.ShowInMenu = item.ShowInMenu;
                existItem.ShowOnHomePage = item.ShowOnHomePage;

                if (existItem.SmallImg != item.SmallImg)
                {
                    StorageHelper.DeleteLogo(existItem.SmallImg);
                    existItem.SmallImg = StorageHelper.SaveTmpLogo(item.SmallImg);
                }

                if (existItem.BigImg != item.BigImg)
                {
                    StorageHelper.DeleteLogo(existItem.BigImg);
                    existItem.BigImg = StorageHelper.SaveTmpLogo(item.BigImg);
                }

                exist = true;
                break;
            }

            if (!exist)
            {
                item.Id = Guid.NewGuid();
                item.SmallImg = StorageHelper.SaveTmpLogo(item.SmallImg);
                item.BigImg = StorageHelper.SaveTmpLogo(item.BigImg);

                settings.Items.Add(item);
            }

            settings.Save();

            MessageService.Send(HttpContext.Current.Request, MessageAction.CustomNavigationSettingsUpdated);

            return item;
        }

        /// <summary>
        /// Deletes a custom navigation item by the ID specified in the request.
        /// </summary>
        /// <short>Delete a custom navigation item</short>
        /// <category>Custom navigation</category>
        /// <param name="id">Item ID</param>
        [Delete("customnavigation/delete/{id}")]
        public void DeleteCustomNavigationItem(Guid id)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = CustomNavigationSettings.Load();

            var terget = settings.Items.FirstOrDefault(item => item.Id == id);

            if (terget == null) return;

            StorageHelper.DeleteLogo(terget.SmallImg);
            StorageHelper.DeleteLogo(terget.BigImg);

            settings.Items.Remove(terget);
            settings.Save();

            MessageService.Send(HttpContext.Current.Request, MessageAction.CustomNavigationSettingsUpdated);
        }

        /// <summary>
        /// Updates the email activation settings.
        /// </summary>
        /// <short>Update the email activation settings</short>
        /// <category>Common settings</category>
        /// <param name="show">Shows the email activation panel for the user</param>
        /// <returns>Updated email activation settings</returns>
        [Update("emailactivation")]
        public EmailActivationSettings UpdateEmailActivationSettings(bool show)
        {
            var settings = new EmailActivationSettings { Show = show };

            settings.SaveForCurrentUser();

            return settings;
        }

        /// <summary>
        /// Returns the licensor data.
        /// </summary>
        /// <short>Get the licensor data</short>
        /// <category>Common settings</category>
        /// <returns>List of company white label settings</returns>
        ///<visible>false</visible>
        [Read("companywhitelabel")]
        public List<CompanyWhiteLabelSettings> GetLicensorData()
        {
            var result = new List<CompanyWhiteLabelSettings>();

            var instance = CompanyWhiteLabelSettings.Instance;

            result.Add(instance);

            if (!instance.IsDefault && !instance.IsLicensor)
            {
                result.Add(instance.GetDefault() as CompanyWhiteLabelSettings);
            }

            return result;
        }

        /// <summary>
        /// Returns the space usage statistics of the web item with the ID specified in the request.
        /// </summary>
        /// <category>Statistics</category>
        /// <short>Get the space usage statistics</short>
        /// <param name="id">Web item ID</param>
        /// <returns>Space usage statistics of the web item</returns>
        [Read("statistics/spaceusage/{id}")]
        public List<UsageSpaceStatItemWrapper> GetSpaceUsageStatistics(Guid id)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var webtem = WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.All, ItemAvailableState.All)
                                       .FirstOrDefault(item =>
                                                       item != null &&
                                                       item.ID == id &&
                                                       item.Context != null &&
                                                       item.Context.SpaceUsageStatManager != null);

            if (webtem == null) return new List<UsageSpaceStatItemWrapper>();

            return webtem.Context.SpaceUsageStatManager.GetStatData()
                         .ConvertAll(it => new UsageSpaceStatItemWrapper
                         {
                             Name = it.Name.HtmlEncode(),
                             Icon = it.ImgUrl,
                             Disabled = it.Disabled,
                             Size = FileSizeComment.FilesSizeToString(it.SpaceUsage),
                             Url = it.Url
                         });
        }

        /// <summary>
        /// Returns the user visit statistics for the period specified in the request.
        /// </summary>
        /// <category>Statistics</category>
        /// <short>Get the visit statistics</short>
        /// <param name="fromDate">Start period date</param>
        /// <param name="toDate">End period date</param>
        /// <returns>List of point charts</returns>
        [Read("statistics/visit")]
        public List<ChartPointWrapper> GetVisitStatistics(ApiDateTime fromDate, ApiDateTime toDate)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var from = TenantUtil.DateTimeFromUtc(fromDate);
            var to = TenantUtil.DateTimeFromUtc(toDate);

            var points = new List<ChartPointWrapper>();

            if (from.CompareTo(to) >= 0) return points;

            for (var d = new DateTime(from.Ticks); d.Date.CompareTo(to.Date) <= 0; d = d.AddDays(1))
            {
                points.Add(new ChartPointWrapper
                {
                    DisplayDate = d.Date.ToShortDateString(),
                    Date = d.Date,
                    Hosts = 0,
                    Hits = 0
                });
            }

            var hits = StatisticManager.GetHitsByPeriod(TenantProvider.CurrentTenantID, from, to);
            var hosts = StatisticManager.GetHostsByPeriod(TenantProvider.CurrentTenantID, from, to);

            if (hits.Count == 0 || hosts.Count == 0) return points;

            hits.Sort((x, y) => x.VisitDate.CompareTo(y.VisitDate));
            hosts.Sort((x, y) => x.VisitDate.CompareTo(y.VisitDate));

            for (int i = 0, n = points.Count, hitsNum = 0, hostsNum = 0; i < n; i++)
            {
                while (hitsNum < hits.Count && points[i].Date.CompareTo(hits[hitsNum].VisitDate.Date) == 0)
                {
                    points[i].Hits += hits[hitsNum].VisitCount;
                    hitsNum++;
                }
                while (hostsNum < hosts.Count && points[i].Date.CompareTo(hosts[hostsNum].VisitDate.Date) == 0)
                {
                    points[i].Hosts++;
                    hostsNum++;
                }
            }

            return points;
        }

        /// <summary>
        /// Returns a list of all the portal storages.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Get storages</short>
        /// <returns>List of storages</returns>
        [Read("storage")]
        public List<StorageWrapper> GetAllStorages()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            TenantExtra.DemandControlPanelPermission();

            var current = StorageSettings.Load();
            var consumers = ConsumerFactory.GetAll<DataStoreConsumer>().ToList();
            return consumers.Select(consumer => new StorageWrapper(consumer, current)).ToList();
        }

        /// <summary>
        /// Returns the storage progress.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Get the storage progress</short>
        /// <returns>Storage progress</returns>
        [Read("storage/progress", checkPayment: false)]
        public double GetStorageProgress()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!CoreContext.Configuration.Standalone) return -1;

            using (var migrateClient = new ServiceClient())
            {
                return migrateClient.GetProgress(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            }
        }

        /// <summary>
        /// Updates a storage with the parameters specified in the request.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Update a storage</short>
        /// <param name="module">Storage name</param>
        /// <param name="props">New storage properties</param>
        /// <returns>Updated storage</returns>
        [Update("storage")]
        public StorageSettings UpdateStorage(string module, IEnumerable<ItemKeyValuePair<string, string>> props)
        {

            try
            {
                Log.Debug("UpdateStorage");
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
                if (!CoreContext.Configuration.Standalone) return null;

                TenantExtra.DemandControlPanelPermission();

                var consumer = ConsumerFactory.GetByName(module);
                if (!consumer.IsSet)
                    throw new ArgumentException("module");

                var settings = StorageSettings.Load();
                if (settings.Module == module) return settings;

                settings.Module = module;
                settings.Props = props.ToDictionary(r => r.Key, b => b.Value);

                StartMigrate(settings);
                return settings;
            }
            catch (Exception e)
            {
                Log.Error("UpdateStorage", e);
                throw;
            }
        }

        /// <summary>
        /// Resets the storage settings to the default parameters.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Reset the storage settings</short>
        [Delete("storage")]
        public void ResetStorageToDefault()
        {
            try
            {
                Log.Debug("ResetStorageToDefault");
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
                if (!CoreContext.Configuration.Standalone) return;

                TenantExtra.DemandControlPanelPermission();

                var settings = StorageSettings.Load();

                settings.Module = null;
                settings.Props = null;


                StartMigrate(settings);
            }
            catch (Exception e)
            {
                Log.Error("ResetStorageToDefault", e);
                throw;
            }
        }

        /// <summary>
        /// Returns a list of all the CDN storages.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Get the CDN storages</short>
        /// <returns>List of the CDN storages</returns>
        [Read("storage/cdn")]
        public List<StorageWrapper> GetAllCdnStorages()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (!CoreContext.Configuration.Standalone) return null;

            TenantExtra.DemandControlPanelPermission();

            var current = CdnStorageSettings.Load();
            var consumers = ConsumerFactory.GetAll<DataStoreConsumer>().Where(r => r.Cdn != null).ToList();
            return consumers.Select(consumer => new StorageWrapper(consumer, current)).ToList();
        }

        /// <summary>
        /// Updates the CDN storage with the parameters specified in the request.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Update the CDN storage</short>
        /// <param name="module">CDN storage name</param>
        /// <param name="props">New CDN storage properties</param>
        /// <returns>Updated CDN storage</returns>
        [Update("storage/cdn")]
        public CdnStorageSettings UpdateCdn(string module, IEnumerable<ItemKeyValuePair<string, string>> props)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (!CoreContext.Configuration.Standalone) return null;

            TenantExtra.DemandControlPanelPermission();

            var consumer = ConsumerFactory.GetByName(module);
            if (!consumer.IsSet)
                throw new ArgumentException("module");

            var settings = CdnStorageSettings.Load();
            if (settings.Module == module) return settings;

            settings.Module = module;
            settings.Props = props.ToDictionary(r => r.Key, b => b.Value);

            try
            {
                using (var migrateClient = new ServiceClient())
                {
                    migrateClient.UploadCdn(CoreContext.TenantManager.GetCurrentTenant().TenantId, "/", HttpContext.Current.Server.MapPath("~/"), settings);
                }

                BundleTable.Bundles.Clear();
            }
            catch (Exception e)
            {
                Log.Error("UpdateCdn", e);
                throw;
            }

            return settings;
        }

        /// <summary>
        /// Resets the CDN storage settings to the default parameters.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Reset the CDN storage settings</short>
        [Delete("storage/cdn")]
        public void ResetCdnToDefault()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (!CoreContext.Configuration.Standalone) return;

            TenantExtra.DemandControlPanelPermission();

            CdnStorageSettings.Load().Clear();
        }

        /// <summary>
        /// Returns a list of all the backup storages.
        /// </summary>
        /// <category>Storage</category>
        /// <short>Get the backup storages</short>
        /// <returns>List of the backup storages</returns>
        [Read("storage/backup")]
        public List<StorageWrapper> GetAllBackupStorages()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (CoreContext.Configuration.Standalone)
            {
                TenantExtra.DemandControlPanelPermission();
            }

            var schedule = new BackupAjaxHandler().GetSchedule();
            var current = new StorageSettings();

            if (schedule != null && schedule.StorageType == Core.Common.Contracts.BackupStorageType.ThirdPartyConsumer)
            {
                current = new StorageSettings
                {
                    Module = schedule.StorageParams["module"],
                    Props = schedule.StorageParams.Where(r => r.Key != "module").ToDictionary(r => r.Key, r => r.Value)
                };
            }

            var consumers = ConsumerFactory.GetAll<DataStoreConsumer>().ToList();
            return consumers.Select(consumer => new StorageWrapper(consumer, current)).ToList();
        }

        private void StartMigrate(StorageSettings settings)
        {
            using (var migrateClient = new ServiceClient())
            {
                migrateClient.Migrate(CoreContext.TenantManager.GetCurrentTenant().TenantId, settings);
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant();
            tenant.SetStatus(TenantStatus.Migrating);
            CoreContext.TenantManager.SaveTenant(tenant);
        }


        /// <summary>
        /// Returns the socket settings.
        /// </summary>
        /// <category>Common settings</category>
        /// <short>Get the socket settings</short>
        /// <returns>Socket settings</returns>
        [Read("socket")]
        public object GetSocketSettings()
        {
            var hubUrl = ConfigurationManagerExtension.AppSettings["web.hub"] ?? string.Empty;
            if (hubUrl != string.Empty)
            {
                if (!hubUrl.EndsWith("/"))
                {
                    hubUrl += "/";
                }
            }

            return new { Url = hubUrl };
        }

        /// <summary>
        /// Returns the tenant control panel settings.
        /// </summary>
        /// <category>Common settings</category>
        /// <short>Get the tenant control panel settings</short>
        /// <returns>Tenant control panel settings</returns>
        ///<visible>false</visible>
        [Read("controlpanel")]
        public TenantControlPanelSettings GetTenantControlPanelSettings()
        {
            return TenantControlPanelSettings.Instance;
        }

        /// <summary>
        /// Saves the company white label settings specified in the request.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Save the white label settings</short>
        /// <param name="settings">White label settings</param>
        ///<visible>false</visible>
        [Create("rebranding/company")]
        public void SaveCompanyWhiteLabelSettings(CompanyWhiteLabelSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            DemandRebrandingPermission();

            settings.IsLicensor = false; //TODO: CoreContext.TenantManager.GetTenantQuota(TenantProvider.CurrentTenantID).Branding && settings.IsLicensor

            settings.SaveForDefaultTenant();
        }

        /// <summary>
        /// Returns the company white label settings.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Get the white label settings</short>
        /// <returns>Company white label settings</returns>
        ///<visible>false</visible>
        [Read("rebranding/company")]
        public CompanyWhiteLabelSettings GetCompanyWhiteLabelSettings()
        {
            return CompanyWhiteLabelSettings.Instance;
        }

        /// <summary>
        /// Deletes the company white label settings.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Delete the white label settings</short>
        /// <returns>Default company white label settings</returns>
        ///<visible>false</visible>
        [Delete("rebranding/company")]
        public CompanyWhiteLabelSettings DeleteCompanyWhiteLabelSettings()
        {
            DemandRebrandingPermission();

            var defaultSettings = (CompanyWhiteLabelSettings)CompanyWhiteLabelSettings.Instance.GetDefault();

            defaultSettings.SaveForDefaultTenant();

            return defaultSettings;
        }

        /// <summary>
        /// Saves the additional white label settings specified in the request.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Save the additional white label settings</short>
        /// <param name="settings">Additional white label settings</param>
        ///<visible>false</visible>
        [Create("rebranding/additional")]
        public void SaveAdditionalWhiteLabelSettings(AdditionalWhiteLabelSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            DemandRebrandingPermission();

            settings.SaveForDefaultTenant();
        }

        /// <summary>
        /// Returns the additional white label settings.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Get the additional white label settings</short>
        /// <returns>Additional white label settings</returns>
        ///<visible>false</visible>
        [Read("rebranding/additional")]
        public AdditionalWhiteLabelSettings GetAdditionalWhiteLabelSettings()
        {
            return AdditionalWhiteLabelSettings.Instance;
        }

        /// <summary>
        /// Deletes the additional white label settings.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Delete the additional white label settings</short>
        /// <returns>Default white label settings</returns>
        ///<visible>false</visible>
        [Delete("rebranding/additional")]
        public AdditionalWhiteLabelSettings DeleteAdditionalWhiteLabelSettings()
        {
            DemandRebrandingPermission();

            var defaultSettings = (AdditionalWhiteLabelSettings)AdditionalWhiteLabelSettings.Instance.GetDefault();

            defaultSettings.SaveForDefaultTenant();

            return defaultSettings;
        }

        /// <summary>
        /// Saves the mail white label settings specified in the request.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Save the mail white label settings</short>
        /// <param name="settings">Mail white label settings</param>
        ///<visible>false</visible>
        [Create("rebranding/mail")]
        public void SaveMailWhiteLabelSettings(MailWhiteLabelSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");

            DemandRebrandingPermission();

            settings.SaveForDefaultTenant();
        }

        /// <summary>
        /// Updates the mail white label settings with a paramater specified in the request.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Update the mail white label settings</short>
        /// <param name="footerEnabled">Specified if the footer will be enabled or not</param>
        ///<visible>false</visible>
        [Update("rebranding/mail")]
        public void UpdateMailWhiteLabelSettings(bool footerEnabled)
        {
            DemandRebrandingPermission();

            var settings = MailWhiteLabelSettings.Instance;

            settings.FooterEnabled = footerEnabled;

            settings.SaveForDefaultTenant();
        }

        /// <summary>
        /// Returns the mail white label settings.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Get the mail white label settings</short>
        /// <returns>Mail white label settings</returns>
        ///<visible>false</visible>
        [Read("rebranding/mail")]
        public MailWhiteLabelSettings GetMailWhiteLabelSettings()
        {
            return MailWhiteLabelSettings.Instance;
        }

        /// <summary>
        /// Deletes the mail white label settings.
        /// </summary>
        /// <category>Rebranding</category>
        /// <short>Delete the mail white label settings</short>
        /// <returns>Default mail white label settings</returns>
        ///<visible>false</visible>
        [Delete("rebranding/mail")]
        public MailWhiteLabelSettings DeleteMailWhiteLabelSettings()
        {
            DemandRebrandingPermission();

            var defaultSettings = (MailWhiteLabelSettings)MailWhiteLabelSettings.Instance.GetDefault();

            defaultSettings.SaveForDefaultTenant();

            return defaultSettings;
        }

        private static void DemandWhiteLabelPermission()
        {
            if (!CoreContext.Configuration.Standalone
                && (!TenantLogoManager.WhiteLabelEnabled || !TenantLogoManager.WhiteLabelPaid))
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "WhiteLabel");
            }
        }

        private static void DemandRebrandingPermission()
        {
            TenantExtra.DemandControlPanelPermission();

            if (!CoreContext.Configuration.Standalone)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "SSBranding");
            }

            if (CoreContext.Configuration.CustomMode)
            {
                throw new SecurityException();
            }
        }

        /// <summary>
        /// Returns the storage encryption settings.
        /// </summary>
        /// <short>Get the storage encryption settings</short>
        /// <category>Encryption</category>
        /// <returns>Storage encryption settings</returns>
        /// <visible>false</visible>
        [Read("encryption/settings")]
        public EncryptionSettings GetStorageEncryptionSettings()
        {
            try
            {
                if (CoreContext.Configuration.CustomMode)
                {
                    return null;
                }

                if (!SetupInfo.IsVisibleSettings<EncryptionSettings>())
                {
                    throw new NotSupportedException();
                }

                if (!CoreContext.Configuration.Standalone)
                {
                    throw new NotSupportedException(Resource.ErrorServerEditionMethod);
                }

                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                TenantExtra.DemandControlPanelPermission();

                var settings = EncryptionSettings.Load();

                settings.Password = string.Empty; // Don't show password

                return settings;
            }
            catch (Exception e)
            {
                Log.Error("GetStorageEncryptionSettings", e);
                return null;
            }
        }

        /// <summary>
        /// Returns the storage encryption progress.
        /// </summary>
        /// <short>Get the storage encryption progress</short>
        /// <category>Encryption</category>
        /// <returns>Storage encryption progress</returns>
        /// <visible>false</visible>
        [Read("encryption/progress", checkPayment: false)]
        public double GetStorageEncryptionProgress()
        {
            if (CoreContext.Configuration.CustomMode)
            {
                return -1;
            }

            if (!SetupInfo.IsVisibleSettings<EncryptionSettings>())
            {
                throw new NotSupportedException();
            }

            if (!CoreContext.Configuration.Standalone)
            {
                throw new NotSupportedException(Resource.ErrorServerEditionMethod);
            }

            using (var encryptionClient = new EncryptionServiceClient())
            {
                return encryptionClient.GetProgress();
            }
        }

        public static readonly object Locker = new object();

        /// <summary>
        /// Starts the storage encryption process.
        /// </summary>
        /// <short>Start the storage encryption process</short>
        /// <category>Encryption</category>
        /// <param name="notifyUsers">Specifies if the users will be notified about the encryption process or not</param>
        /// <visible>false</visible>
        [Create("encryption/start")]
        public void StartStorageEncryption(bool notifyUsers)
        {
            if (CoreContext.Configuration.CustomMode)
            {
                return;
            }

            lock (Locker)
            {
                var activeTenants = CoreContext.TenantManager.GetTenants();

                if (activeTenants.Any())
                {
                    StartEncryption(notifyUsers);
                }
            }
        }

        private void StartEncryption(bool notifyUsers)
        {
            if (!SetupInfo.IsVisibleSettings<EncryptionSettings>())
            {
                throw new NotSupportedException();
            }

            if (!CoreContext.Configuration.Standalone)
            {
                throw new NotSupportedException(Resource.ErrorServerEditionMethod);
            }

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            TenantExtra.DemandControlPanelPermission();

            var storages = GetAllStorages();

            if (storages.Any(s => s.Current))
            {
                throw new NotSupportedException(Resource.ErrorDefaultStorageMethod);
            }

            var cdnStorages = GetAllCdnStorages();

            if (cdnStorages.Any(s => s.Current))
            {
                throw new NotSupportedException(Resource.ErrorDefaultStorageMethod);
            }

            var tenants = CoreContext.TenantManager.GetTenants();

            using (var service = new BackupServiceClient())
            {
                foreach (var tenant in tenants)
                {
                    var progress = service.GetBackupProgress(tenant.TenantId);
                    if (progress != null && progress.IsCompleted == false)
                    {
                        throw new Exception(Resource.ErrorWaitForBackupProcessComplete);
                    }
                }

                foreach (var tenant in tenants)
                {
                    service.DeleteSchedule(tenant.TenantId);
                }
            }

            var settings = EncryptionSettings.Load();

            settings.NotifyUsers = notifyUsers;

            if (settings.Status == EncryprtionStatus.Decrypted)
            {
                settings.Status = EncryprtionStatus.EncryptionStarted;
                settings.Password = EncryptionSettings.GeneratePassword(32, 16);
            }
            else if (settings.Status == EncryprtionStatus.Encrypted)
            {
                settings.Status = EncryprtionStatus.DecryptionStarted;
            }

            MessageService.Send(HttpContext.Current.Request, settings.Status == EncryprtionStatus.EncryptionStarted ? MessageAction.StartStorageEncryption : MessageAction.StartStorageDecryption);

            var serverRootPath = CommonLinkUtility.GetFullAbsolutePath("~").TrimEnd('/');

            foreach (var tenant in tenants)
            {
                CoreContext.TenantManager.SetCurrentTenant(tenant);

                if (notifyUsers)
                {
                    if (settings.Status == EncryprtionStatus.EncryptionStarted)
                    {
                        StudioNotifyService.Instance.SendStorageEncryptionStart(serverRootPath);
                    }
                    else
                    {
                        StudioNotifyService.Instance.SendStorageDecryptionStart(serverRootPath);
                    }
                }

                tenant.SetStatus(TenantStatus.Encryption);
                CoreContext.TenantManager.SaveTenant(tenant);
            }

            settings.Save();

            using (var encryptionClient = new EncryptionServiceClient())
            {
                encryptionClient.Start(settings, serverRootPath);
            }
        }
    }
}