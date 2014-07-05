/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using ASC.Core;
using ASC.Core.Common.Contracts;
using ASC.Core.Users;
using ASC.Web.Studio.Core;
using ASC.Web.Studio.Utility;
using AjaxPro;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Web;
using Resources;

namespace ASC.Web.Studio.UserControls.Management
{
    [AjaxNamespace("TransferPortal")]
    public partial class TransferPortal : UserControl
    {
        protected class TransferRegionWithName : TransferRegion
        {
            public string FullName { get; set; }
        }

        public static string Location
        {
            get { return "~/UserControls/Management/TransferPortal/TransferPortal.ascx"; }
        }

        private List<TransferRegionWithName> _transferRegions;

        protected string CurrentRegion
        {
            get { return TransferRegions.Where(x => x.IsCurrentRegion).Select(x => x.Name).FirstOrDefault() ?? string.Empty; }
        }

        protected string BaseDomain
        {
            get { return TransferRegions.Where(x => x.IsCurrentRegion).Select(x => x.BaseDomain).FirstOrDefault() ?? string.Empty; }
        }

        protected List<TransferRegionWithName> TransferRegions
        {
            get { return _transferRegions ?? (_transferRegions = GetRegions()); }
        }

        protected bool IsVisibleMigration
        {
            get
            {
                return (ConfigurationManager.AppSettings["web.migration.status"] == "true") && TransferRegions.Count > 1;
            }
        }

        protected bool EnableMigration
        {
            get
            {
                var currentUser = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                return IsSecretEmail(currentUser.Email) || currentUser.IsOwner() && !TenantExtra.GetTenantQuota().Trial;
            }
        }

        protected bool IsSecretEmail(string email)
        {
            var s = ConfigurationManager.AppSettings["web.autotest.secret-email"];
            return !string.IsNullOrEmpty(s) && s.Split(new[] {',', ';', ' '}, StringSplitOptions.RemoveEmptyEntries).Contains(email, StringComparer.CurrentCultureIgnoreCase);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(TransferPortal), Page);

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/TransferPortal/js/transferportal.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/transferportal/css/transferportal.less"));

            popupTransferStart.Options.IsPopup = true;
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string StartTransfer(string targetRegion, bool notify, bool backupmail)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (!EnableMigration)
                return null;

            if (CurrentRegion.Equals(targetRegion, StringComparison.OrdinalIgnoreCase))
                return ToJsonError(Resource.ErrorTransferPortalInRegion);

            try
            {
                using (var backupClient = new BackupServiceClient())
                {
                    var transferRequest = new TransferRequest
                        {
                            TenantId = TenantProvider.CurrentTenantID,
                            TargetRegion = targetRegion,
                            NotifyUsers = notify,
                            BackupMail = backupmail
                        };

                    return ToJsonSuccess(backupClient.TransferPortal(transferRequest));
                }
            }
            catch (Exception error)
            {
                return ToJsonError(error.Message);
            }
        }

        [AjaxMethod(HttpSessionStateRequirement.ReadWrite)]
        public string CheckTransferProgress(string tenantID)
        {
            SecurityContext.DemandPermissions(SecutiryConstants.EditPortalSettings);
            if (string.IsNullOrEmpty(tenantID))
                return ToJsonSuccess(new BackupResult());

            try
            {
                using (var backupClient = new BackupServiceClient())
                {
                    BackupResult status = backupClient.GetTransferStatus(Convert.ToInt32(tenantID));
                    return ToJsonSuccess(status);
                }
            }
            catch (Exception error)
            {
                return ToJsonError(error.Message);
            }
        }

        private static string ToJsonSuccess(BackupResult result)
        {
            var data = new { result.Completed, result.Id, result.Percent };
            return JsonConvert.SerializeObject(new { success = true, data });
        }

        private static string ToJsonError(string message)
        {
            return JsonConvert.SerializeObject(new { success = false, data = message });
        }

        private static List<TransferRegionWithName> GetRegions()
        {
            try
            {
                using (var backupClient = new BackupServiceClient())
                {
                    return backupClient.GetTransferRegions()
                                       .Select(x => new TransferRegionWithName
                                           {
                                               Name = x.Name,
                                               IsCurrentRegion = x.IsCurrentRegion,
                                               BaseDomain = x.BaseDomain,
                                               FullName = TransferResourceHelper.GetRegionDescription(x.Name)
                                           })
                                       .ToList();
                }
            }
            catch
            {
                return new List<TransferRegionWithName>();
            }
        }
    }
}