
using System;

using ASC.Common.Data;
using ASC.Common.Data.Sql;

namespace ASC.Common.Radicale.Core
{
    public class DbRadicale
    {
        private IDbManager db = DbManager.FromHttpContext("default");
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
