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
                                     SettingsManager.Instance.LoadSettings<TenantAccessSettings>(TenantProvider.CurrentTenantID).Anyone;

            var webItems = WebItemManager.Instance.GetItems(WebZoneType.All, ItemAvailableState.All)
                                         .Where(item => !item.IsSubItem() && !item.CanNotBeDisabled())
                                         .ToList();

            foreach (var webItem in webItems)
            {
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
            Page.RegisterBodyScripts("~/usercontrols/management/productsandinstruments/js/productsandinstruments.js");
            Page.RegisterStyle("~/usercontrols/management/productsandinstruments/css/productsandinstruments.less");
        }

        #endregion
    }
}