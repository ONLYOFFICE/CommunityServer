/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using ASC.Core.Users;

namespace ASC.Data.Storage
{
    public class TenantQuotaController : IQuotaController
    {
        private readonly int _tenant;
        private long _currentSize;

        public TenantQuotaController(int tenant)
        {
            _tenant = tenant;
            _currentSize = CoreContext.TenantManager.FindTenantQuotaRows(tenant)
                                      .Where(r => UsedInQuota(r.Tag))
                                      .Sum(r => r.Counter);
        }

        #region IQuotaController Members
        public void QuotaUsedAdd(string module, string domain, string dataTag, long size, bool quotaCheckFileSize = true)
        {
            QuotaUsedAdd(module, domain, dataTag, size, Guid.Empty, quotaCheckFileSize);
        }
        public void QuotaUsedAdd(string module, string domain, string dataTag, long size, Guid ownerId, bool quotaCheckFileSize)
        {
            size = Math.Abs(size);
            if (UsedInQuota(dataTag))
            {
                QuotaUsedCheck(size, quotaCheckFileSize, ownerId);
                Interlocked.Add(ref _currentSize, size);
            }

            SetTenantQuotaRow(module, domain, size, dataTag, true, Guid.Empty);
            if (ownerId != Core.Configuration.Constants.CoreSystem.ID)
            {
                SetTenantQuotaRow(module, domain, size, dataTag, true, ownerId != Guid.Empty ? ownerId : SecurityContext.CurrentAccount.ID);
            }

        }

        public void QuotaUsedDelete(string module, string domain, string dataTag, long size)
        {
            QuotaUsedDelete(module, domain, dataTag, size, Guid.Empty); 
        }
        public void QuotaUsedDelete(string module, string domain, string dataTag, long size, Guid ownerId)
        {
            size = -Math.Abs(size);
            if (UsedInQuota(dataTag))
            {
                Interlocked.Add(ref _currentSize, size);
            }

            SetTenantQuotaRow(module, domain, size, dataTag, true, Guid.Empty);
            if (ownerId != Core.Configuration.Constants.CoreSystem.ID)
            {
                SetTenantQuotaRow(module, domain, size, dataTag, true, ownerId != Guid.Empty ? ownerId : SecurityContext.CurrentAccount.ID);
            }
        }

        public void QuotaUsedSet(string module, string domain, string dataTag, long size)
        {
            size = Math.Max(0, size);
            if (UsedInQuota(dataTag))
            {
                Interlocked.Exchange(ref _currentSize, size);
            }
            SetTenantQuotaRow(module, domain, size, dataTag, false, Guid.Empty);
        }

        public void QuotaUsedCheck(long size, Guid ownedId)
        {
            QuotaUsedCheck(size, true, ownedId);
        }

        public void QuotaUsedCheck(long size, bool quotaCheckFileSize, Guid ownedId)
        {
            var quota = CoreContext.TenantManager.GetTenantQuota(_tenant);
            if (quota != null)
            {
                if (quotaCheckFileSize && quota.MaxFileSize != 0 && quota.MaxFileSize < size)
                {
                    throw new TenantQuotaException(string.Format("Exceeds the maximum file size ({0}MB)", BytesToMegabytes(quota.MaxFileSize)));
                }

                if (CoreContext.Configuration.Standalone)
                {
                    var tenantQuotaSettings = TenantQuotaSettings.Load();
                    if (!tenantQuotaSettings.DisableQuota)
                    {
                        if (quota.MaxTotalSize != 0 && quota.MaxTotalSize < _currentSize + size)
                        {
                            throw new TenantQuotaException(string.Format("Exceeded maximum amount of disk quota ({0}MB)", BytesToMegabytes(quota.MaxTotalSize)));
                        }
                    }
                }
                else
                {
                    if (quota.MaxTotalSize != 0 && quota.MaxTotalSize < _currentSize + size)
                    {
                        throw new TenantQuotaException(string.Format("Exceeded maximum amount of disk quota ({0}MB)", BytesToMegabytes(quota.MaxTotalSize)));
                    }
                }
            }
            var quotaSettings = TenantUserQuotaSettings.Load();

            if (quotaSettings.EnableUserQuota)
            {
                var userQuotaSettings = UserQuotaSettings.LoadForUser(ownedId);
                var quotaLimit = userQuotaSettings.UserQuota;

                if (quotaLimit != -1)
                {
                    var userUsedSpace = Math.Max(0, CoreContext.TenantManager.FindUserQuotaRows(_tenant, ownedId).Where(r => !string.IsNullOrEmpty(r.Tag)).Where(r => r.Tag != Guid.Empty.ToString()).Sum(r => r.Counter));

                    if (quotaLimit - userUsedSpace < size)
                    {
                        throw new TenantQuotaException(string.Format("Exceeds the maximum file size ({0}MB)", BytesToMegabytes(quotaLimit)));
                    }
                }
            }
        }

        #endregion

        public long QuotaCurrentGet()
        {
            return _currentSize;
        }

        private void SetTenantQuotaRow(string module, string domain, long size, string dataTag, bool exchange, Guid userId)
        {
            CoreContext.TenantManager.SetTenantQuotaRow(
                new TenantQuotaRow { Tenant = _tenant, Path = string.Format("/{0}/{1}", module, domain), Counter = size, Tag = dataTag, UserId = userId},
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