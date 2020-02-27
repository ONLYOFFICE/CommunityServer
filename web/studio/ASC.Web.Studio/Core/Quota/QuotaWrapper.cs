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
using System.Runtime.Serialization;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Statistics;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Studio.Core.Quota
{
    [DataContract(Name = "quota", Namespace = "")]
    public class QuotaWrapper
    {
        [DataMember(Name = "storageSize")]
        public ulong StorageSize { get; set; }

        [DataMember(Name = "maxFileSize")]
        public ulong MaxFileSize { get; set; }

        [DataMember(Name = "usedSize")]
        public ulong UsedSize { get; set; }

        [DataMember(Name = "maxUsersCount")]
        public int MaxUsersCount { get; set; }

        [DataMember(Name = "usersCount")]
        public int UsersCount { get; set; }

        [DataMember(Name = "availableSize")]
        public ulong AvailableSize
        {
            get { return Math.Max(0, StorageSize > UsedSize ? StorageSize - UsedSize : 0); }
            set { throw new NotImplementedException(); }
        }

        [DataMember(Name = "availableUsersCount")]
        public int AvailableUsersCount
        {
            get { return Math.Max(0, MaxUsersCount - UsersCount); }
            set { throw new NotImplementedException(); }
        }

        [DataMember(Name = "storageUsage")]
        public IList<QuotaUsage> StorageUsage { get; set; }

        [DataMember(Name = "userStorageSize")]
        public long UserStorageSize { get; set; }

        [DataMember(Name = "userUsedSize")]
        public long UserUsedSize { get; set; }

        [DataMember(Name = "userAvailableSize")]
        public long UserAvailableSize
        {
            get { return Math.Max(0, UserStorageSize - UserUsedSize); }
            set { throw new NotImplementedException(); }
        }


        public static QuotaWrapper GetCurrent()
        {
            var quota = TenantExtra.GetTenantQuota();
            var quotaRows = TenantStatisticsProvider.GetQuotaRows(TenantProvider.CurrentTenantID).ToList();

            var result = new QuotaWrapper
                {
                    StorageSize = (ulong)Math.Max(0, quota.MaxTotalSize),
                    UsedSize = (ulong)Math.Max(0, quotaRows.Sum(r => r.Counter)),
                    MaxUsersCount = TenantExtra.GetTenantQuota().ActiveUsers,
                    UsersCount = CoreContext.Configuration.Personal ? 1 : TenantStatisticsProvider.GetUsersCount(),

                    StorageUsage = quotaRows
                        .Select(x => new QuotaUsage { Path = x.Path.TrimStart('/').TrimEnd('/'), Size = x.Counter, })
                        .ToList()
                };

            if (CoreContext.Configuration.Personal && SetupInfo.IsVisibleSettings("PersonalMaxSpace"))
            {
                result.UserStorageSize = CoreContext.Configuration.PersonalMaxSpace;

                var webItem = WebItemManager.Instance[WebItemManager.DocumentsProductID];
                var spaceUsageManager = webItem.Context.SpaceUsageStatManager as IUserSpaceUsage;
                if (spaceUsageManager != null)
                    result.UserUsedSize = spaceUsageManager.GetUserSpaceUsage(SecurityContext.CurrentAccount.ID);
            }

            result.MaxFileSize = Math.Min(result.AvailableSize, (ulong)quota.MaxFileSize);
            return result;
        }

        public static QuotaWrapper GetSample()
        {
            return new QuotaWrapper
                {
                    MaxFileSize = 25 * 1024 * 1024,
                    StorageSize = 1024 * 1024 * 1024,
                    UsedSize = 250 * 1024 * 1024,
                    StorageUsage = new List<QuotaUsage>
                        {
                            new QuotaUsage { Size = 100*1024*1024, Path = "crm" },
                            new QuotaUsage { Size = 150*1024*1024, Path = "files" }
                        }
                };
        }


        [DataContract(Name = "quota_usage", Namespace = "")]
        public class QuotaUsage
        {
            [DataMember(Name = "path")]
            public string Path { get; set; }

            [DataMember(Name = "size")]
            public long Size { get; set; }
        }
    }
}