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
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Linq;
using System.Configuration;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ProductsAndInstruments, Location, SortOrder = 100)]
    public partial class DefaultPageSettings : UserControl
    {
        public const string Location = "~/UserControls/Management/DefaultPageSettings/DefaultPageSettings.ascx";

        protected List<DefaultStartPageWrapper> DefaultPages { get; set; }
        protected Guid DefaultProductID { get; set; }
        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterBodyScripts("~/UserControls/Management/DefaultPageSettings/js/defaultpage.js");

            DefaultPages = new List<DefaultStartPageWrapper>();

            var defaultPageSettings = StudioDefaultPageSettings.Load();
            DefaultProductID = defaultPageSettings.DefaultProductID;

            var products = WebItemManager.Instance.GetItemsAll<IProduct>().Where(p => p.Visible);
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

            var addons = WebItemManager.Instance.GetItemsAll<IAddon>().Where(a => a.Visible && a.ID != WebItemManager.VoipModuleID);
            var isEnabledTalk = ConfigurationManagerExtension.AppSettings["web.talk"] ?? "false";
            foreach (var a in addons)
            {
                var productInfo = WebItemSecurity.GetSecurityInfo(a.ID.ToString());
                if (a.GetSysName() == "talk" && isEnabledTalk == "false") continue;
                if (productInfo.Enabled)
                    DefaultPages.Add(new DefaultStartPageWrapper
                    {
                        ProductID = a.ID,
                        DisplayName = a.Name,
                        ProductName = a.GetSysName(),
                        IsSelected = DefaultProductID.Equals(a.ID)
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

            HelpLink = CommonLinkUtility.GetHelpLink();
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