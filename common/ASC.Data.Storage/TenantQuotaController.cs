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
using System.Linq;
using System.Threading;
using ASC.Core;
using ASC.Core.Tenants;

namespace ASC.Data.Storage
{
    public class TenantQuotaController : IQuotaController
    {
        private readonly int _tenant;
        private long _currentSize;


        public TenantQuotaController(int tenant)
        {
            _tenant = tenant;
            _currentSize = CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(tenant))
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
                Interlocked.Add(ref _currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, true);
        }

        public void QuotaUsedDelete(string module, string domain, string dataTag, long size)
        {
            size = -Math.Abs(size);
            if (UsedInQuota(dataTag))
            {
                Interlocked.Add(ref _currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, true);
        }

        public void QuotaUsedSet(string module, string domain, string dataTag, long size)
        {
            size = Math.Max(0, size);
            if (UsedInQuota(dataTag))
            {
                Interlocked.Exchange(ref _currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, false);
        }

        public long QuotaUsedGet(string module, string domain)
        {
            var path = string.IsNullOrEmpty(module) ? null : string.Format("/{0}/{1}", module, domain);
            return CoreContext.TenantManager.FindTenantQuotaRows(new TenantQuotaRowQuery(_tenant).WithPath(path))
                              .Where(r => UsedInQuota(r.Tag))
                              .Sum(r => r.Counter);
        }

        public void QuotaUsedCheck(long size)
        {
            var quota = CoreContext.TenantManager.GetTenantQuota(_tenant);
            if (quota != null)
            {
                if (quota.MaxFileSize != 0 && quota.MaxFileSize < size)
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum file size ({0}MB)", BytesToMegabytes(quota.MaxFileSize)));
                }
                if (quota.MaxTotalSize != 0 && quota.MaxTotalSize < _currentSize + size)
                {
                    throw new TenantQuotaException(string.Format("Exceeded maximum amount of disk quota ({0}MB)", BytesToMegabytes(quota.MaxTotalSize)));
                }
            }
        }

        #endregion

        public long QuotaCurrentGet()
        {
            return _currentSize;
        }

        private void SetTenantQuotaRow(string module, string domain, long size, string dataTag, bool exchange)
        {
            CoreContext.TenantManager.SetTenantQuotaRow(
                new TenantQuotaRow { Tenant = _tenant, Path = string.Format("/{0}/{1}", module, domain), Counter = size, Tag = dataTag },
                exchange);
        }

        private static bool UsedInQuota(string tag)
        {
            return !string.IsNullOrEmpty(tag) && new Guid(tag) != Guid.Empty;
        }

        private static double BytesToMegabytes(long bytes)
        {
            return Math.Round(bytes / 1024d / 1024d, 1);
        }
    }
}