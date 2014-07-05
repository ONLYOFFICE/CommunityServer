/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Web.Core;
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
                        ItemName = webItem.GetSysName()
                    };

                var productInfo = WebItemSecurity.GetSecurityInfo(item.ID.ToString());
                item.Disabled = !productInfo.Enabled;

                foreach (var m in WebItemManager.Instance.GetSubItems(webItem.ID, ItemAvailableState.All))
                {
                    if ((m as Module) == null) continue;

                    var subItem = new Item
                        {
                            ID = m.ID,
                            Name = m.Name,
                            DisplayedAlways = (m as Module).DisplayedAlways,
                            ItemName = m.GetSysName()
                        };

                    var moduleInfo = WebItemSecurity.GetSecurityInfo(subItem.ID.ToString());
                    subItem.Disabled = !moduleInfo.Enabled;

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
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/productsandinstruments/js/productsandinstruments.js"));

            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/productsandinstruments/css/productsandinstruments.less"));
        }

        #endregion
    }
}