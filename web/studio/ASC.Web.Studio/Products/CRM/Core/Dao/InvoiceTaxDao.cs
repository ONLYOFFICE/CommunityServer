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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;

namespace ASC.CRM.Core.Dao
{
    public class CachedInvoiceTaxDao : InvoiceTaxDao
    {
        private readonly HttpRequestDictionary<InvoiceTax> _invoiceTaxCache = new HttpRequestDictionary<InvoiceTax>("crm_invoice_tax");

        public CachedInvoiceTaxDao(int tenantID)
            : base(tenantID)
        {
        }

        public override InvoiceTax GetByID(int invoiceTaxID)
        {
            return _invoiceTaxCache.Get(invoiceTaxID.ToString(CultureInfo.InvariantCulture), () => GetByIDBase(invoiceTaxID));
        }

        private InvoiceTax GetByIDBase(int invoiceTaxID)
        {
            return base.GetByID(invoiceTaxID);
        }

        public override InvoiceTax SaveOrUpdateInvoiceTax(InvoiceTax invoiceTax)
        {
            if (invoiceTax != null && invoiceTax.ID > 0)
                ResetCache(invoiceTax.ID);

            return base.SaveOrUpdateInvoiceTax(invoiceTax);
        }

        public override InvoiceTax DeleteInvoiceTax(int invoiceTaxID)
        {
            ResetCache(invoiceTaxID);

            return base.DeleteInvoiceTax(invoiceTaxID);
        }

        private void ResetCache(int invoiceTaxID)
        {
            _invoiceTaxCache.Reset(invoiceTaxID.ToString(CultureInfo.InvariantCulture));
        }
    }

    public class InvoiceTaxDao : AbstractDao
    {
        public InvoiceTaxDao(int tenantID)
            : base(tenantID)
        {
        }


        public Boolean IsExist(int invoiceTaxID)
        {
            return Db.ExecuteScalar<bool>(
                    @"select exists(select 1 from crm_invoice_tax where tenant_id = @tid and id = @id)",
                    new {tid = TenantID, id = invoiceTaxID});
        }

        public Boolean IsExist(String invoiceName)
        {
            var q = new SqlQuery("crm_invoice_tax")
                .Select("1")
                .Where("tenant_id", TenantID)
                .Where("name", invoiceName)
                .SetMaxResults(1);

            return Db.ExecuteScalar<bool>(q);
        }

        public Boolean CanDelete(int invoiceTaxID)
        {
            var count1 = Db.ExecuteScalar<int>(@"select count(*) from crm_invoice_item where tenant_id = @tid and (invoice_tax1_id = @id or invoice_tax2_id = @id)",
                new { tid = TenantID, id = invoiceTaxID });
            var count2 = Db.ExecuteScalar<int>(@"select count(*) from crm_invoice_line where tenant_id = @tid and (invoice_tax1_id = @id or invoice_tax2_id = @id)", 
                new { tid = TenantID, id = invoiceTaxID });

            return count1 == 0 && count2 == 0;
        }

        #region Get

        public virtual List<InvoiceTax> GetAll()
        {
            return Db.ExecuteList(GetInvoiceTaxSqlQuery(null)).ConvertAll(ToInvoiceTax);
        }

        public DateTime GetMaxLastModified()
        {
            return Db.ExecuteScalar<DateTime>(Query("crm_invoice_tax").Select("last_modifed_on"));
        }

        public virtual List<InvoiceTax> GetByID(int[] ids)
        {
            return Db.ExecuteList(GetInvoiceTaxSqlQuery(Exp.In("id", ids))).ConvertAll(ToInvoiceTax);
        }

        public virtual InvoiceTax GetByID(int id)
        {
            var invoiceTaxes = Db.ExecuteList(GetInvoiceTaxSqlQuery(Exp.Eq("id", id))).ConvertAll(ToInvoiceTax);

            return invoiceTaxes.Count > 0 ? invoiceTaxes[0] : null;
        }

        #endregion

        #region SaveOrUpdate

        public virtual InvoiceTax SaveOrUpdateInvoiceTax(InvoiceTax invoiceTax)
        {
            /*_cache.Remove(_invoiceItemCacheKey);
            _cache.Insert(_invoiceTaxCacheKey, String.Empty);*/

            return SaveOrUpdateInvoiceTaxInDb(invoiceTax);
        }

        private InvoiceTax SaveOrUpdateInvoiceTaxInDb(InvoiceTax invoiceTax)
        {
            if (String.IsNullOrEmpty(invoiceTax.Name))
                throw new ArgumentException();

            invoiceTax.LastModifedBy = SecurityContext.CurrentAccount.ID;
            invoiceTax.LastModifedOn = DateTime.UtcNow;

            if (Db.ExecuteScalar<int>(Query("crm_invoice_tax").SelectCount().Where(Exp.Eq("id", invoiceTax.ID))) == 0)
            {
                invoiceTax.CreateOn = DateTime.UtcNow;
                invoiceTax.CreateBy = SecurityContext.CurrentAccount.ID;

                invoiceTax.ID = Db.ExecuteScalar<int>(
                               Insert("crm_invoice_tax")
                              .InColumnValue("id", 0)
                              .InColumnValue("name", invoiceTax.Name)
                              .InColumnValue("description", invoiceTax.Description)
                              .InColumnValue("rate", invoiceTax.Rate)
                              .InColumnValue("create_on", invoiceTax.CreateOn)
                              .InColumnValue("create_by", SecurityContext.CurrentAccount.ID)
                              .InColumnValue("last_modifed_on", invoiceTax.LastModifedOn)
                              .InColumnValue("last_modifed_by", invoiceTax.LastModifedBy)
                              .Identity(1, 0, true));
            }
            else
            {
                var oldInvoiceTax = Db.ExecuteList(GetInvoiceTaxSqlQuery(Exp.Eq("id", invoiceTax.ID)))
                    .ConvertAll(ToInvoiceTax)
                    .FirstOrDefault();

                CRMSecurity.DemandEdit(oldInvoiceTax);

                Db.ExecuteNonQuery(
                    Update("crm_invoice_tax")
                        .Set("name", invoiceTax.Name)
                        .Set("description", invoiceTax.Description)
                        .Set("rate", invoiceTax.Rate)
                        .Set("last_modifed_on", invoiceTax.LastModifedOn)
                        .Set("last_modifed_by", invoiceTax.LastModifedBy)
                        .Where(Exp.Eq("id", invoiceTax.ID)));
            }

            return invoiceTax;
        }

        #endregion


        #region Delete

        public virtual InvoiceTax DeleteInvoiceTax(int invoiceTaxID)
        {
            var invoiceTax = GetByID(invoiceTaxID);

            if (invoiceTax == null) return null;

            CRMSecurity.DemandDelete(invoiceTax);

            Db.ExecuteNonQuery(Delete("crm_invoice_tax").Where("id", invoiceTaxID));

           /* _cache.Remove(_invoiceItemCacheKey);
            _cache.Insert(_invoiceTaxCacheKey, String.Empty);*/
            return invoiceTax;
        }

        #endregion


        #region Private Methods

        private static InvoiceTax ToInvoiceTax(object[] row)
        {
            return new InvoiceTax
                {
                    ID = Convert.ToInt32(row[0]),
                    Name = Convert.ToString(row[1]),
                    Description = Convert.ToString(row[2]),
                    Rate = Convert.ToDecimal(row[3]),
                    CreateOn = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[4].ToString())),
                    CreateBy = ToGuid(row[5]),
                    LastModifedOn = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[6].ToString())),
                    LastModifedBy = ToGuid(row[7])
                };
        }

        private SqlQuery GetInvoiceTaxSqlQuery(Exp where)
        {
            var sqlQuery = Query("crm_invoice_tax")
                .Select(
                    "id",
                    "name",
                    "description",
                    "rate",
                    "create_on",
                    "create_by",
                    "last_modifed_on",
                    "last_modifed_by");

            if (where != null)
            {
                sqlQuery.Where(where);
            }

            return sqlQuery;
        }

        #endregion
    }
}