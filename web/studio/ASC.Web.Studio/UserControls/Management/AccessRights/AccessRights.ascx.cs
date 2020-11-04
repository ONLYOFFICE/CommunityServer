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


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using ASC.ActiveDirectory.Base.Settings;
using ASC.Core;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Notify;
using ASC.Web.Studio.Core.Users;
using ASC.Web.Studio.Utility;

using Resources;

using AjaxPro;
using ASC.Web.Core.WhiteLabel;
using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.AccessRights, Location)]
    [AjaxNamespace("AccessRightsController")]
    public partial class AccessRights : UserControl
    {
        #region Properies

        public const string Location = "~/UserControls/Management/AccessRights/AccessRights.ascx";

        protected bool CanOwnerEdit;

        private List<IWebItem> products;

        protected List<IWebItem> Products
        {
            get { return products ?? (products = GetProductList()); }
        }

        protected string[] FullAccessOpportunities { get; set; }

        protected List<string> LdapRights { get; set; }

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            InitControl();
            RegisterClientScript();
        }

        #endregion

        #region Methods

        private void InitControl()
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            //owner settings
            var curTenant = CoreContext.TenantManager.GetCurrentTenant();
            var currentOwner = CoreContext.UserManager.GetUsers(curTenant.OwnerId);
            CanOwnerEdit = currentOwner.ID.Equals(SecurityContext.CurrentAccount.ID);

            _phOwnerCard.Controls.Add(new EmployeeUserCard
                {
                    EmployeeInfo = currentOwner,
                    EmployeeUrl = currentOwner.GetUserProfilePageURL(),
                    Width = 330
                });

            FullAccessOpportunities = Resource.AccessRightsFullAccessOpportunities.Split('|');
        }

        private void RegisterClientScript()
        {
            var isRetina = TenantLogoManager.IsRetina(HttpContext.Current.Request);

            Page.RegisterBodyScripts("~/UserControls/Management/AccessRights/js/accessrights.js")
                .RegisterStyle("~/UserControls/Management/AccessRights/css/accessrights.less");

            var curTenant = CoreContext.TenantManager.GetCurrentTenant();
            var currentOwner = CoreContext.UserManager.GetUsers(curTenant.OwnerId);

            var admins = WebItemSecurity.GetProductAdministrators(Guid.Empty).ToList();

            admins = admins
                .GroupBy(admin => admin.ID)
                .Select(group => group.First())
                .Where(admin => admin.ID != currentOwner.ID)
                .SortByUserName();

            InitLdapRights();

            var sb = new StringBuilder();

            sb.AppendFormat("ownerId = \"{0}\";", curTenant.OwnerId);

            sb.AppendFormat("adminList = {0};", JsonConvert.SerializeObject(admins.ConvertAll(u => new
                {
                    id = u.ID,
                    smallFotoUrl = u.GetSmallPhotoURL(),
                    bigFotoUrl = isRetina ? u.GetBigPhotoURL() : "",
                    displayName = u.DisplayUserName(),
                    title = u.Title.HtmlEncode(),
                    userUrl = CommonLinkUtility.GetUserProfile(u.ID),
                    accessList = GetAccessList(u.ID, WebItemSecurity.IsProductAdministrator(Guid.Empty, u.ID)),
                    ldap = LdapRights.Contains(u.ID.ToString())
                })));

            sb.AppendFormat("imageHelper = {0};", JsonConvert.SerializeObject(new
                {
                    PeopleImgSrc = WebImageSupplier.GetAbsoluteWebPath("user_12.png"),
                    GroupImgSrc = WebImageSupplier.GetAbsoluteWebPath("group_12.png"),
                    TrashImgSrc = WebImageSupplier.GetAbsoluteWebPath("trash_12.png"),
                    TrashImgTitle = Resource.DeleteButton
                }));

            var managementPage = Page as Studio.Management;
            var tenantAccess = managementPage != null ? managementPage.TenantAccess : TenantAccessSettings.Load();

            if (!tenantAccess.Anyone)
            {
                var productItemList = GetProductItemListForSerialization();

                foreach (var productItem in productItemList.Where(productItem => !productItem.CanNotBeDisabled))
                {
                    sb.AppendFormat("ASC.Settings.AccessRights.initProduct('{0}');", Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(productItem))));
                }
            }

            sb.AppendFormat("ASC.Settings.AccessRights.init({0});",
                            JsonConvert.SerializeObject(Products.Select(p => p.GetSysName()).ToArray()));

            Page.RegisterInlineScript(sb.ToString());
        }

        private void InitLdapRights()
        {
            if (LdapRights != null) return;

            var ldapRightsSettings = LdapCurrentAcccessSettings.Load();
            LdapRights = ldapRightsSettings.CurrentAccessRights == null
                             ? new List<string>()
                             : ldapRightsSettings.CurrentAccessRights.SelectMany(r => r.Value).Distinct().ToList();
        }

        private static List<IWebItem> GetProductList()
        {
            var webItems = new List<IWebItem>();
            webItems.AddRange(WebItemManager.Instance.GetItemsAll<IProduct>().Where(p => p.Visible).ToList());
            webItems.Add(WebItemManager.Instance[WebItemManager.MailProductID]);
            return webItems;
        }

        private IEnumerable<Item> GetProductItemListForSerialization()
        {
            var data = new List<Item>();

            foreach (var p in Products)
            {
                if (String.IsNullOrEmpty(p.Name)) continue;

                var userOpportunities = p.GetUserOpportunities();

                var item = new Item
                    {
                        ID = p.ID,
                        Name = p.Name.HtmlEncode(),
                        IconUrl = p.GetIconAbsoluteURL(),
                        DisabledIconUrl = p.GetDisabledIconAbsoluteURL(),
                        SubItems = new List<Item>(),
                        ItemName = p.GetSysName(),
                        UserOpportunitiesLabel = String.Format(CustomNamingPeople.Substitute<Resource>("AccessRightsProductUsersCan"), p.Name).HtmlEncode(),
                        UserOpportunities = userOpportunities != null ? userOpportunities.ConvertAll(uo => uo.HtmlEncode()) : null,
                        CanNotBeDisabled = p.CanNotBeDisabled()
                    };

                if (p.HasComplexHierarchyOfAccessRights())
                    item.UserOpportunitiesLabel = String.Format(CustomNamingPeople.Substitute<Resource>("AccessRightsProductUsersWithRightsCan"), item.Name).HtmlEncode();

                var productInfo = WebItemSecurity.GetSecurityInfo(item.ID.ToString());
                item.Disabled = !productInfo.Enabled;
                item.SelectedGroups = productInfo.Groups.Select(g => new SelectedItem {ID = g.ID, Name = g.Name});
                item.SelectedUsers = productInfo.Users.Select(g => new SelectedItem {ID = g.ID, Name = g.DisplayUserName()});

                data.Add(item);
            }

            return data;
        }

        private object GetAccessList(Guid uId, bool fullAccess)
        {
            var res = new List<object>
                {
                    new
                        {
                            pId = Guid.Empty,
                            pName = "full",
                            pAccess = fullAccess,
                            disabled = LdapRights.Contains(uId.ToString()) || uId == SecurityContext.CurrentAccount.ID
                        }
                };

            foreach (var p in Products)
                res.Add(new
                    {
                        pId = p.ID,
                        pName = p.GetSysName(),
                        pAccess = fullAccess || WebItemSecurity.IsProductAdministrator(p.ID, uId),
                        disabled = LdapRights.Contains(uId.ToString()) || fullAccess
                    });

            return res;
        }

        #endregion

        #region AjaxMethods

        [AjaxMethod]
        public object ChangeOwner(Guid ownerId)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var curTenant = CoreContext.TenantManager.GetCurrentTenant();
                var owner = CoreContext.UserManager.GetUsers(curTenant.OwnerId);
                var newOwner = CoreContext.UserManager.GetUsers(ownerId);

                if (newOwner.IsVisitor()) throw new System.Security.SecurityException("Collaborator can not be an owner");

                if (!owner.ID.Equals(SecurityContext.CurrentAccount.ID) || Guid.Empty.Equals(newOwner.ID))
                {
                    return new { Status = 0, Message = Resource.ErrorAccessDenied };
                }

                var confirmLink = CommonLinkUtility.GetConfirmationUrl(owner.Email, ConfirmType.PortalOwnerChange, newOwner.ID, newOwner.ID);
                StudioNotifyService.Instance.SendMsgConfirmChangeOwner(owner, newOwner, confirmLink);

                MessageService.Send(HttpContext.Current.Request, MessageAction.OwnerSentChangeOwnerInstructions, MessageTarget.Create(owner.ID), owner.DisplayUserName(false));

                var emailLink = string.Format("<a href=\"mailto:{0}\">{0}</a>", owner.Email.HtmlEncode());
                return new { Status = 1, Message = Resource.ChangePortalOwnerMsg.Replace(":email", emailLink) };
            }
            catch (Exception e)
            {
                return new { Status = 0, Message = e.Message.HtmlEncode() };
            }
        }

        [AjaxMethod]
        public object AddAdmin(Guid id)
        {
            var isRetina = TenantLogoManager.IsRetina(HttpContext.Current.Request);

            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

            var user = CoreContext.UserManager.GetUsers(id);

            if (user.IsVisitor())
                throw new System.Security.SecurityException("Collaborator can not be an administrator");

            WebItemSecurity.SetProductAdministrator(Guid.Empty, id, true);

            MessageService.Send(HttpContext.Current.Request, MessageAction.AdministratorAdded, MessageTarget.Create(user.ID), user.DisplayUserName(false));

            InitLdapRights();

            return new
                {
                    id = user.ID,
                    smallFotoUrl = user.GetSmallPhotoURL(),
                    bigFotoUrl = isRetina ? user.GetBigPhotoURL() : "",
                    displayName = user.DisplayUserName(),
                    title = user.Title.HtmlEncode(),
                    userUrl = CommonLinkUtility.GetUserProfile(user.ID),
                    accessList = GetAccessList(user.ID, true)
                };
        }

        #endregion
    }

    public class Item
    {
        public bool Disabled { get; set; }
        public bool DisplayedAlways { get; set; }
        public bool HasPermissionSettings { get; set; }
        public bool CanNotBeDisabled { get; set; }
        public string Name { get; set; }
        public string ItemName { get; set; }
        public string IconUrl { get; set; }
        public string DisabledIconUrl { get; set; }
        public string AccessSwitcherLabel { get; set; }
        public string UserOpportunitiesLabel { get; set; }
        public List<string> UserOpportunities { get; set; }
        public Guid ID { get; set; }
        public List<Item> SubItems { get; set; }
        public IEnumerable<SelectedItem> SelectedUsers { get; set; }
        public IEnumerable<SelectedItem> SelectedGroups { get; set; }
    }

    public class SelectedItem
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
    }
}