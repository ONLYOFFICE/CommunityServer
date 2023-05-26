/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Utility;
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
        public bool EnableUserQuota { get; set; }

        public string LastUpdate { get; set; }
        public string DefaultUserQuota { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page
                .RegisterStyle("~/UserControls/Statistics/ProductQuotes/css/dark_productquotes_style.less");
            }
            else
            {
                Page
                .RegisterStyle("~/UserControls/Statistics/ProductQuotes/css/productquotes_style.less");
            }

            Page
                .RegisterStaticScripts(GetStaticJavaScript())
                .RegisterBodyScripts("~/UserControls/Statistics/ProductQuotes/js/product_quotes.js")
                .RegisterBodyScripts("~/UserControls/Statistics/ProductQuotes/js/jq_quota_extensions.js")
                .RegisterBodyScripts("~/UserControls/Statistics/ProductQuotes/js/user_quota_controller.js")
                .RegisterBodyScripts("~/UserControls/Statistics/ProductQuotes/js/navigatorhandler.js");

            MaxTotalSpace = TenantExtra.GetTenantQuota().MaxTotalSize;
            UsedSpace = TenantStatisticsProvider.GetUsedSize();
            var quotaSettings = TenantUserQuotaSettings.Load();
            EnableUserQuota = quotaSettings.EnableUserQuota;
            LastUpdate = quotaSettings.LastRecalculateDate != DateTime.MinValue ? TimeZoneInfo.ConvertTimeFromUtc(quotaSettings.LastRecalculateDate, CoreContext.TenantManager.GetCurrentTenant().TimeZone).ToString() : null;
            DefaultUserQuota = FileSizeComment.FilesSizeToString(quotaSettings.DefaultUserQuota);
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            var result = new ScriptBundleData("quota", "studio");
            result.AddSource(ResolveUrl, new ClientTemplateResources());
            return result;
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
            if (!CoreContext.Configuration.Standalone || maxActiveUsers != Constants.MaxEveryoneCount)
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
            return !CoreContext.Configuration.Standalone && UsedSpace > MaxTotalSpace * 9 / 10 ? "red-text" : string.Empty;
        }
    }
}