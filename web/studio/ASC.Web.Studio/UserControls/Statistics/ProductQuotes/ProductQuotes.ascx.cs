/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using ASC.Core;
using ASC.Core.Billing;
using ASC.Web.Core;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Statistics
{
    [ManagementControl(ManagementType.Statistic, Location)]
    public partial class ProductQuotes : UserControl
    {
        public const string Location = "~/UserControls/Statistics/ProductQuotes/ProductQuotes.ascx";

        private long MaxTotalSpace { get; set; }

        private long UsedSpace { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page
                .RegisterStyle("~/usercontrols/statistics/productquotes/css/productquotes_style.less")
                .RegisterBodyScripts("~/usercontrols/statistics/productquotes/js/product_quotes.js");

            MaxTotalSpace = TenantExtra.GetTenantQuota().MaxTotalSize;
            UsedSpace = TenantStatisticsProvider.GetUsedSize();
        }

        protected IEnumerable<IWebItem> GetWebItems()
        {
            return WebItemManager.Instance.GetItems(Web.Core.WebZones.WebZoneType.All, ItemAvailableState.All)
                                 .Where(item =>
                                        item != null &&
                                        item.Visible &&
                                        item.Context != null &&
                                        item.Context.SpaceUsageStatManager != null);
        }

        protected String RenderCreatedDate()
        {
            return String.Format("{0}", CoreContext.TenantManager.GetCurrentTenant().CreatedDateTime.ToShortDateString());
        }

        protected string RenderUsersTotal()
        {
            var result = TenantStatisticsProvider.GetUsersCount().ToString();

            var maxActiveUsers = TenantExtra.GetTenantQuota().ActiveUsers;
            if (!CoreContext.Configuration.Standalone || maxActiveUsers != LicenseReader.MaxUserCount)
            {
                result += " / " + maxActiveUsers;
            }

            return result;
        }

        protected String RenderMaxTotalSpace()
        {
            return FileSizeComment.FilesSizeToString(MaxTotalSpace);
        }

        protected String RenderUsedSpace()
        {
            return FileSizeComment.FilesSizeToString(UsedSpace);
        }

        protected String RenderUsedSpaceClass()
        {
            return !CoreContext.Configuration.Standalone && UsedSpace > MaxTotalSpace*9/10 ? "red-text" : string.Empty;
        }
    }
}