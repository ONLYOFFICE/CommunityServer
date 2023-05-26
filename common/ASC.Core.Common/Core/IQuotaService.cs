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
using System.Collections.Generic;

using ASC.Core.Tenants;

namespace ASC.Core
{
    public interface IQuotaService
    {
        IEnumerable<TenantQuota> GetTenantQuotas(bool useCache = true);

        TenantQuota GetTenantQuota(int id, bool useCache = true);

        TenantQuota SaveTenantQuota(TenantQuota quota);

        void RemoveTenantQuota(int id);


        IEnumerable<TenantQuotaRow> FindTenantQuotaRows(int tenantId);
        IEnumerable<TenantQuotaRow> FindUserQuotaRows(int tenantId, Guid userId, bool useCache);
        
        TenantQuotaRow FindUserQuotaRow(int tenantId, Guid userId, Guid tag);
        void SetTenantQuotaRow(TenantQuotaRow row, bool exchange);
    }
}
