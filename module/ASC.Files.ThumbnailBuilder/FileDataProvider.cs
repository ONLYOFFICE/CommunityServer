/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
using System.Linq;

using ASC.Common.Caching;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.Files.Core;

namespace ASC.Files.ThumbnailBuilder
{
    internal class FileDataProvider
    {
        private readonly ConfigSection config;
        private readonly ICache cache;
        private readonly string cacheKey;

        public FileDataProvider(ConfigSection configSection, ICache ascCache)
        {
            config = configSection;
            cache = ascCache;
            cacheKey = "PremiumTenants";
        }

        public int[] GetPremiumTenants()
        {
            /*
            // v_premium_tenants view:
            select
                t.tenant,
                (now() < max(t.stamp)) AS premium

            from tenants_tariff t
            left join tenants_quota q ON (t.tariff = q.tenant)

            where
            (
                (
                    isnull(t.comment) or
                    (
                        (not((t.comment like '%non-profit%'))) and
                        (not((t.comment like '%test%'))) and
                        (not((t.comment like '%translate%'))) and
                        (not((t.comment like '%trial%')))
                    )
                ) and
                (not((q.features like '%free%'))) and
                (not((q.features like '%non-profit%'))) and
                (not((q.features like '%trial%')))
            )
            group by t.tenant
            */

            var result = cache.Get<int[]>(cacheKey);

            if (result != null)
            {
                return result;
            }

            var nullTariffComment = Exp.Eq("t.comment", null);

            var excludeTariffComments = new List<string> { "non-profit", "test", "translate", "trial" }
                .Aggregate(Exp.Empty, (current, item) => Exp.And(current, !Exp.Like("t.comment", item, SqlLike.AnyWhere)));

            var excludeQuotaFeatures = new List<string> { "free", "non-profit", "trial" }
                .Aggregate(Exp.Empty, (current, item) => Exp.And(current, !Exp.Like("q.features", item, SqlLike.AnyWhere)));

            var search = new SqlQuery("tenants_tariff t")
                .LeftOuterJoin("tenants_quota q", Exp.EqColumns("t.tariff", "q.tenant"))
                .Select("t.tenant", "(NOW() < MAX(t.stamp)) AS premium")
                .Where(
                    Exp.And(
                        Exp.Or(
                            nullTariffComment,
                            excludeTariffComments
                        ),
                        excludeQuotaFeatures
                    )
                )
                .GroupBy("t.tenant");

            using (var db = DbManager.FromHttpContext(config.ConnectionStringName))
            {
                result = db.ExecuteList(search)
                    .Where(row => Convert.ToBoolean(row[1]))
                    .Select(row => Convert.ToInt32(row[0]))
                    .ToArray();

                cache.Insert(cacheKey, result, DateTime.UtcNow.AddHours(1));

                return result;
            }
        }

        private IEnumerable<FileData> GetFileData(Exp where)
        {
            var search = new SqlQuery("files_file f")
                .Select("f.tenant_id, f.id")
                .Where("f.current_version", true)
                .Where("f.thumb", Thumbnail.Waiting)
                .Where("f.encrypted", false)
                .OrderBy("f.modified_on", false)
                .SetMaxResults(config.SqlMaxResults);

            search.InnerJoin("tenants_tenants t", Exp.EqColumns("f.tenant_id", "t.id"))
                .Where("t.status", TenantStatus.Active);

            if (where != null)
            {
                search.Where(where);
            }

            using (var db = DbManager.FromHttpContext(config.ConnectionStringName))
            {
                return db.ExecuteList(search).Select(row => new FileData(
                    Convert.ToInt32(row[0]),
                    row[1]
                ));
            }
        }

        public IEnumerable<FileData> GetFilesWithoutThumbnails()
        {
            IEnumerable<FileData> result;

            var premiumTenants = GetPremiumTenants();

            if (premiumTenants.Any())
            {
                result = GetFileData(Exp.In("f.tenant_id", premiumTenants));
                if (result.Any())
                {
                    return result;
                }
            }

            result = GetFileData(null);

            return result;
        }
    }
}
