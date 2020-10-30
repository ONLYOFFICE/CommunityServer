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
using ASC.Data.Storage;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.AuditTrail, Location)]
    public partial class AuditTrail : UserControl
    {
        public const string Location = "~/UserControls/Management/AuditTrail/AuditTrail.ascx";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (CoreContext.Configuration.Standalone || !SetupInfo.IsVisibleSettings(ManagementType.AuditTrail.ToString()))
            {
                Response.Redirect(CommonLinkUtility.GetDefault(), true);
                return;
            }

            Page.RegisterBodyScripts("~/UserControls/Management/AuditTrail/js/audittrail.js")
                .RegisterStyle("~/UserControls/Management/AuditTrail/css/audittrail.less");

            var emptyScreenControl = new EmptyScreenControl
            {
                ImgSrc = WebPath.GetPath("UserControls/Management/AuditTrail/img/audit_trail_empty_screen.jpg"),
                Header = AuditResource.AuditTrailEmptyScreenHeader,
                Describe = AuditResource.AuditTrailEmptyScreenDscr
            };
            emptyScreenHolder.Controls.Add(emptyScreenControl);
        }
    }
}