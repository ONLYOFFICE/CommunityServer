/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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

        public CachedInvoiceTaxDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
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
        public InvoiceTaxDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }


        public Boolean IsExist(int invoiceTaxID)
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<bool>(@"select exists(select 1 from crm_invoice_tax where tenant_id = @tid and id = @id)",
                                new { tid = TenantID, id = invoiceTaxID });
            }
        }

        public Boolean IsExist(String invoiceName)
        {
            var q = new SqlQuery("crm_invoice_tax")
                .Select("1")
                .Where("tenant_id", TenantID)
                .Where("name", invoiceName)
                .SetMaxResults(1);

            using (var db = GetDb())
            {
                return db.ExecuteScalar<bool>(q);
            }
        }

        public Boolean CanDelete(int invoiceTaxID)
        {
            using (var db = GetDb())
            {
                var count1 = db.ExecuteScalar<int>(@"select count(*) from crm_invoice_item where tenant_id = @tid and (invoice_tax1_id = @id or invoice_tax2_id = @id)",
                    new { tid = TenantID, id = invoiceTaxID });
                var count2 = db.ExecuteScalar<int>(@"select count(*) from crm_invoice_line where tenant_id = @tid and (invoice_tax1_id = @id or invoice_tax2_id = @id)", 
                    new { tid = TenantID, id = invoiceTaxID });

                return count1 == 0 && count2 == 0;
            }
        }

        #region Get

        public virtual List<InvoiceTax> GetAll()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetInvoiceTaxSqlQuery(null)).ConvertAll(ToInvoiceTax);
            }
        }

        public virtual List<InvoiceTax> GetByID(int[] ids)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetInvoiceTaxSqlQuery(Exp.In("id", ids))).ConvertAll(ToInvoiceTax);
            }
        }

        public virtual InvoiceTax GetByID(int id)
        {
            using (var db = GetDb())
            {
                var invoiceTaxes = db.ExecuteList(GetInvoiceTaxSqlQuery(Exp.Eq("id", id))).ConvertAll(ToInvoiceTax);

                return invoiceTaxes.Count > 0 ? invoiceTaxes[0] : null;
            }
        }

        #endregion

        #region SaveOrUpdate

        public virtual InvoiceTax SaveOrUpdateInvoiceTax(InvoiceTax invoiceTax)
        {
            /*_cache.Remove(_invoiceItemCacheKey);
            _cache.Insert(_invoiceTaxCacheKey, String.Empty);*/

            using (var db = GetDb())
            {
                return SaveOrUpdateInvoiceTax(invoiceTax, db);
            }
        }

        private InvoiceTax SaveOrUpdateInvoiceTax(InvoiceTax invoiceTax, DbManager db)
        {
            if (String.IsNullOrEmpty(invoiceTax.Name))
                throw new ArgumentException();

            invoiceTax.LastModifedBy = SecurityContext.CurrentAccount.ID;
            invoiceTax.LastModifedOn = DateTime.UtcNow;

            if (db.ExecuteScalar<int>(Query("crm_invoice_tax").SelectCount().Where(Exp.Eq("id", invoiceTax.ID))) == 0)
            {
                invoiceTax.CreateOn = DateTime.UtcNow;
                invoiceTax.CreateBy = SecurityContext.CurrentAccount.ID;

                invoiceTax.ID = db.ExecuteScalar<int>(
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
                var oldInvoiceTax = db.ExecuteList(GetInvoiceTaxSqlQuery(Exp.Eq("id", invoiceTax.ID)))
                    .ConvertAll(ToInvoiceTax)
                    .FirstOrDefault();

                CRMSecurity.DemandEdit(oldInvoiceTax);

                db.ExecuteNonQuery(
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

            using (var db = GetDb())
            {
                db.ExecuteNonQuery(Delete("crm_invoice_tax").Where("id", invoiceTaxID));
            }

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
                    Rate = Convert.ToInt32(row[3]),
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