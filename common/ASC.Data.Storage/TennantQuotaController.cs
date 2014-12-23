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

        #endregion

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