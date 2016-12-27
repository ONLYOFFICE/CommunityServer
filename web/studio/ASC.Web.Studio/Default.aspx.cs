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


using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Files;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ASC.Web.Studio
{
    public partial class _Default : MainPage
    {
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

        protected List<IWebItem> defaultListProducts;

        protected void Page_Load(object sender, EventArgs e)
        {
            CurrentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);

            Page.RegisterStyle("~/skins/page_default.less");

            var defaultPageSettings = SettingsManager.Instance.LoadSettings<StudioDefaultPageSettings>(TenantProvider.CurrentTenantID);
            if (defaultPageSettings != null && defaultPageSettings.DefaultProductID != Guid.Empty)
            {
                if (defaultPageSettings.DefaultProductID == defaultPageSettings.FeedModuleID && !CurrentUser.IsOutsider())
                {
                    Response.Redirect("feed.aspx", true);
                }

                var products = WebItemManager.Instance.GetItemsAll<IProduct>();
                foreach (var p in products)
                {
                    if (p.ID.Equals(defaultPageSettings.DefaultProductID))
                    {
                        var productInfo = WebItemSecurity.GetSecurityInfo(p.ID.ToString());
                        if (productInfo.Enabled && WebItemSecurity.IsAvailableForUser(p.ID.ToString(), CurrentUser.ID))
                        {
                            var url = p.StartURL;
                            if (Request.DesktopApp())
                            {
                                url += "?desktop=true";
                                if (!string.IsNullOrEmpty(Request["first"]))
                                {
                                    url += "&first=true";
                                }
                            }
                            Response.Redirect(url, true);
                        }
                    }
                }
            }

            Master.DisabledSidePanel = true;

            Title = Resource.MainPageTitle;
            defaultListProducts = WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.StartProductList);
            _showDocs = (Product)defaultListProducts.Find(r => r.ID == WebItemManager.DocumentsProductID);
            if (_showDocs != null)
            {
                defaultListProducts.RemoveAll(r => r.ID == _showDocs.ProductID);
            }

            var mailProduct = WebItemManager.Instance[WebItemManager.MailProductID];
            if (mailProduct != null && !mailProduct.IsDisabled()) {
                defaultListProducts.Add(mailProduct);
            }

            var calendarProduct = WebItemManager.Instance[WebItemManager.CalendarProductID];
            if (calendarProduct != null && !calendarProduct.IsDisabled())
            {
                defaultListProducts.Add(calendarProduct);
            }

            var talkProduct = WebItemManager.Instance[WebItemManager.TalkProductID];
            if (talkProduct != null && !talkProduct.IsDisabled())
            {
                defaultListProducts.Add(talkProduct);
            }

            var priority = GetStartProductsPriority();

            defaultListProducts = defaultListProducts
                .Where(p => priority.Keys.Contains(p.ID))
                .OrderBy(p => priority[p.ID])
                .ToList();

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

        private static Dictionary<Guid, Int32> GetStartProductsPriority()
        {
            var priority = new Dictionary<Guid, Int32>
                    {
                        {WebItemManager.ProjectsProductID, 0},
                        {WebItemManager.CRMProductID, 1},
                        {WebItemManager.MailProductID, 2},
                        {WebItemManager.PeopleProductID, 3},
                        {WebItemManager.CommunityProductID, 4}
                    };

            if (!string.IsNullOrEmpty(SetupInfo.StartProductList))
            {
                var products = SetupInfo.StartProductList.Split(',');

                if (products.Any())
                {
                    priority = new Dictionary<Guid, int>();

                    for (var i = 0; i < products.Length; i++)
                    {
                        var productId = GetProductId(products[i]);
                        if (productId != Guid.Empty)
                            priority.Add(productId, i);
                    }
                }
            }

            return priority;
        }

        private static Guid GetProductId(string productName)
        {
            switch (productName.ToLowerInvariant())
            {
                case "documents":
                    return WebItemManager.DocumentsProductID;
                case "projects":
                    return WebItemManager.ProjectsProductID;
                case "crm":
                    return WebItemManager.CRMProductID;
                case "people":
                    return WebItemManager.PeopleProductID;
                case "community":
                    return WebItemManager.CommunityProductID;
                case "mail":
                    return WebItemManager.MailProductID;
                case "calendar":
                    return WebItemManager.CalendarProductID;
                case "talk":
                    return WebItemManager.TalkProductID;
                default:
                    return Guid.Empty;
            }
        }

        protected string GetProductLabel(IWebItem product)
        {
            if (product.ID == WebItemManager.CRMProductID)
                return Resource.ProductCRMAndVoIP;

            if (product.ID == WebItemManager.MailProductID &&
                SetupInfo.IsVisibleSettings("AdministrationPage") &&
                CurrentUser.IsAdmin() &&
                UserControls.Management.MailService.MailServiceHelper.GetMailServerInfo() != null)
                return Resource.AdministrationLabel;

            return HttpUtility.HtmlEncode(product.Name);
        }
    }
}