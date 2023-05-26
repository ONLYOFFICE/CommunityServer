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

using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Common.Radicale.Core
{
    public class DbRadicale
    {
        private IDbManager db = new DbManager("default");
        public void SaveCardDavUser(int tenant, string id)
        {
            var i = new SqlInsert("core_userdav").ReplaceExists(true)
                    .InColumnValue("tenant_id", tenant)
                    .InColumnValue("user_id", id);

            db.ExecuteNonQuery(i);
        }

        public void RemoveCardDavUser(int tenant, string id)
        {
            var i = new SqlDelete("core_userdav").Where("user_id", id).Where("tenant_id", tenant);
            db.ExecuteNonQuery(i);
        }

        public Boolean IsExistCardDavUser(int tenant, string id)
        {
            var q = new SqlQuery("core_userdav")
                .Select("1")
                .Where("user_id", id)
                .Where("tenant_id", tenant)
                .SetMaxResults(1);

            return db.ExecuteScalar<bool>(q);
        }

    }
}
