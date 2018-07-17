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


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Employee;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Threading;
using ASC.Core;
using ASC.Core.Billing;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WhiteLabel;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Quota;
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.Utility;
using log4net;
using Resources;
using SecurityContext = ASC.Core.SecurityContext;
using StorageHelper = ASC.Web.Studio.UserControls.CustomNavigation.StorageHelper;


namespace ASC.Api.Settings
{
    ///<summary>
    /// Portal settings
    ///</summary>
    public partial class SettingsApi : IApiEntryPoint
    {
        private const int ONE_THREAD = 1;
        private static readonly DistributedTaskQueue ldapTasks = new DistributedTaskQueue("ldapOperations");
        private static readonly DistributedTaskQueue quotaTasks = new DistributedTaskQueue("quotaOperations", ONE_THREAD);

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

        ///<summary>
        /// Returns the list of all available portal settings with the current values for each one
        ///</summary>
        ///<short>
        /// Portal settings
        ///</short>
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
        /// Returns space usage quota for portal with the space usage of each module
        ///</summary>
        ///<short>
        /// Space usage
        ///</short>
        ///<returns>Space usage and limits for upload</returns>
        [Read("quota")]
        public QuotaWrapper GetQuotaUsed()
        {
            var diskQuota = CoreContext.TenantManager.GetTenantQuota(CoreContext.TenantManager.GetCurrentTenant().TenantId);
            return new QuotaWrapper(diskQuota, GetQuotaRows());
        }

        ///<summary>
        /// Start Recalculate Quota Task
        ///</summary>
        ///<short>
        /// Recalculate Quota 
        ///</short>
        ///<returns></returns>
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
        /// Check Recalculate Quota Task
        ///</summary>
        ///<short>
        /// Check Recalculate Quota Task
        ///</short>
        ///<returns>Check Recalculate Quota Task Status</returns>
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
        /// Get build version
        /// </summary>
        /// <visible>false</visible>
        /// <returns>Current onlyoffice, editor, mailserver versions</returns>
        [Read("version/build")]
        public BuildVersion GetBuildVersions()
        {
            return BuildVersion.GetCurrentBuildVersion();
        }

        /// <summary>
        /// Get list of availibe portal versions including current version
        /// </summary>
        /// <short>
        /// Portal versions
        /// </short>
        /// <visible>false</visible>
        /// <returns>List of availibe portal versions including current version</returns>
        [Read("version")]
        public TenantVersionWrapper GetVersions()
        {
            return new TenantVersionWrapper(CoreContext.TenantManager.GetCurrentTenant().Version, CoreContext.TenantManager.GetTenantVersions());
        }

        /// <summary>
        /// Set current portal version to the one with the ID specified in the request
        /// </summary>
        /// <short>
        /// Change portal version
        /// </short>
        /// <param name="versionId">Version ID</param>
        /// <visible>false</visible>
        /// <returns>List of availibe portal versions including current version</returns>
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
        /// Returns security settings about product, module or addons
        /// </summary>
        /// <short>
        /// Get security settings
        /// </short>
        /// <param name="ids">Module ID list</param>
        /// <returns></returns>
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
        /// Set security settings for product, module or addons
        /// </summary>
        /// <short>
        /// Set security settings
        /// </short>
        /// <param name="id">Module ID</param>
        /// <param name="enabled">Enabled</param>
        /// <param name="subjects">User (Group) ID list</param>
        [Update("security")]
        public IEnumerable<SecurityWrapper> SetWebItemSecurity(string id, bool enabled, IEnumerable<Guid> subjects)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            WebItemSecurity.SetSecurity(id, enabled, subjects != null ? subjects.ToArray() : null);
            var securityInfo = GetWebItemSecurityInfo(new List<string> {id});

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
        /// Set access to products, modules or addons
        /// </summary>
        /// <short>
        /// Set access
        /// </short>
        /// <param name="items"></param>
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
                    if (webItem != null)
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
                    (defaultPageSettings.GetDefault() as StudioDefaultPageSettings).Save();
                }

                WebItemSecurity.SetSecurity(item.Key, item.Value, subjects);
            }

            MessageService.Send(Request, MessageAction.ProductsListUpdated);

            return GetWebItemSecurityInfo(itemList.Keys.ToList());
        }

        [Read("security/administrator/{productid}")]
        public IEnumerable<EmployeeWraper> GetProductAdministrators(Guid productid)
        {
            return WebItemSecurity.GetProductAdministrators(productid)
                                  .Select(EmployeeWraper.Get)
                                  .ToList();
        }

        [Read("security/administrator")]
        public object IsProductAdministrator(Guid productid, Guid userid)
        {
            var result = WebItemSecurity.IsProductAdministrator(productid, userid);
            return new {ProductId = productid, UserId = userid, Administrator = result,};
        }

        [Update("security/administrator")]
        public object SetProductAdministrator(Guid productid, Guid userid, bool administrator)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

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

            return new {ProductId = productid, UserId = userid, Administrator = administrator};
        }

        private static IList<TenantQuotaRow> GetQuotaRows()
        {
            return CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(CoreContext.TenantManager.GetCurrentTenant().TenantId))
                              .Where(r => !string.IsNullOrEmpty(r.Tag) && new Guid(r.Tag) != Guid.Empty).ToList();
        }

        /// <summary>
        /// Get portal logo image URL
        /// </summary>
        /// <short>
        /// Portal logo
        /// </short>
        /// <returns>Portal logo image URL</returns>
        [Read("logo")]
        public string GetLogo()
        {
            return TenantInfoSettings.Load().GetAbsoluteCompanyLogoPath();
        }


        ///<visible>false</visible>
        [Create("whitelabel/save")]
        public void SaveWhiteLabelSettings(string logoText, IEnumerable<ItemKeyValuePair<int, string>> logo)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!TenantLogoManager.WhiteLabelEnabled || !TenantLogoManager.WhiteLabelPaid)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "WhiteLabel");
            }

            var tenantId = TenantProvider.CurrentTenantID;
            var _tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();

            if (logo != null)
            {
                var logoDict = new Dictionary<int, string>();
                logo.ToList().ForEach(n => logoDict.Add(n.Key, n.Value));

                _tenantWhiteLabelSettings.SetLogo(logoDict);
            }

            _tenantWhiteLabelSettings.LogoText = logoText;
            _tenantWhiteLabelSettings.Save(tenantId);

        }


        ///<visible>false</visible>
        [Create("whitelabel/savefromfiles")]
        public void SaveWhiteLabelSettingsFromFiles(IEnumerable<System.Web.HttpPostedFileBase> attachments)
        {
            if (attachments != null && attachments.Any())
            {
                var tenantId = TenantProvider.CurrentTenantID;
                var _tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();

                foreach (var f in attachments)
                {
                    var parts = f.FileName.Split('.');

                    WhiteLabelLogoTypeEnum logoType = (WhiteLabelLogoTypeEnum)(Convert.ToInt32(parts[0]));
                    string fileExt = parts[1];
                    _tenantWhiteLabelSettings.SetLogoFromStream(logoType, fileExt, f.InputStream);
                }
                _tenantWhiteLabelSettings.Save(tenantId);
            }
            else
            {
                throw new InvalidOperationException("No input files");
            }
        }


        ///<visible>false</visible>
        [Read("whitelabel/sizes")]
        public object GetWhiteLabelSizes()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!TenantLogoManager.WhiteLabelEnabled)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "WhiteLabel");
            }

            return
            new[]
            {
                new {type = (int)WhiteLabelLogoTypeEnum.LightSmall, name = WhiteLabelLogoTypeEnum.LightSmall.ToString(), height = TenantWhiteLabelSettings.logoLightSmallSize.Height, width = TenantWhiteLabelSettings.logoLightSmallSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.Dark, name = WhiteLabelLogoTypeEnum.Dark.ToString(), height = TenantWhiteLabelSettings.logoDarkSize.Height, width = TenantWhiteLabelSettings.logoDarkSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.Favicon, name = WhiteLabelLogoTypeEnum.Favicon.ToString(), height = TenantWhiteLabelSettings.logoFaviconSize.Height, width = TenantWhiteLabelSettings.logoFaviconSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.DocsEditor, name = WhiteLabelLogoTypeEnum.DocsEditor.ToString(), height = TenantWhiteLabelSettings.logoDocsEditorSize.Height, width = TenantWhiteLabelSettings.logoDocsEditorSize.Width}
            };
        }



        ///<visible>false</visible>
        [Read("whitelabel/logos")]
        public Dictionary<int, string> GetWhiteLabelLogos(bool retina)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!TenantLogoManager.WhiteLabelEnabled)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "WhiteLabel");
            }

            var _tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();


            var result = new Dictionary<int, string>();

            result.Add((int)WhiteLabelLogoTypeEnum.LightSmall, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.LightSmall, !retina)));
            result.Add((int)WhiteLabelLogoTypeEnum.Dark, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Dark, !retina)));
            result.Add((int)WhiteLabelLogoTypeEnum.Favicon, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, !retina)));
            result.Add((int)WhiteLabelLogoTypeEnum.DocsEditor, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, !retina)));

            return result;
        }

        ///<visible>false</visible>
        [Read("whitelabel/logotext")]
        public string GetWhiteLabelLogoText()
        {
            if (!TenantLogoManager.WhiteLabelEnabled)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "WhiteLabel");
            }

            var whiteLabelSettings = TenantWhiteLabelSettings.Load();

            return whiteLabelSettings.LogoText ?? TenantWhiteLabelSettings.DefaultLogoText;
        }


        ///<visible>false</visible>
        [Update("whitelabel/restore")]
        public void RestoreWhiteLabelOptions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!TenantLogoManager.WhiteLabelEnabled || !TenantLogoManager.WhiteLabelPaid)
            {
                throw new BillingException(Resource.ErrorNotAllowedOption, "WhiteLabel");
            }

            var _tenantWhiteLabelSettings = TenantWhiteLabelSettings.Load();
            _tenantWhiteLabelSettings.RestoreDefault();

            var _tenantInfoSettings = TenantInfoSettings.Load();
            _tenantInfoSettings.RestoreDefaultLogo();
            _tenantInfoSettings.Save();
        }

        /// <summary>
        /// Get portal ip restrictions
        /// </summary>
        /// <returns></returns>
        [Read("/iprestrictions")]
        public IEnumerable<IPRestriction> GetIpRestrictions()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return IPRestrictionsService.Get(CurrentTenant);
        }

        /// <summary>
        /// save new portal ip restrictions
        /// </summary>
        /// <param name="ips">ip restrictions</param>
        /// <returns></returns>
        [Update("iprestrictions")]
        public IEnumerable<string> SaveIpRestrictions(IEnumerable<string> ips)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            return IPRestrictionsService.Save(ips, CurrentTenant);
        }

        /// <summary>
        /// update ip restrictions settings
        /// </summary>
        /// <param name="enable">enable ip restrictions settings</param>
        /// <returns></returns>
        [Update("iprestrictions/settings")]
        public IPRestrictionsSettings UpdateIpRestrictionsSettings(bool enable)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = new IPRestrictionsSettings {Enable = enable};
            settings.Save();

            return settings;
        }

        /// <summary>
        /// update tips settings
        /// </summary>
        /// <param name="show">show tips for user</param>
        /// <returns></returns>
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
                    LogManager.GetLogger(typeof(SettingsApi)).Error(e.Message, e);
                }
            }

            return settings;
        }

        /// <summary>
        /// change tips&amp;tricks subscription
        /// </summary>
        /// <returns>subscription state</returns>
        [Update("tips/change/subscription")]
        public bool UpdateTipsSubscription()
        {
            var isSubscribe = StudioNotifyService.Instance.IsSubscribeToPeriodicNotify(SecurityContext.CurrentAccount.ID);

            StudioNotifyService.Instance.SubscribeToPeriodicNotify(SecurityContext.CurrentAccount.ID, !isSubscribe);

            return !isSubscribe;
        }

        /// <summary>
        /// Complete Wizard
        /// </summary>
        /// <returns>WizardSettings</returns>
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
        /// Update two-factor authentication settings
        /// </summary>
        /// <param name="enable">Enable two-factor authentication</param>
        /// <returns>Setting value</returns>
        [Update("sms")]
        public bool SmsSettings(bool enable)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            if (!StudioSmsNotificationSettings.IsVisibleSettings)
            {
                throw new Exception(Resource.SmsNotAvailable);
            }

            if (enable && !SmsProviderManager.Enabled())
                throw new MethodAccessException();

            StudioSmsNotificationSettings.Enable = enable;

            MessageService.Send(Request, MessageAction.TwoFactorAuthenticationSettingsUpdated);

            return StudioSmsNotificationSettings.Enable;
        }

        ///<visible>false</visible>
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

        ///<visible>false</visible>
        [Update("colortheme")]
        public void SaveColorTheme(string theme)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            ColorThemesSettings.SaveColorTheme(theme);
            MessageService.Send(HttpContext.Current.Request, MessageAction.ColorThemeChanged);
        }

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
            var product = WebItemManager.Instance[productId] as IProduct;
            return productId == Guid.Empty ? "All" : product != null ? product.Name : productId.ToString();
        }

        /// <summary>
        /// Refresh license
        /// </summary>
        /// <visible>false</visible>
        [Read("license/refresh")]
        public bool RefreshLicense()
        {
            if (!CoreContext.Configuration.Standalone) return false;
            LicenseReader.RefreshLicense();
            return true;
        }


        /// <summary>
        /// Get Custom Navigation Items
        /// </summary>
        /// <returns>CustomNavigationItem List</returns>
        [Read("customnavigation/getall")]
        public List<CustomNavigationItem> GetCustomNavigationItems()
        {
            return CustomNavigationSettings.Load().Items;
        }

        /// <summary>
        /// Get Custom Navigation Items Sample
        /// </summary>
        /// <returns>CustomNavigationItem Sample</returns>
        [Read("customnavigation/getsample")]
        public CustomNavigationItem GetCustomNavigationItemSample()
        {
            return CustomNavigationItem.GetSample();
        }

        /// <summary>
        /// Get Custom Navigation Item by Id
        /// </summary>
        /// <param name="id">Item id</param>
        /// <returns>CustomNavigationItem</returns>
        [Read("customnavigation/get/{id}")]
        public CustomNavigationItem GetCustomNavigationItem(Guid id)
        {
            return CustomNavigationSettings.Load().Items.FirstOrDefault(item => item.Id == id);
        }

        /// <summary>
        /// Add Custom Navigation Item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns>CustomNavigationItem</returns>
        [Create("customnavigation/create")]
        public CustomNavigationItem CreateCustomNavigationItem(CustomNavigationItem item)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var settings = CustomNavigationSettings.Load();

            var exist = false;

            foreach (var existItem in settings.Items)
            {
                if(existItem.Id != item.Id) continue;

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
        /// Delete Custom Navigation Item by Id
        /// </summary>
        /// <param name="id">Item id</param>
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
        /// update email activation settings
        /// </summary>
        /// <param name="show">show email activation panel for user</param>
        /// <returns></returns>
        [Update("emailactivation")]
        public EmailActivationSettings UpdateEmailActivationSettings(bool show)
        {
            var settings = new EmailActivationSettings { Show = show };

            settings.SaveForCurrentUser();

            return settings;
        }
    }
}