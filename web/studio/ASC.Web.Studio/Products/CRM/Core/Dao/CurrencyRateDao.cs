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
using ASC.Core;

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CurrencyRateDao : AbstractDao
    {
        public CurrencyRateDao(int tenantID)
            : base(tenantID)
        {
        }

        public virtual List<CurrencyRate> GetAll()
        {
            return Db.ExecuteList(GetSqlQuery(null)).ConvertAll(ToCurrencyRate);
        }

        public virtual CurrencyRate GetByID(int id)
        {
            var rates = Db.ExecuteList(GetSqlQuery(Exp.Eq("id", id))).ConvertAll(ToCurrencyRate);

            return rates.Count > 0 ? rates[0] : null;
        }

        public CurrencyRate GetByCurrencies(string fromCurrency, string toCurrency)
        {
            var rates = Db.ExecuteList(GetSqlQuery(Exp.Eq("from_currency", fromCurrency.ToUpper()) & Exp.Eq("to_currency", toCurrency.ToUpper())))
                .ConvertAll(ToCurrencyRate);
                
            return rates.Count > 0 ? rates[0] : null;
        }

        public int SaveOrUpdate(CurrencyRate currencyRate)
        {
            if (String.IsNullOrEmpty(currencyRate.FromCurrency) || String.IsNullOrEmpty(currencyRate.ToCurrency) || currencyRate.Rate < 0)
                throw new ArgumentException();

            if (currencyRate.ID > 0 && currencyRate.Rate == 0)
                return Delete(currencyRate.ID);

            if (Db.ExecuteScalar<int>(Query("crm_currency_rate").SelectCount().Where(Exp.Eq("id", currencyRate.ID))) == 0)
            {
                var query = Insert("crm_currency_rate")
                    .InColumnValue("id", 0)
                    .InColumnValue("from_currency", currencyRate.FromCurrency.ToUpper())
                    .InColumnValue("to_currency", currencyRate.ToCurrency.ToUpper())
                    .InColumnValue("rate", currencyRate.Rate)
                    .InColumnValue("create_by", SecurityContext.CurrentAccount.ID)
                    .InColumnValue("create_on", DateTime.UtcNow)
                    .InColumnValue("last_modifed_by", SecurityContext.CurrentAccount.ID)
                    .InColumnValue("last_modifed_on", DateTime.UtcNow)
                    .Identity(1, 0, true);

                currencyRate.ID = Db.ExecuteScalar<int>(query);
            }
            else
            {
                Db.ExecuteNonQuery(
                    Update("crm_currency_rate")
                        .Set("from_currency", currencyRate.FromCurrency.ToUpper())
                        .Set("to_currency", currencyRate.ToCurrency.ToUpper())
                        .Set("rate", currencyRate.Rate)
                        .Set("last_modifed_on", DateTime.UtcNow)
                        .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                        .Where(Exp.Eq("id", currencyRate.ID)));
            }

            return currencyRate.ID;
        }

        public int Delete(int id)
        {
            if (id <= 0) throw new ArgumentException();

            var sqlQuery = Delete("crm_currency_rate")
                .Where(Exp.Eq("id", id));

            Db.ExecuteNonQuery(sqlQuery);

            return id;
        }

        public List<CurrencyRate> SetCurrencyRates(List<CurrencyRate> rates)
        {
            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Delete("crm_currency_rate"));
                
                foreach (var rate in rates)
                {
                    var query = Insert("crm_currency_rate")
                        .InColumnValue("id", 0)
                        .InColumnValue("from_currency", rate.FromCurrency.ToUpper())
                        .InColumnValue("to_currency", rate.ToCurrency.ToUpper())
                        .InColumnValue("rate", rate.Rate)
                        .InColumnValue("create_by", SecurityContext.CurrentAccount.ID)
                        .InColumnValue("create_on", DateTime.UtcNow)
                        .InColumnValue("last_modifed_by", SecurityContext.CurrentAccount.ID)
                        .InColumnValue("last_modifed_on", DateTime.UtcNow)
                        .Identity(1, 0, true);

                    rate.ID = Db.ExecuteScalar<int>(query);
                }

                tx.Commit();

                return rates;
            }
        }

        private SqlQuery GetSqlQuery(Exp where)
        {
            var sqlQuery = Query("crm_currency_rate")
                .Select("id",
                        "from_currency",
                        "to_currency",
                        "rate");

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        private static CurrencyRate ToCurrencyRate(object[] row)
        {
            return new CurrencyRate{
                    ID = Convert.ToInt32(row[0]),
                    FromCurrency = Convert.ToString(row[1]),
                    ToCurrency = Convert.ToString(row[2]),
                    Rate = Convert.ToDecimal(row[3])
                };
        }
    }
}