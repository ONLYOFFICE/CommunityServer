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
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.Core;
using AjaxPro;

namespace ASC.Web.Studio
{
    [AjaxNamespace("StudioDefault")]
    public partial class _Default : MainPage
    {
        public bool ShowWelcomePopupForCollaborator { get; set; }

        protected Product _showDocs;

        protected UserInfo CurrentUser;

        protected override void OnPreInit(EventArgs e)
        {
            base.OnPreInit(e);
            if (CoreContext.Configuration.Personal)
                Context.Response.Redirect(FilesLinkUtility.FilesBaseAbsolutePath);
        }
        protected bool? IsAutorizePartner { get; set; }
        protected Partner Partner { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/skins/page_default.less"));

            var defaultPageSettings = SettingsManager.Instance.LoadSettings<StudioDefaultPageSettings>(TenantProvider.CurrentTenantID);
            if (defaultPageSettings != null && defaultPageSettings.DefaultProductID != Guid.Empty)
            {
                if (defaultPageSettings.DefaultProductID == defaultPageSettings.FeedModuleID && !CurrentUser.IsOutsider())
                    Context.Response.Redirect("feed.aspx");

                var products = WebItemManager.Instance.GetItemsAll<IProduct>();
                foreach (var p in products)
                {
                    if (p.ID.Equals(defaultPageSettings.DefaultProductID))
                    {
                        var productInfo = WebItemSecurity.GetSecurityInfo(p.ID.ToString());
                        if (productInfo.Enabled && WebItemSecurity.IsAvailableForUser(p.ID.ToString(), CurrentUser.ID))
                        {
                            Context.Response.Redirect(p.StartURL);
                        }
                    }
                }
            }

            Master.DisabledSidePanel = true;

            Title = Resources.Resource.MainPageTitle;
            var items = WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.StartProductList);
            _showDocs = (Product)items.Find(r => r.ID == WebItemManager.DocumentsProductID);
            if (_showDocs != null)
            {
                items.RemoveAll(r => r.ID == _showDocs.ProductID);
            }
            _productRepeater.DataSource = items;
            _productRepeater.DataBind();

            var collaboratorPopupSettings = SettingsManager.Instance.LoadSettingsFor<CollaboratorSettings>(CurrentUser.ID);

            if (CurrentUser.IsVisitor() && collaboratorPopupSettings.FirstVisit && !CurrentUser.IsOutsider())
            {
                AjaxPro.Utility.RegisterTypeForAjax(GetType());

                ShowWelcomePopupForCollaborator = true;
                _welcomePopupForCollaborators.Visible = true;
                _welcomeCollaboratorContainer.Options.IsPopup = true;

                Page.RegisterInlineScript("StudioBlockUIManager.blockUI('#studio_welcomeCollaboratorContainer', 500, 400, 0);");
            }

            if (CoreContext.Configuration.PartnerHosted)
            {
                IsAutorizePartner = false;
                var partner = CoreContext.PaymentManager.GetApprovedPartner();
                if (partner != null)
                {
                    IsAutorizePartner = !string.IsNullOrEmpty(partner.AuthorizedKey);
                    Partner = partner;
                }
            }
        }

        [AjaxMethod]
        public void CloseWelcomePopup()
        {
            var collaboratorPopupSettings = SettingsManager.Instance.LoadSettingsFor<CollaboratorSettings>(SecurityContext.CurrentAccount.ID);
            collaboratorPopupSettings.FirstVisit = false;
            SettingsManager.Instance.SaveSettingsFor(collaboratorPopupSettings, SecurityContext.CurrentAccount.ID);
        }
    }
}