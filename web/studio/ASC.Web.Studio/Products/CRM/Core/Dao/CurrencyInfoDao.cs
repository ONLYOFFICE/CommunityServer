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


#region Import

using System;
using System.Collections.Generic;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CurrencyInfoDao : AbstractDao
    {
        public CurrencyInfoDao(int tenantID)
            : base(tenantID)
        {
        }

        public virtual List<CurrencyInfo> GetAll()
        {
            return Db.ExecuteList(GetSqlQuery(null)).ConvertAll(ToCurrencyInfo);
        }
        
        public virtual CurrencyInfo GetByAbbreviation(string abbreviation)
        {
            var currencies = Db.ExecuteList(GetSqlQuery(Exp.Eq("abbreviation", abbreviation))).ConvertAll(ToCurrencyInfo);

            return currencies.Count > 0 ? currencies[0] : null;
        }

        public List<CurrencyInfo> GetBasic()
        {
            return Db.ExecuteList(GetSqlQuery(Exp.Eq("is_basic", true))).ConvertAll(ToCurrencyInfo);
        }

        public List<CurrencyInfo> GetOther()
        {
            return Db.ExecuteList(GetSqlQuery(Exp.Eq("is_basic", false))).ConvertAll(ToCurrencyInfo);
        }

        private SqlQuery GetSqlQuery(Exp where)
        {
            var sqlQuery = new SqlQuery("crm_currency_info")
                .Select("resource_key",
                        "abbreviation",
                        "symbol",
                        "culture_name",
                        "is_convertable",
                        "is_basic");

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        private static CurrencyInfo ToCurrencyInfo(object[] row)
        {
            return new CurrencyInfo(
                    Convert.ToString(row[0]),
                    Convert.ToString(row[1]),
                    Convert.ToString(row[2]),
                    Convert.ToString(row[3]),
                    Convert.ToBoolean(row[4]),
                    Convert.ToBoolean(row[5])
                );
        }
    }
}