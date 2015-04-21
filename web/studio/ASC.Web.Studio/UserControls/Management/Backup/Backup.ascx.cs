/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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


using Amazon;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Web.Studio.Core.Backup;
using ASC.Web.Studio.UserControls.Common.ChooseTimePeriod;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Management
{
    [ManagementControl(ManagementType.Backup, Location)]
    public partial class Backup : UserControl
    {
        public const string Location = "~/UserControls/Management/Backup/Backup.ascx";

        protected readonly long AvailableZipSize = 4 * 1024 * 1024 * 1024L;
        protected BackupAvailableSize AvailableStatus = BackupAvailableSize.Available;

        private readonly Guid _mailStorageTag = new Guid("666ceac1-4532-4f8c-9cba-8f510eca2fd1");
                
        
        protected bool EnableBackup
        {
            get { return (TenantExtra.GetTenantQuota().HasBackup); }
        }

        protected List<RegionEndpoint> Regions
        {
            get { return RegionEndpoint.EnumerableAllRegions.ToList(); }
        }

        
        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(typeof (BackupAjaxHandler), Page);

            Page.RegisterStyleControl(ResolveUrl("~/usercontrols/management/backup/css/backup.less"));
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/management/backup/js/backup.js"));

            FolderSelectorHolder.Controls.Add(LoadControl(CommonLinkUtility.ToAbsolute("~/products/files/controls/fileselector/fileselector.ascx")));

            BackupTimePeriod.Controls.Add(LoadControl(ChooseTimePeriod.Location));
            RestoreHolder.Controls.Add(LoadControl(Restore.Location));

            var size = CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(TenantProvider.CurrentTenantID))
                                  .Where(r => !string.IsNullOrEmpty(r.Tag) && new Guid(r.Tag) != Guid.Empty && !new Guid(r.Tag).Equals(_mailStorageTag))
                                  .Sum(r => r.Counter);
            if (size > AvailableZipSize)
            {
                AvailableStatus = BackupAvailableSize.NotAvailable;
            }
            else
            {
                size = TenantStatisticsProvider.GetUsedSize();
                if (size > AvailableZipSize)
                {
                    AvailableStatus = BackupAvailableSize.WithoutMail;
                }
            }
        }

        protected enum BackupAvailableSize
        {
            Available,
            WithoutMail,
            NotAvailable,
        }
    }
}