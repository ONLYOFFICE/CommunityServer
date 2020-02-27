/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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