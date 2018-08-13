/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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