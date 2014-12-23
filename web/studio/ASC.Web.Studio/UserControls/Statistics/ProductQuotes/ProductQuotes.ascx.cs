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
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using System.Web;
using System.Text;

namespace ASC.Web.Studio.UserControls.Statistics
{
    [ManagementControl(ManagementType.Statistic, Location)]
    public partial class ProductQuotes : UserControl
    {
        public const string Location = "~/UserControls/Statistics/ProductQuotes/ProductQuotes.ascx";

        public long CurrentSize { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/statistics/productquotes/css/productquotes_style.less"));

            var data = new List<object>();
            foreach (var item in WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.All, ItemAvailableState.All))
            {
                if (item.Context == null || item.Context.SpaceUsageStatManager == null)
                    continue;

                data.Add(new Product { Id = item.ID, Name = item.Name, Icon = item.GetIconAbsoluteURL() });
            }

            _itemsRepeater.ItemDataBound += _itemsRepeater_ItemDataBound;
            _itemsRepeater.DataSource = data;
            _itemsRepeater.DataBind();

            RegisterScript();
        }

        private void _itemsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {

            var product = e.Item.DataItem as Product;
            var webItem = WebItemManager.Instance[product.Id];

            var data = new List<object>();
            var items = webItem.Context.SpaceUsageStatManager.GetStatData();

            foreach (var it in items)
            {
                data.Add(new { Name = it.Name, Icon = it.ImgUrl, Size = FileSizeComment.FilesSizeToString(it.SpaceUsage), Url = it.Url });
            }

            if (items.Count == 0)
            {
                e.Item.FindControl("_emptyUsageSpace").Visible = true;
                e.Item.FindControl("_showMorePanel").Visible = false;

            }
            else
            {
                var repeater = (Repeater)e.Item.FindControl("_usageSpaceRepeater");
                repeater.DataSource = data;
                repeater.DataBind();


                e.Item.FindControl("_showMorePanel").Visible = (items.Count > 10);
                e.Item.FindControl("_emptyUsageSpace").Visible = false;
            }
        }

        protected String RenderCreatedDate()
        {
            return String.Format("{0}", CoreContext.TenantManager.GetCurrentTenant().CreatedDateTime.ToShortDateString());
        }

        protected int RenderUsersTotal()
        {
            return TenantStatisticsProvider.GetUsersCount();
        }

        protected String GetMaxTotalSpace()
        {
            return FileSizeComment.FilesSizeToString(TenantExtra.GetTenantQuota().MaxTotalSize);
        }

        protected String RenderUsedSpace()
        {
            var used = TenantStatisticsProvider.GetUsedSize();
            return FileSizeComment.FilesSizeToString(used);
        }

        private void RegisterScript()
        {
            var sb = new StringBuilder();

            sb.Append(@"
                    jq('.moreBox a.topTitleLink').click(function () {
                        jq(jq(this).parent().prev()).find('tr').show();
                        jq(this).parent().hide();
                    });"
                );

            Page.RegisterInlineScript(sb.ToString());
        }

        protected sealed class Product
        {
            public Guid Id { get; set; }
            public String Name { get; set; }
            public String Icon { get; set; }
            public long Size { get; set; }
        }
    }
}