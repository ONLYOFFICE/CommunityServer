/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Api.Attributes;
using ASC.Api.Employee;
using ASC.Api.Interfaces;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.IPSecurity;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Api.Collections;
using ASC.Web.Core.Utility.Settings;
using ASC.Core.Users;

namespace ASC.Api.Settings
{
    ///<summary>
    /// Portal settings
    ///</summary>
    public class SettingsApi : IApiEntryPoint
    {
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

            return settings;
        }

        private static string GetProductName(Guid productId)
        {
            var product = WebItemManager.Instance[productId] as IProduct;
            return productId == Guid.Empty ? "All" : product != null ? product.Name : productId.ToString();
        }
    }
}