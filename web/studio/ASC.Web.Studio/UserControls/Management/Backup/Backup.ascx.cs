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
using System.Web;
using System.Web.UI;

using ASC.Core;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.UserControls.Common.ChooseTimePeriod;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Backup, Location)]
    public partial class Backup : UserControl
    {
        public const string Location = "~/UserControls/Management/Backup/Backup.ascx";

        protected bool EnableBackup
        {
            get { return SetupInfo.IsVisibleSettings(ManagementType.Backup.ToString()); }
        }

        protected bool EnableAutoBackup
        {
            get { return SetupInfo.IsVisibleSettings("AutoBackup"); }
        }

        protected bool AutoBackup
        {
            get
            {
                return CoreContext.Configuration.Standalone || TenantExtra.GetTenantQuota().AutoBackup;
            }
        }

        protected string HelpLink { get; set; }
        protected int TenantId { get; set; }
        public string TariffPageLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            TariffPageLink = TenantExtra.GetTariffPageLink();
            if (!SetupInfo.IsVisibleSettings(ManagementType.Backup.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            TenantId = TenantProvider.CurrentTenantID;
            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            if (ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/UserControls/Management/Backup/css/backup.less",
                               "~/Products/Files/Controls/FileSelector/fileselector.less",
                               "~/Products/Files/Controls/ThirdParty/dark-thirdparty.less",
                               "~/Products/Files/Controls/ContentList/dark-contentlist.less",
                               "~/Products/Files/Controls/EmptyFolder/emptyfolder.less",
                               "~/Products/Files/Controls/Tree/dark-tree.less"
                );
            }
            else
            {
                Page.RegisterStyle("~/UserControls/Management/Backup/css/backup.less",
                               "~/Products/Files/Controls/FileSelector/fileselector.less",
                               "~/Products/Files/Controls/ThirdParty/thirdparty.less",
                               "~/Products/Files/Controls/ContentList/contentlist.less",
                               "~/Products/Files/Controls/EmptyFolder/emptyfolder.less",
                               "~/Products/Files/Controls/Tree/tree.less"
                );
            }
            Page.RegisterBodyScripts("~/UserControls/Management/Backup/js/backup.js",
                                     "~/UserControls/Management/Backup/js/consumersettings.js",
                                     "~/Products/Files/Controls/Tree/tree.js",
                                     "~/Products/Files/Controls/EmptyFolder/emptyfolder.js",
                                     "~/Products/Files/Controls/FileSelector/fileselector.js",
                                     "~/Products/Files/js/common.js",
                                     "~/Products/Files/js/templatemanager.js",
                                     "~/Products/Files/js/servicemanager.js",
                                     "~/Products/Files/js/ui.js",
                                     "~/Products/Files/js/eventhandler.js"
                );

            FolderSelectorHolder.Controls.Add(LoadControl(CommonLinkUtility.ToAbsolute("~/Products/Files/Controls/FileSelector/FileSelector.ascx")));

            BackupTimePeriod.Controls.Add(LoadControl(ChooseTimePeriod.Location));

            if (SetupInfo.IsVisibleSettings("Restore"))
            {
                RestoreHolder.Controls.Add(LoadControl(Restore.Location));
            }

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}