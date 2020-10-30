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
using ASC.Common.Logging;
using ASC.Data.Storage;

namespace ASC.Mail.Core.Engine
{
    public class QuotaEngine
    {
        public int Tenant { get; private set; }
        public ILog Log { get; private set; }

        public QuotaEngine(int tenant, ILog log = null)
        {
            Tenant = tenant;
            Log = log ?? LogManager.GetLogger("ASC.Mail.QuotaEngine");
        }

        public void QuotaUsedAdd(long usedQuota)
        {
            try
            {
                var quotaController = new TenantQuotaController(Tenant);
                quotaController.QuotaUsedAdd(Defines.MODULE_NAME, string.Empty, Defines.MAIL_QUOTA_TAG, usedQuota);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("QuotaUsedAdd with params: tenant={0}, used_quota={1}", Tenant, usedQuota), ex);

                throw;
            }
        }

        public void QuotaUsedDelete(long usedQuota)
        {
            try
            {
                var quotaController = new TenantQuotaController(Tenant);
                quotaController.QuotaUsedDelete(Defines.MODULE_NAME, string.Empty, Defines.MAIL_QUOTA_TAG, usedQuota);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("QuotaUsedDelete with params: tenant={0}, used_quota={1}", Tenant, usedQuota), ex);

                throw;
            }
        }
    }
}
