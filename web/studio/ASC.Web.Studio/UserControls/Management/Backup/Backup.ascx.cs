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
using System.Web;
using System.Web.UI;

using ASC.Core;
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

        protected string HelpLink { get; set; }
        protected int TenantId { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.Standalone || !SetupInfo.IsVisibleSettings(ManagementType.Backup.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            TenantId = TenantProvider.CurrentTenantID;
            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            Page.RegisterStyle("~/UserControls/Management/Backup/css/backup.less",
                               "~/Products/Files/Controls/FileSelector/fileselector.css",
                               "~/Products/Files/Controls/ThirdParty/thirdparty.css",
                               "~/Products/Files/Controls/ContentList/contentlist.css",
                               "~/Products/Files/Controls/EmptyFolder/emptyfolder.css",
                               "~/Products/Files/Controls/Tree/tree.css"
                )
                .RegisterBodyScripts("~/UserControls/Management/Backup/js/backup.js",
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

            if (!CoreContext.Configuration.Standalone && SetupInfo.IsVisibleSettings("Restore"))
            {
                RestoreHolder.Controls.Add(LoadControl(Restore.Location));
            }

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}