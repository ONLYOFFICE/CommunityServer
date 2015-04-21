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