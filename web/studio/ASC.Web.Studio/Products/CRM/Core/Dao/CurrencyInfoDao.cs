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

#endregion

namespace ASC.CRM.Core.Dao
{
    public class CurrencyInfoDao : AbstractDao
    {
        public CurrencyInfoDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }

        public virtual List<CurrencyInfo> GetAll()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetSqlQuery(null)).ConvertAll(ToCurrencyInfo);
            }
        }
        
        public virtual CurrencyInfo GetByAbbreviation(string abbreviation)
        {
            using (var db = GetDb())
            {
                var currencies = db.ExecuteList(GetSqlQuery(Exp.Eq("abbreviation", abbreviation))).ConvertAll(ToCurrencyInfo);

                return currencies.Count > 0 ? currencies[0] : null;
            }
        }

        public List<CurrencyInfo> GetBasic()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetSqlQuery(Exp.Eq("is_basic", true))).ConvertAll(ToCurrencyInfo);
            }
        }

        public List<CurrencyInfo> GetOther()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetSqlQuery(Exp.Eq("is_basic", false))).ConvertAll(ToCurrencyInfo);
            }
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