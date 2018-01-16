/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Core.Tenants;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.UserControls.Statistics;

namespace ASC.Api.Settings
{
    [DataContract(Name = "quota", Namespace = "")]
    public class QuotaWrapper
    {
        [DataMember]
        public ulong StorageSize { get; set; }

        [DataMember]
        public ulong MaxFileSize { get; set; }

        [DataMember]
        public ulong UsedSize { get; set; }

        [DataMember]
        public int MaxUsersCount { get; set; }

        [DataMember]
        public int UsersCount { get; set; }

        [DataMember]
        public ulong AvailableSize
        {
            get { return Math.Max(0, StorageSize - UsedSize); }
        }

        [DataMember]
        public int AvailableUsersCount
        {
            get { return Math.Max(0, MaxUsersCount - UsersCount); }
        }

        [DataMember]
        public IList<QuotaUsage> StorageUsage { get; set; }

        private QuotaWrapper()
        {
        }

        public QuotaWrapper(TenantQuota quota, IList<TenantQuotaRow> quotaRows)
        {
            StorageSize = (ulong)Math.Max(0, quota.MaxTotalSize);
            UsedSize = (ulong)Math.Max(0, quotaRows.Sum(r => r.Counter));
            MaxFileSize = Math.Min(AvailableSize, (ulong)quota.MaxFileSize);
            MaxUsersCount = TenantExtra.GetTenantQuota().ActiveUsers;
            UsersCount = CoreContext.Configuration.Personal ? 1 : TenantStatisticsProvider.GetUsersCount();

            StorageUsage = quotaRows
                .Select(x => new QuotaUsage {Path = x.Path.TrimStart('/').TrimEnd('/'), Size = x.Counter,})
                .ToList();
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
                            new QuotaUsage {Size = 100 * 1024 * 1024, Path = "crm"},
                            new QuotaUsage {Size = 150 * 1024 * 1024, Path = "files"}
                        }
                };
        }
    }
}