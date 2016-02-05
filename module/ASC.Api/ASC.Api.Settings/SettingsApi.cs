/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.Employee;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Common.Threading.Progress;
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
using ASC.Web.Studio.Core.SMS;
using ASC.Web.Studio.UserControls.FirstTime;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Configuration;


namespace ASC.Api.Settings
{
    ///<summary>
    /// Portal settings
    ///</summary>
    public partial class SettingsApi : IApiEntryPoint
    {
        private const int ONE_THREAD = 1;
        private static ProgressQueue ldapTasks = new ProgressQueue(ONE_THREAD, TimeSpan.FromMinutes(15), true);

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

        private static ProgressQueue LdapTasks
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

            foreach (var item in itemList)
            {
                Guid[] subjects = null;

                if (item.Value)
                {
                    var webItem = WebItemManager.Instance[new Guid(item.Key)] as IProduct;
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
                MessageService.Send(Request, messageAction, admin.DisplayUserName(false));
            }
            else
            {
                var messageAction = administrator ? MessageAction.ProductAddedAdministrator : MessageAction.ProductDeletedAdministrator;
                MessageService.Send(Request, messageAction, GetProductName(productid), admin.DisplayUserName(false));
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
            return SettingsManager.Instance.LoadSettings<TenantInfoSettings>(CoreContext.TenantManager.GetCurrentTenant().TenantId).GetAbsoluteCompanyLogoPath();
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

            var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);

            if (logo != null)
            {
                var logoDict = new Dictionary<int, string>();
                logo.ToList().ForEach(n => logoDict.Add(n.Key, n.Value));

                _tenantWhiteLabelSettings.SetLogo(logoDict);
            }

            _tenantWhiteLabelSettings.LogoText = logoText;
            _tenantWhiteLabelSettings.Save();

        }


        ///<visible>false</visible>
        [Create("whitelabel/savefromfiles")]
        public void SaveWhiteLabelSettingsFromFiles(IEnumerable<System.Web.HttpPostedFileBase> files)
        {
            if (files != null && files.Any())
            {
                var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);

                foreach (var f in files)
                {
                    var parts = f.FileName.Split('.');

                    WhiteLabelLogoTypeEnum logoType = (WhiteLabelLogoTypeEnum)(Convert.ToInt32(parts[0]));
                    string fileExt = parts[1];
                    _tenantWhiteLabelSettings.SetLogoFromStream(logoType, fileExt, f.InputStream);
                }
                _tenantWhiteLabelSettings.Save();
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
                new {type = (int)WhiteLabelLogoTypeEnum.Light, name = WhiteLabelLogoTypeEnum.Light.ToString(), height = TenantWhiteLabelSettings.logoLightSize.Height, width = TenantWhiteLabelSettings.logoLightSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.LightSmall, name = WhiteLabelLogoTypeEnum.LightSmall.ToString(), height = TenantWhiteLabelSettings.logoLightSmallSize.Height, width = TenantWhiteLabelSettings.logoLightSmallSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.Dark, name = WhiteLabelLogoTypeEnum.Dark.ToString(), height = TenantWhiteLabelSettings.logoDarkSize.Height, width = TenantWhiteLabelSettings.logoDarkSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.Favicon, name = WhiteLabelLogoTypeEnum.Favicon.ToString(), height = TenantWhiteLabelSettings.logoFaviconSize.Height, width = TenantWhiteLabelSettings.logoFaviconSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.DocsEditor, name = WhiteLabelLogoTypeEnum.DocsEditor.ToString(), height = TenantWhiteLabelSettings.logoDocsEditorSize.Height, width = TenantWhiteLabelSettings.logoDocsEditorSize.Width},
                new {type = (int)WhiteLabelLogoTypeEnum.DocsEditorEmbedded, name = WhiteLabelLogoTypeEnum.DocsEditorEmbedded.ToString(), height = TenantWhiteLabelSettings.logoDocsEditorEmbeddedSize.Height, width = TenantWhiteLabelSettings.logoDocsEditorEmbeddedSize.Width}
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

            var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);


            var result = new Dictionary<int, string>();

            result.Add((int)WhiteLabelLogoTypeEnum.Light, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Light, !retina)));
            result.Add((int)WhiteLabelLogoTypeEnum.LightSmall, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.LightSmall, !retina)));


            var logoDarkPath = _tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Dark, !retina);
            var defaultDarkLogoPath = TenantWhiteLabelSettings.GetAbsoluteDefaultLogoPath(WhiteLabelLogoTypeEnum.Dark, !retina);

            if (String.Equals(logoDarkPath, defaultDarkLogoPath, StringComparison.OrdinalIgnoreCase))
            {
                var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
                logoDarkPath = _tenantInfoSettings.GetAbsoluteCompanyLogoPath();
            }

            result.Add((int)WhiteLabelLogoTypeEnum.Dark, CommonLinkUtility.GetFullAbsolutePath(logoDarkPath));

            result.Add((int)WhiteLabelLogoTypeEnum.Favicon, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.Favicon, !retina)));
            result.Add((int)WhiteLabelLogoTypeEnum.DocsEditor, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditor, !retina)));
            result.Add((int)WhiteLabelLogoTypeEnum.DocsEditorEmbedded, CommonLinkUtility.GetFullAbsolutePath(_tenantWhiteLabelSettings.GetAbsoluteLogoPath(WhiteLabelLogoTypeEnum.DocsEditorEmbedded, !retina)));

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

            var whiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);

            return whiteLabelSettings.LogoText != null ? whiteLabelSettings.LogoText : TenantWhiteLabelSettings.DefaultLogo;
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

            var _tenantWhiteLabelSettings = SettingsManager.Instance.LoadSettings<TenantWhiteLabelSettings>(TenantProvider.CurrentTenantID);
            _tenantWhiteLabelSettings.RestoreDefault();

            var _tenantInfoSettings = SettingsManager.Instance.LoadSettings<TenantInfoSettings>(TenantProvider.CurrentTenantID);
            _tenantInfoSettings.RestoreDefaultLogo();
            SettingsManager.Instance.SaveSettings(_tenantInfoSettings, TenantProvider.CurrentTenantID);
        }

        ///<visible>false</visible>
        [Read("webconfig/basedomain")]
        public string GetWebConfigBaseDomain()
        {
            if (String.IsNullOrEmpty(SetupInfo.ControlPanelUrl)
                || CoreContext.Configuration.Personal
                || !CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsAdmin())
                    throw new Exception(Resource.ErrorAccessDenied);

            return CoreContext.Configuration.BaseDomain;
        }

        ///<visible>false</visible>
        [Update("webconfig/basedomain")]
        public string SetWebConfigBaseDomain(string domain)
        {
            if (String.IsNullOrEmpty(SetupInfo.ControlPanelUrl)
                || CoreContext.Configuration.Personal
                || !CoreContext.Configuration.Standalone)
                    throw new Exception(Resource.ErrorAccessDenied);

            ASC.Core.SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            CoreContext.Configuration.BaseDomain = domain;
            return CoreContext.Configuration.BaseDomain;
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
            SettingsManager.Instance.SaveSettings(settings, CurrentTenant);

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
            SettingsManager.Instance.SaveSettingsFor(settings, CurrentUser);

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
        /// Complete Wizard
        /// </summary>
        /// <returns>WizardSettings</returns>
        /// <visible>false</visible>
        [Update("wizard/complete")]
        public WizardSettings CompleteWizard()
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;
            var settings = SettingsManager.Instance.LoadSettings<WizardSettings>(tenantId);

            if (settings.Completed)
                return settings;

            settings.Completed = true;
            SettingsManager.Instance.SaveSettings(settings, tenantId);

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

            if (!StudioSmsNotificationSettings.IsVisibleSettings || CoreContext.PaymentManager.GetApprovedPartner() != null)
            {
                throw new Exception(Resource.SmsNotAvailable);
            }

            if (enable && StudioSmsNotificationSettings.LeftSms <= 0)
                throw new Exception(Resource.SmsNotPaidError);

            StudioSmsNotificationSettings.Enable = enable;

            MessageService.Send(Request, MessageAction.TwoFactorAuthenticationSettingsUpdated);

            return StudioSmsNotificationSettings.Enable;
        }

        ///<visible>false</visible>
        [Update("welcome/close")]
        public void CloseWelcomePopup()
        {
            var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            var collaboratorPopupSettings = SettingsManager.Instance.LoadSettingsFor<CollaboratorSettings>(currentUser.ID);

            if (!(currentUser.IsVisitor() && collaboratorPopupSettings.FirstVisit && !currentUser.IsOutsider()))
                throw new NotSupportedException("Not available.");

            collaboratorPopupSettings.FirstVisit = false;
            SettingsManager.Instance.SaveSettingsFor(collaboratorPopupSettings, SecurityContext.CurrentAccount.ID);
        }

        ///<visible>false</visible>
        [Update("firsttimetenantsettings")]
        public void SetFirstTimeTenantSettings()
        {
            var currentUser = CoreContext.UserManager.GetUsers(CurrentUser);
            if (!currentUser.IsOwner())
                throw new NotSupportedException("Access Denied.");

            FirstTimeTenantSettings.SetDefaultTenantSettings();
            FirstTimeTenantSettings.SendInstallInfo(currentUser);
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
        [Update("defaultpage")]
        public string SaveDefaultPageSettings(string defaultProductID)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var defaultPageSettingsObj = new StudioDefaultPageSettings
            {
                DefaultProductID = new Guid(defaultProductID)
            };
            SettingsManager.Instance.SaveSettings(defaultPageSettingsObj, TenantProvider.CurrentTenantID);

            MessageService.Send(HttpContext.Current.Request, MessageAction.DefaultStartPageSettingsUpdated);

            return Resources.Resource.SuccessfullySaveSettingsMessage;
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
            LicenseClient.RefreshLicense();
            return true;
        }
    }
}