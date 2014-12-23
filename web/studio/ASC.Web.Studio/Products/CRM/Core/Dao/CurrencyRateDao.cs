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
        public CurrencyRateDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }

        public virtual List<CurrencyRate> GetAll()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetSqlQuery(null)).ConvertAll(ToCurrencyRate);
            }
        }

        public virtual CurrencyRate GetByID(int id)
        {
            using (var db = GetDb())
            {
                var rates = db.ExecuteList(GetSqlQuery(Exp.Eq("id", id))).ConvertAll(ToCurrencyRate);

                return rates.Count > 0 ? rates[0] : null;
            }
        }

        public CurrencyRate GetByCurrencies(string fromCurrency, string toCurrency)
        {
            using (var db = GetDb())
            {
                var rates = db.ExecuteList(GetSqlQuery(Exp.Eq("from_currency", fromCurrency.ToUpper()) & Exp.Eq("to_currency", toCurrency.ToUpper())))
                    .ConvertAll(ToCurrencyRate);
                
                return rates.Count > 0 ? rates[0] : null;
            }
        }

        public int SaveOrUpdate(CurrencyRate currencyRate)
        {
            using (var db = GetDb())
            {
                if (String.IsNullOrEmpty(currencyRate.FromCurrency) || String.IsNullOrEmpty(currencyRate.ToCurrency) || currencyRate.Rate < 0)
                    throw new ArgumentException();

                if (currencyRate.ID > 0 && currencyRate.Rate == 0)
                    return Delete(currencyRate.ID);

                if (db.ExecuteScalar<int>(Query("crm_currency_rate").SelectCount().Where(Exp.Eq("id", currencyRate.ID))) == 0)
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

                    currencyRate.ID = db.ExecuteScalar<int>(query);
                }
                else
                {
                    db.ExecuteNonQuery(
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
        }

        public int Delete(int id)
        {
            if (id <= 0) throw new ArgumentException();

            using (var db = GetDb())
            {
                var sqlQuery = Delete("crm_currency_rate")
                    .Where(Exp.Eq("id", id));

                db.ExecuteNonQuery(sqlQuery);

                return id;
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