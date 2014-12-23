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
using System.Web.UI;
using ASC.Core;
using ASC.MessagingSystem;
using AjaxPro;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using ASC.Web.Core;
using System.Collections.Generic;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ProductsAndInstruments, Location, SortOrder = 100)]
    [AjaxNamespace("DefaultPageController")]
    public partial class DefaultPageSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/DefaultPageSettings/DefaultPageSettings.ascx";

        protected List<DefaultStartPageWrapper> DefaultPages { get; set; }
        protected Guid DefaultProductID { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/defaultpagesettings/js/defaultpage.js"));

            DefaultPages = new List<DefaultStartPageWrapper>();

            var defaultPageSettings = SettingsManager.Instance.LoadSettings<StudioDefaultPageSettings>(TenantProvider.CurrentTenantID);
            DefaultProductID = defaultPageSettings.DefaultProductID;

            var products = WebItemManager.Instance.GetItemsAll<IProduct>();
            foreach (var p in products)
            {
                var productInfo = WebItemSecurity.GetSecurityInfo(p.ID.ToString());
                if (productInfo.Enabled)
                    DefaultPages.Add(new DefaultStartPageWrapper
                        {
                            ProductID = p.ID,
                            DisplayName = p.Name,
                            ProductName = p.GetSysName(),
                            IsSelected = DefaultProductID.Equals(p.ID)
                        });
            }

            DefaultPages.Add(new DefaultStartPageWrapper
                {
                    ProductID = defaultPageSettings.FeedModuleID,
                    DisplayName = Resources.UserControlsCommonResource.FeedTitle,
                    ProductName = "feed",
                    IsSelected = DefaultProductID.Equals(defaultPageSettings.FeedModuleID)
                });

            DefaultPages.Add(new DefaultStartPageWrapper
                {
                    ProductID = Guid.Empty,
                    DisplayName = Resources.Resource.DefaultPageSettingsChoiseOfProducts,
                    ProductName = string.Empty,
                    IsSelected = DefaultProductID.Equals(Guid.Empty)
                });
        }

        [AjaxMethod]
        public object SaveSettings(string defaultProductID)
        {
            try
            {
                SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);

                var defaultPageSettingsObj = new StudioDefaultPageSettings
                    {
                        DefaultProductID = new Guid(defaultProductID)
                    };
                SettingsManager.Instance.SaveSettings(defaultPageSettingsObj, TenantProvider.CurrentTenantID);

                MessageService.Send(HttpContext.Current.Request, MessageAction.DefaultStartPageSettingsUpdated);

                return new
                    {
                        Status = 1,
                        Message = Resources.Resource.SuccessfullySaveSettingsMessage
                    };
            }
            catch(Exception e)
            {
                return new {Status = 0, Message = e.Message.HtmlEncode()};
            }
        }
    }

    public class DefaultStartPageWrapper
    {
        public Guid ProductID { get; set; }
        public string DisplayName { get; set; }
        public string ProductName { get; set; }
        public bool IsSelected { get; set; }
    }
}