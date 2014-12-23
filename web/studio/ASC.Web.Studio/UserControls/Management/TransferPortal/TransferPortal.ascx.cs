/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
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
using System.Text.RegularExpressions;
using System.Web;
using ASC.Web.Studio.Core.Backup;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Migration, Location)]
    [AjaxNamespace("TransferPortal")]
    public partial class TransferPortal : UserControl
    {
        public const string Location = "~/UserControls/Management/TransferPortal/TransferPortal.ascx";

        protected class TransferRegionWithName : TransferRegion
        {
            public string FullName { get; set; }
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
                return SetupInfo.IsSecretEmail(currentUser.Email) || currentUser.IsOwner() && !TenantExtra.GetTenantQuota().Trial;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof(BackupAjaxHandler), Page);

            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/TransferPortal/js/transferportal.js"));
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/usercontrols/management/transferportal/css/transferportal.less"));

            popupTransferStart.Options.IsPopup = true;
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