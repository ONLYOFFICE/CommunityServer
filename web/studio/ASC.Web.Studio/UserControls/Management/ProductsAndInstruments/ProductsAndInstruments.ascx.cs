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
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Core.WebZones;
using ASC.Web.Core.ModuleManagement;
using ASC.Web.Studio.Utility;
using System.Configuration;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.ProductsAndInstruments, Location)]
    public partial class ProductsAndInstruments : UserControl
    {
        #region Properies

        public const string Location = "~/UserControls/Management/ProductsAndInstruments/ProductsAndInstruments.ascx";

        protected List<Item> Products;
        protected List<Item> Modules;
        protected bool TenantAccessAnyone;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            InitProperties();
            RegisterClientScript();
        }

        #endregion

        #region Methods

        private void InitProperties()
        {
            Products = new List<Item>();
            Modules = new List<Item>();

            var managementPage = Page as Studio.Management;
            TenantAccessAnyone = managementPage != null ?
                                     managementPage.TenantAccess.Anyone :
                                     TenantAccessSettings.Load().Anyone;

            var webItems = WebItemManager.Instance.GetItems(WebZoneType.All, ItemAvailableState.All)
                                         .Where(item => !item.IsSubItem() && !item.CanNotBeDisabled() && item.Visible)
                                         .ToList();

            foreach (var webItem in webItems)
            {
                var isEnabledTalk = ConfigurationManagerExtension.AppSettings["web.talk"] ?? "false";
                if (webItem.GetSysName() == "talk" && isEnabledTalk == "false") continue;
                var item = new Item
                    {
                        ID = webItem.ID,
                        Name = webItem.Name,
                        IconUrl = webItem.GetIconAbsoluteURL(),
                        DisabledIconUrl = webItem.GetDisabledIconAbsoluteURL(),
                        SubItems = new List<Item>(),
                        ItemName = webItem.GetSysName(),
                        Disabled = !WebItemSecurity.GetSecurityInfo(webItem.ID.ToString()).Enabled
                    };

                foreach (var m in WebItemManager.Instance.GetSubItems(webItem.ID, ItemAvailableState.All))
                {
                    if ((m as Module) == null) continue;

                    var subItem = new Item
                        {
                            ID = m.ID,
                            Name = m.Name,
                            DisplayedAlways = (m as Module).DisplayedAlways,
                            ItemName = m.GetSysName(),
                            Disabled = !WebItemSecurity.GetSecurityInfo(m.ID.ToString()).Enabled
                        };

                    item.SubItems.Add(subItem);
                }

                if (webItem is IProduct)
                    Products.Add(item);
                else
                    Modules.Add(item);
            }
        }

        private void RegisterClientScript()
        {
            Page.RegisterBodyScripts("~/UserControls/Management/ProductsAndInstruments/js/productsandinstruments.js")
                .RegisterStyle("~/UserControls/Management/ProductsAndInstruments/css/productsandinstruments.less");
        }

        #endregion
    }
}