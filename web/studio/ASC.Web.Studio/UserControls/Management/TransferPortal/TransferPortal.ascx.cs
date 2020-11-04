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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using System.Web;
using ASC.Web.Studio.Core.Backup;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Migration, Location)]
    [AjaxNamespace("TransferPortal")]
    public partial class TransferPortal : UserControl
    {
        public const string Location = "~/UserControls/Management/TransferPortal/TransferPortal.ascx";

        private static List<TransferRegion> transferRegions;

        protected string CurrentRegion
        {
            get { return TransferRegions.Where(x => x.IsCurrentRegion).Select(x => x.Name).FirstOrDefault() ?? string.Empty; }
        }

        protected string BaseDomain
        {
            get { return TransferRegions.Where(x => x.IsCurrentRegion).Select(x => x.BaseDomain).FirstOrDefault() ?? string.Empty; }
        }

        public static List<TransferRegion> TransferRegions
        {
            get { return transferRegions ?? (transferRegions = GetRegions()); }
        }

        protected bool IsVisibleMigration
        {
            get
            {
                return SetupInfo.IsVisibleSettings(ManagementType.Migration.ToString())
                    && TransferRegions.Count > 1;
            }
        }

        protected bool PaidMigration
        {
            get
            {
                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                return SetupInfo.IsSecretEmail(currentUser.Email) || TenantExtra.GetTenantQuota().HasMigration;
            }
        }

        protected bool OwnerMigration
        {
            get
            {
                return CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsOwner();
            }
        }

        protected string HelpLink { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            HelpLink = CommonLinkUtility.GetHelpLink();

            Page.RegisterBodyScripts("~/UserControls/Management/TransferPortal/js/transferportal.js")
                .RegisterStyle("~/UserControls/Management/TransferPortal/css/transferportal.less");

            popupTransferStart.Options.IsPopup = true;
        }

        private static List<TransferRegion> GetRegions()
        {
            try
            {
                using (var backupClient = new BackupServiceClient())
                {
                    return backupClient.GetTransferRegions();
                }
            }
            catch
            {
                return new List<TransferRegion>();
            }
        }
    }

    public static class TransferRegionExtension
    {
        public static string GetFullName(this TransferRegion transferRegion)
        {
            var result = TransferResourceHelper.GetRegionDescription(transferRegion.Name);
            return !string.IsNullOrEmpty(result) ? result : transferRegion.Name;
        }
    }
}