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

using Amazon;

using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.UserControls.Management
{
    public partial class Restore : UserControl
    {
        public const string Location = "~/UserControls/Management/Restore/Restore.ascx";

        protected bool IsSendNotification
        {
            get { return true; }
        }
        protected List<BackupHistoryRecord> BackupVersions { get; set; }

        protected List<RegionEndpoint> Regions
        {
            get { return RegionEndpoint.EnumerableAllRegions.ToList(); }
        }
        protected bool isAvailable
        {
            get
            {
                return CoreContext.Configuration.Standalone || TenantExtra.GetTenantQuota().Restore;
            }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SetupInfo.IsVisibleSettings("Restore"))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            _restoreChooseBackupDialog.Options.IsPopup = true;

            Page
                .RegisterStyle("~/UserControls/Management/Restore/css/restore.less")
                .RegisterBodyScripts("~/js/uploader/jquery.fileupload.js")
                .RegisterBodyScripts("~/UserControls/Management/Restore/js/restore.js");

            HelpLink = CommonLinkUtility.GetHelpLink();
        }
    }
}