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
using ASC.CRM.Core.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace ASC.CRM.Core.Dao
{
    public class CachedInvoiceLineDao : InvoiceLineDao
    {
        private readonly HttpRequestDictionary<InvoiceLine> _invoiceLineCache = new HttpRequestDictionary<InvoiceLine>("crm_invoice_line");

        public CachedInvoiceLineDao(int tenantID)
            : base(tenantID)
        {
        }

        public override InvoiceLine GetByID(int invoiceLineID)
        {
            return _invoiceLineCache.Get(invoiceLineID.ToString(CultureInfo.InvariantCulture), () => GetByIDBase(invoiceLineID));
        }

        private InvoiceLine GetByIDBase(int invoiceLineID)
        {
            return base.GetByID(invoiceLineID);
        }

        public override int SaveOrUpdateInvoiceLine(InvoiceLine invoiceLine)
        {
            if (invoiceLine != null && invoiceLine.ID > 0)
                ResetCache(invoiceLine.ID);

            return base.SaveOrUpdateInvoiceLine(invoiceLine);
        }

        public override void DeleteInvoiceLine(int invoiceLineID)
        {
            ResetCache(invoiceLineID);

            base.DeleteInvoiceLine(invoiceLineID);
        }

        private void ResetCache(int invoiceLineID)
        {
            _invoiceLineCache.Reset(invoiceLineID.ToString(CultureInfo.InvariantCulture));
        }
    }
    
    public class InvoiceLineDao : AbstractDao
    {
        public InvoiceLineDao(int tenantID)
            : base(tenantID)
        {
        }


        public static string GetJson(InvoiceItem invoiceItem) {
            return invoiceItem == null ?
                    string.Empty :
                    JsonConvert.SerializeObject(new
                    {
                        id = invoiceItem.ID,
                        title = invoiceItem.Title,
                        description = invoiceItem.Description
                    });
        }
        public static string GetJson(InvoiceTax invoiceTax) {
            return invoiceTax == null ?
                    string.Empty :
                    JsonConvert.SerializeObject(new
                    {
                        id = invoiceTax.ID,
                        name = invoiceTax.Name,
                        rate = invoiceTax.Rate,
                        description = invoiceTax.Description
                    });
        }

        #region Get

        public virtual List<InvoiceLine> GetAll()
        {
            return Db.ExecuteList(GetInvoiceLineSqlQuery(null)).ConvertAll(ToInvoiceLine);
        }

        public virtual List<InvoiceLine> GetByID(int[] ids)
        {
            return Db.ExecuteList(GetInvoiceLineSqlQuery(Exp.In("id", ids))).ConvertAll(ToInvoiceLine);
        }

        public virtual InvoiceLine GetByID(int id)
        {
            var invoiceLines = Db.ExecuteList(GetInvoiceLineSqlQuery(Exp.Eq("id", id))).ConvertAll(ToInvoiceLine);

            return invoiceLines.Count > 0 ? invoiceLines[0] : null;
        }
        
        public List<InvoiceLine> GetInvoiceLines(int invoiceID)
        {
            return Db.ExecuteList(GetInvoiceLineSqlQuery(Exp.Eq("invoice_id", invoiceID)).OrderBy("sort_order", true)).ConvertAll(ToInvoiceLine);
        }

        public List<InvoiceLine> GetInvoicesLines(int[] invoiceIDs)
        {
            return Db.ExecuteList(GetInvoiceLineSqlQuery(Exp.In("invoice_id", invoiceIDs)).OrderBy("sort_order", true)).ConvertAll(ToInvoiceLine);
        }

        #endregion


        #region SaveOrUpdate

        public virtual int SaveOrUpdateInvoiceLine(InvoiceLine invoiceLine)
        {
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "invoice.*"));

            return SaveOrUpdateInvoiceLineInDb(invoiceLine);
        }

        private int SaveOrUpdateInvoiceLineInDb(InvoiceLine invoiceLine)
        {
            if (invoiceLine.InvoiceID <= 0 || invoiceLine.InvoiceItemID <= 0)
                throw new ArgumentException();

            if (String.IsNullOrEmpty(invoiceLine.Description))
            {
                invoiceLine.Description = String.Empty;
            }

            if (Db.ExecuteScalar<int>(Query("crm_invoice_line").SelectCount().Where(Exp.Eq("id", invoiceLine.ID))) == 0)
            {
                invoiceLine.ID = Db.ExecuteScalar<int>(
                               Insert("crm_invoice_line")
                              .InColumnValue("id", 0)
                              .InColumnValue("invoice_id", invoiceLine.InvoiceID)
                              .InColumnValue("invoice_item_id", invoiceLine.InvoiceItemID)
                              .InColumnValue("invoice_tax1_id", invoiceLine.InvoiceTax1ID)
                              .InColumnValue("invoice_tax2_id", invoiceLine.InvoiceTax2ID)
                              .InColumnValue("sort_order", invoiceLine.SortOrder)
                              .InColumnValue("description", invoiceLine.Description)
                              .InColumnValue("quantity", invoiceLine.Quantity)
                              .InColumnValue("price", invoiceLine.Price)
                              .InColumnValue("discount", invoiceLine.Discount)
                              .Identity(1, 0, true));
            }
            else
            {

                Db.ExecuteNonQuery(
                    Update("crm_invoice_line")
                        .Set("invoice_id", invoiceLine.InvoiceID)
                        .Set("invoice_item_id", invoiceLine.InvoiceItemID)
                        .Set("invoice_tax1_id", invoiceLine.InvoiceTax1ID)
                        .Set("invoice_tax2_id", invoiceLine.InvoiceTax2ID)
                        .Set("sort_order", invoiceLine.SortOrder)
                        .Set("description", invoiceLine.Description)
                        .Set("quantity", invoiceLine.Quantity)
                        .Set("price", invoiceLine.Price)
                        .Set("discount", invoiceLine.Discount)
                        .Where(Exp.Eq("id", invoiceLine.ID)));
            }
            return invoiceLine.ID;
        }

        #endregion


        #region Delete

        public virtual void DeleteInvoiceLine(int invoiceLineID)
        {
            var invoiceLine = GetByID(invoiceLineID);

            if (invoiceLine == null) return;

            Db.ExecuteNonQuery(Delete("crm_invoice_line").Where("id", invoiceLineID));

            /*_cache.Remove(_invoiceItemCacheKey);
            _cache.Insert(_invoiceLineCacheKey, String.Empty);*/
        }

        public void DeleteInvoiceLines(int invoiceID)
        {
            Db.ExecuteNonQuery(Delete("crm_invoice_line").Where(Exp.Eq("invoice_id", invoiceID)));

            /*_cache.Remove(_invoiceItemCacheKey);
            _cache.Insert(_invoiceLineCacheKey, String.Empty);*/
        }

        public Boolean CanDelete(int invoiceLineID)
        {
            return CanDeleteInDb(invoiceLineID);
        }

        public Boolean CanDeleteInDb(int invoiceLineID)
        {

                var invoiceID = Db.ExecuteScalar<int>(Query("crm_invoice_line").Select("invoice_id")
                                     .Where(Exp.Eq("id", invoiceLineID)));

                if (invoiceID == 0) return false;

                var count = Db.ExecuteScalar<int>(Query("crm_invoice_line").SelectCount()
                                        .Where(Exp.Eq("invoice_id", invoiceID)));

                return count > 1;
        }

        #endregion


        #region Private Methods

        private static InvoiceLine ToInvoiceLine(object[] row)
        {
            return new InvoiceLine
                {
                    ID = Convert.ToInt32(row[0]),
                    InvoiceID = Convert.ToInt32(row[1]),
                    InvoiceItemID = Convert.ToInt32(row[2]),
                    InvoiceTax1ID = Convert.ToInt32(row[3]),
                    InvoiceTax2ID = Convert.ToInt32(row[4]),
                    SortOrder = Convert.ToInt32(row[5]),
                    Description = Convert.ToString(row[6]),
                    Quantity = Convert.ToDecimal(row[7]),
                    Price = Convert.ToDecimal(row[8]),
                    Discount = Convert.ToDecimal(row[9])
                };
        }

        private SqlQuery GetInvoiceLineSqlQuery(Exp where)
        {
            var sqlQuery = Query("crm_invoice_line")
                .Select(
                    "id",
                    "invoice_id",
                    "invoice_item_id",
                    "invoice_tax1_id",
                    "invoice_tax2_id",
                    "sort_order",
                    "description",
                    "quantity",
                    "price",
                    "discount");

            if (where != null)
            {
                sqlQuery.Where(where);
            }

            return sqlQuery;
        }

        #endregion
    }
}