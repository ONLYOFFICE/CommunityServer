/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Threading;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Data.Storage
{
    public class TennantQuotaController : IQuotaController
    {
        private readonly int tenant;
        private long currentSize;


        public TennantQuotaController(int tenant)
        {
            this.tenant = tenant;
            currentSize = CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(tenant))
                .Where(r => UsedInQuota(r.Tag))
                .Sum(r => r.Counter);
        }

        #region IQuotaController Members

        public void QuotaUsedAdd(string module, string domain, string dataTag, long size)
        {
            size = Math.Abs(size);
            if (UsedInQuota(dataTag))
            {
                QuotaUsedCheck(size);
                Interlocked.Add(ref currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, true);
        }

        public void QuotaUsedDelete(string module, string domain, string dataTag, long size)
        {
            size = -Math.Abs(size);
            if (UsedInQuota(dataTag))
            {
                Interlocked.Add(ref currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, true);
        }

        public void QuotaUsedSet(string module, string domain, string dataTag, long size)
        {
            size = Math.Max(0, size);
            if (UsedInQuota(dataTag))
            {
                Interlocked.Exchange(ref currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, false);
        }

        public long QuotaUsedGet(string module, string domain)
        {
            var path = string.IsNullOrEmpty(module) ? null : string.Format("/{0}/{1}", module, domain);
            return CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(tenant).WithPath(path))
                .Where(r => UsedInQuota(r.Tag))
                .Sum(r => r.Counter);
        }

        public void QuotaUsedCheck(long size)
        {
            var quota = CoreContext.TenantManager.GetTenantQuota(tenant);
            if (quota != null)
            {
                if (quota.MaxFileSize != 0 && quota.MaxFileSize < size)
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum file size ({0}MB)", BytesToMegabytes(quota.MaxFileSize)));
                }
                if (quota.MaxTotalSize != 0 && quota.MaxTotalSize < currentSize + size)
                {
                    throw new TenantQuotaException(string.Format("Exceeded maximum amount of disk quota ({0}MB)", BytesToMegabytes(quota.MaxTotalSize)));
                }
            }
        }

        #endregion

        public long QuotaCurrentGet()
        {
            return currentSize;
        }

        private void SetTenantQuotaRow(string module, string domain, long size, string dataTag, bool exchange)
        {
            CoreContext.TenantManager.SetTenantQuotaRow(
                new TenantQuotaRow { Tenant = tenant, Path = string.Format("/{0}/{1}", module, domain), Counter = size, Tag = dataTag },
                exchange);
        }

        private bool UsedInQuota(string tag)
        {
            return !string.IsNullOrEmpty(tag) && new Guid(tag) != Guid.Empty;
        }

        private double BytesToMegabytes(long bytes)
        {
            return Math.Round(bytes / 1024d / 1024d, 1);
        }
    }
}