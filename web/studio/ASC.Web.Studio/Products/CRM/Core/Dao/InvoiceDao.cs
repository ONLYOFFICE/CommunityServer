/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Caching;
using ASC.Collections;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Common.Logging;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.Core.Utility.Settings;
using ASC.Web.Studio.Utility;
using SecurityContext = ASC.Core.SecurityContext;
using System.Security;
using Newtonsoft.Json;

namespace ASC.CRM.Core.Dao
{
    public class CachedInvoiceDao : InvoiceDao
    {
        private readonly HttpRequestDictionary<Invoice> _invoiceCache = new HttpRequestDictionary<Invoice>("crm_invoice");

        public CachedInvoiceDao(int tenantID, string storageKey)
            : base(tenantID, storageKey)
        {
        }

        public override Invoice GetByID(int invoiceID)
        {
            return _invoiceCache.Get(invoiceID.ToString(CultureInfo.InvariantCulture), () => GetByIDBase(invoiceID));
        }

        private Invoice GetByIDBase(int invoiceID)
        {
            return base.GetByID(invoiceID);
        }

        public override int SaveOrUpdateInvoice(Invoice invoice)
        {
            if (invoice != null && invoice.ID > 0)
                ResetCache(invoice.ID);

            return base.SaveOrUpdateInvoice(invoice);
        }

        public override Invoice DeleteInvoice(int invoiceID)
        {
            ResetCache(invoiceID);

            return base.DeleteInvoice(invoiceID);
        }

        private void ResetCache(int invoiceID)
        {
            _invoiceCache.Reset(invoiceID.ToString(CultureInfo.InvariantCulture));
        }
    }

    public class InvoiceDao : AbstractDao
    {
        public List<KeyValuePair<InvoiceStatus, InvoiceStatus>> invoiceStatusMap = new List<KeyValuePair<InvoiceStatus, InvoiceStatus>>()
        {
            new KeyValuePair<InvoiceStatus, InvoiceStatus>(InvoiceStatus.Draft, InvoiceStatus.Sent),
            new KeyValuePair<InvoiceStatus, InvoiceStatus>(InvoiceStatus.Sent, InvoiceStatus.Paid),
            new KeyValuePair<InvoiceStatus, InvoiceStatus>(InvoiceStatus.Sent, InvoiceStatus.Rejected),
            new KeyValuePair<InvoiceStatus, InvoiceStatus>(InvoiceStatus.Rejected, InvoiceStatus.Draft),
            new KeyValuePair<InvoiceStatus, InvoiceStatus>(InvoiceStatus.Paid, InvoiceStatus.Sent)//Bug 23450
        };

        public InvoiceDao(int tenantID, String storageKey)
            : base(tenantID, storageKey)
        {
        }

        public Boolean IsExist(int invoiceID)
        {
            using (var db = GetDb())
            {
                return IsExist(invoiceID, db);
            }
        }


        public Boolean IsExist(int invoiceID, DbManager db)
        {
            var q = new SqlExp(
                    string.Format(@"select exists(select 1 from crm_invoice where tenant_id = {0} and id = {1})",
                                TenantID,
                                invoiceID));
            return db.ExecuteScalar<bool>(q);
        }

        public Boolean IsExist(string number)
        {
            using (var db = GetDb())
            {
                return IsExist(number, db);
            }
        }

        public Boolean IsExist(string number, DbManager db)
        {
            var q = new SqlQuery("crm_invoice")
                .SelectCount()
                .Where("tenant_id", TenantID)
                .Where("number", number);
            return db.ExecuteScalar<int>(q) > 0;
        }

        #region Get

        public virtual List<Invoice> GetAll()
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetInvoiceSqlQuery(null, null)).ConvertAll(ToInvoice);
            }
        }
        
        public virtual List<Invoice> GetByID(int[] ids)
        {
            using (var db = GetDb())
            {
                return db.ExecuteList(GetInvoiceSqlQuery(Exp.In("id", ids), null)).ConvertAll(ToInvoice);
            }
        }

        public virtual Invoice GetByID(int id)
        {
            using (var db = GetDb())
            {
                return GetByID(id, db);
            }
        }

        public virtual Invoice GetByID(int id, DbManager db)
        {
            var invoices = db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("id", id), null)).ConvertAll(ToInvoice);
            return invoices.Count > 0 ? invoices[0] : null;
        }

        public Invoice GetByNumber(string number)
        {
            using (var db = GetDb())
            {
                var invoices = db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("number", number), null)).ConvertAll(ToInvoice);
                return invoices.Count > 0 ? invoices[0] : null;
            }
        }

        public Invoice GetByFileId(Int32 fileID)
        {
            using (var db = GetDb())
            {
                var invoices = db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("file_id", fileID), null)).ConvertAll(ToInvoice);
                return invoices.Count > 0 ? invoices[0] : null;
            }
        }

        public List<Invoice> GetInvoices(
                                    String searchText,
                                    InvoiceStatus? status,
                                    DateTime issueDateFrom,
                                    DateTime issueDateTo,
                                    DateTime dueDateFrom,
                                    DateTime dueDateTo,
                                    EntityType entityType,
                                    int entityID,
                                    String currency,
                                    int from,
                                    int count,
                                    OrderBy orderBy)
        {

            if (CRMSecurity.IsAdmin)
                return GetCrudeInvoices(
                    searchText,
                    status,
                    issueDateFrom,
                    issueDateTo,
                    dueDateFrom,
                    dueDateTo,
                    entityType,
                    entityID,
                    currency,
                    from,
                    count,
                    orderBy);


            var crudeInvoices = GetCrudeInvoices(
                    searchText,
                    status,
                    issueDateFrom,
                    issueDateTo,
                    dueDateFrom,
                    dueDateTo,
                    entityType,
                    entityID,
                    currency,
                    0,
                    from + count,
                    orderBy);

            if (crudeInvoices.Count == 0) return crudeInvoices;

            if (crudeInvoices.Count < from + count) return CRMSecurity.FilterRead(crudeInvoices).Skip(from).ToList();

            var result = CRMSecurity.FilterRead(crudeInvoices).ToList();

            if (result.Count == crudeInvoices.Count) return result.Skip(from).ToList();

            var localCount = count;
            var localFrom = from + count;

            while (true)
            {
                crudeInvoices = GetCrudeInvoices(
                    searchText,
                    status,
                    issueDateFrom,
                    issueDateTo,
                    dueDateFrom,
                    dueDateTo,
                    entityType,
                    entityID,
                    currency,
                    localFrom,
                    localCount,
                    orderBy);

                if (crudeInvoices.Count == 0) break;

                result.AddRange(CRMSecurity.FilterRead(crudeInvoices));

                if ((result.Count >= count + from) || (crudeInvoices.Count < localCount)) break;

                localFrom += localCount;
                localCount = localCount * 2;
            }

            return result.Skip(from).Take(count).ToList();
        }


        public List<Invoice> GetCrudeInvoices(
                                String searchText,
                                InvoiceStatus? status,
                                DateTime issueDateFrom,
                                DateTime issueDateTo,
                                DateTime dueDateFrom,
                                DateTime dueDateTo,
                                EntityType entityType,
                                int entityID,
                                String currency,
                                int from,
                                int count,
                                OrderBy orderBy)
        {
            var invoicesTableAlias = "i_tbl";

            var sqlQuery = GetInvoiceSqlQuery(null, invoicesTableAlias);

            var withParams = hasParams(searchText, status, issueDateFrom, issueDateTo, dueDateFrom, dueDateTo, entityType, entityID, currency);

            var whereConditional = WhereConditional(invoicesTableAlias, new List<int>(), searchText, status, issueDateFrom, issueDateTo, dueDateFrom, dueDateTo, entityType, entityID, currency);
                // WhereConditional(CRMSecurity.GetPrivateItems(typeof(Invoice)).ToList(), searchText);

            if (withParams && whereConditional == null)
                return new List<Invoice>();

            sqlQuery.Where(whereConditional);

            if (0 < from && from < int.MaxValue) sqlQuery.SetFirstResult(from);
            if (0 < count && count < int.MaxValue) sqlQuery.SetMaxResults(count);

            if (orderBy != null && Enum.IsDefined(typeof(InvoiceSortedByType), orderBy.SortedBy))
            {
                switch ((InvoiceSortedByType)orderBy.SortedBy)
                {
                    case InvoiceSortedByType.Number:
                        sqlQuery.OrderBy(invoicesTableAlias + ".number", orderBy.IsAsc);
                        break;
                    case InvoiceSortedByType.Status:
                        sqlQuery.OrderBy(invoicesTableAlias + ".status", orderBy.IsAsc);
                        break;
                    case InvoiceSortedByType.DueDate:
                        sqlQuery.OrderBy(invoicesTableAlias + ".due_date", orderBy.IsAsc);
                        break;
                    case InvoiceSortedByType.IssueDate:
                        sqlQuery.OrderBy(invoicesTableAlias + ".issue_date", orderBy.IsAsc);
                        break;
                    case InvoiceSortedByType.Contact:
                        sqlQuery.LeftOuterJoin("crm_contact c_tbl", Exp.EqColumns(invoicesTableAlias + ".contact_id", "c_tbl.id"))
                                .OrderBy("case when c_tbl.display_name is null then 1 else 0 end, c_tbl.display_name", orderBy.IsAsc)
                                .OrderBy(invoicesTableAlias + ".number", true);
                        break;
                    default:
                        sqlQuery.OrderBy(invoicesTableAlias + ".number", true);
                        break;
                }
            }
            else
            {
                sqlQuery.OrderBy(invoicesTableAlias + ".number", true);
            }

            using (var db = GetDb())
            {
                return db.ExecuteList(sqlQuery).ConvertAll(ToInvoice);
            }
        }

        public int GetAllInvoicesCount()
        {
            using (var db = GetDb())
            {
                return db.ExecuteScalar<int>(Query("crm_invoice").SelectCount());
            }
        }


        public int GetInvoicesCount()
        {
            return GetInvoicesCount(String.Empty, null, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, EntityType.Any, 0, null);
        }

        public int GetInvoicesCount(
                                    String searchText,
                                    InvoiceStatus? status,
                                    DateTime issueDateFrom,
                                    DateTime issueDateTo,
                                    DateTime dueDateFrom,
                                    DateTime dueDateTo,
                                    EntityType entityType,
                                    int entityID,
                                    String currency)
        {
            var cacheKey = TenantID.ToString(CultureInfo.InvariantCulture) +
                           "invoice" +
                           SecurityContext.CurrentAccount.ID.ToString() +
                           searchText;

            var fromCache = _cache.Get(cacheKey);

            if (fromCache != null) return Convert.ToInt32(fromCache);

            var withParams = hasParams(searchText, status, issueDateFrom, issueDateTo, dueDateFrom, dueDateTo, entityType, entityID, currency);

            var exceptIDs = CRMSecurity.GetPrivateItems(typeof(Invoice)).ToList();

            int result;

            using (var db = GetDb())
            {
                if (withParams)
                {
                    var whereConditional = WhereConditional(null, exceptIDs, searchText, status, issueDateFrom, issueDateTo, dueDateFrom, dueDateTo, entityType, entityID, currency);
                    result = whereConditional != null ? db.ExecuteScalar<int>(Query("crm_invoice").Where(whereConditional).SelectCount()) : 0;
                }
                else
                {
                    var countWithoutPrivate = db.ExecuteScalar<int>(Query("crm_invoice").SelectCount());
                    var privateCount = exceptIDs.Count;

                    if (privateCount > countWithoutPrivate)
                    {
                        _log.ErrorFormat(@"Private invoice count more than all cases. Tenant: {0}. CurrentAccount: {1}", 
                                                               TenantID, 
                                                               SecurityContext.CurrentAccount.ID);
                 
                        privateCount = 0;
                    }

                    result = countWithoutPrivate - privateCount;
                }
            }

            if (result > 0)
                _cache.Insert(cacheKey, result, new CacheDependency(null, new[] { _invoiceCacheKey }), Cache.NoAbsoluteExpiration,
                                      TimeSpan.FromSeconds(30));

            return result;
        }

        public List<Invoice> GetInvoices(int[] ids)
        {
            if (ids == null || !ids.Any()) return new List<Invoice>();

            using (var db = GetDb())
            {
                return db.ExecuteList(GetInvoiceSqlQuery(Exp.In("id", ids), null))
                   .ConvertAll(ToInvoice).FindAll(CRMSecurity.CanAccessTo).ToList();
            }
        }

        public List<Invoice> GetEntityInvoices(EntityType entityType, int entityID)
        {
            var result = new List<Invoice>();
            if (entityID <= 0)
                return result;

            using (var db = GetDb())
            {
                if (entityType == EntityType.Opportunity)
                    return db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType), null))
                        .ConvertAll(ToInvoice)
                        .FindAll(CRMSecurity.CanAccessTo)
                        .ToList();

                if (entityType == EntityType.Contact || entityType == EntityType.Person || entityType == EntityType.Company)
                    return GetContactInvoices(entityID);

                return result;
            }
        }

        public List<Invoice> GetContactInvoices(int contactID)
        {
            var result = new List<Invoice>();
            if (contactID <= 0)
                return result;

            using (var db = GetDb())
            {
                return db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("contact_id", contactID), null))
                        .ConvertAll(ToInvoice)
                        .FindAll(CRMSecurity.CanAccessTo)
                        .ToList();
            }
        }

        public string GetNewInvoicesNumber()
        {
            using (var db = GetDb())
            {
                var settings = GetSettings();

                if (!settings.Autogenerated)
                    return string.Empty;
                
                var query = Query("crm_invoice")
                    .Select("number")
                    .OrderBy("id", false)
                    .SetMaxResults(1);

                var stringNumber = db.ExecuteScalar<string>(query);
                
                if (string.IsNullOrEmpty(stringNumber) || !stringNumber.StartsWith(settings.Prefix))
                    return string.Concat(settings.Prefix, settings.Number);

                if (!string.IsNullOrEmpty(settings.Prefix))
                    stringNumber = stringNumber.Replace(settings.Prefix, string.Empty);

                int intNumber;
                if (!Int32.TryParse(stringNumber, out intNumber))
                    intNumber = 0;

                var format = string.Empty;
                for (var i = 0; i < settings.Number.Length; i++)
                {
                    format += "0";
                }

                var nextNumber = intNumber + 1;

                return settings.Prefix + (string.IsNullOrEmpty(format) ? nextNumber.ToString(CultureInfo.InvariantCulture) : nextNumber.ToString(format));
            } 
        }

        public InvoiceSetting GetSettings()
        {
            return Global.TenantSettings.InvoiceSetting ?? InvoiceSetting.DefaultSettings;
        }

        #endregion

        #region SaveOrUpdate

        public virtual int SaveOrUpdateInvoice(Invoice invoice)
        {
            _cache.Insert(_invoiceCacheKey, String.Empty);

            using (var db = GetDb())
            {
                return SaveOrUpdateInvoice(invoice, db);
            }
        }

        private int SaveOrUpdateInvoice(Invoice invoice, DbManager db)
        {
            if (String.IsNullOrEmpty(invoice.Number) ||
                invoice.IssueDate == DateTime.MinValue ||
                invoice.ContactID <= 0 ||
                invoice.DueDate == DateTime.MinValue ||
                String.IsNullOrEmpty(invoice.Currency) ||
                invoice.ExchangeRate <= 0 ||
                String.IsNullOrEmpty(invoice.Terms))
                throw new ArgumentException();


            if (!IsExist(invoice.ID, db))
            {
                if (IsExist(invoice.Number, db))
                    throw new ArgumentException();

                invoice.ID = db.ExecuteScalar<int>(
                               Insert("crm_invoice")
                              .InColumnValue("id", 0)
                              .InColumnValue("status", (int)invoice.Status)
                              .InColumnValue("number", invoice.Number)
                              .InColumnValue("issue_date", TenantUtil.DateTimeToUtc(invoice.IssueDate))
                              .InColumnValue("template_type", invoice.TemplateType)
                              .InColumnValue("contact_id", invoice.ContactID)
                              .InColumnValue("consignee_id", invoice.ConsigneeID)
                              .InColumnValue("entity_type", (int)invoice.EntityType)
                              .InColumnValue("entity_id", invoice.EntityID)
                              .InColumnValue("due_date", TenantUtil.DateTimeToUtc(invoice.DueDate))
                              .InColumnValue("language", invoice.Language)
                              .InColumnValue("currency", invoice.Currency)
                              .InColumnValue("exchange_rate", invoice.ExchangeRate)
                              .InColumnValue("purchase_order_number", invoice.PurchaseOrderNumber)
                              .InColumnValue("terms", invoice.Terms)
                              .InColumnValue("description", invoice.Description)
                              .InColumnValue("json_data", null)
                              .InColumnValue("file_id", 0)
                              .InColumnValue("create_on", DateTime.UtcNow)
                              .InColumnValue("create_by", SecurityContext.CurrentAccount.ID)
                              .InColumnValue("last_modifed_on", DateTime.UtcNow)
                              .InColumnValue("last_modifed_by", SecurityContext.CurrentAccount.ID)
                              .Identity(1, 0, true));
            }
            else
            {

                var oldInvoice = db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("id", invoice.ID), null))
                    .ConvertAll(ToInvoice)
                    .FirstOrDefault();

                CRMSecurity.DemandEdit(oldInvoice);

                if (oldInvoice.ContactID != invoice.ContactID) {
                    oldInvoice.FileID = 0;
                }

                db.ExecuteNonQuery(
                    Update("crm_invoice")
                        .Set("status", (int) invoice.Status)
                        .Set("issue_date", TenantUtil.DateTimeToUtc(invoice.IssueDate))
                        .Set("template_type", invoice.TemplateType)
                        .Set("contact_id", invoice.ContactID)
                        .Set("consignee_id", invoice.ConsigneeID)
                        .Set("entity_type", (int) invoice.EntityType)
                        .Set("entity_id", invoice.EntityID)
                        .Set("due_date", TenantUtil.DateTimeToUtc(invoice.DueDate))
                        .Set("language", invoice.Language)
                        .Set("currency", invoice.Currency)
                        .Set("exchange_rate", invoice.ExchangeRate)
                        .Set("purchase_order_number", invoice.PurchaseOrderNumber)
                        .Set("terms", invoice.Terms)
                        .Set("description", invoice.Description)
                        .Set("json_data", null)
                        .Set("file_id", oldInvoice.FileID)
                        .Set("last_modifed_on", DateTime.UtcNow)
                        .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                        .Where(Exp.Eq("id", invoice.ID)));
            }

            return invoice.ID;
        }

        public virtual Invoice UpdateInvoiceStatus(int invoiceid, InvoiceStatus status)
        {
            using (var db = GetDb())
            {
                return UpdateInvoiceStatus(invoiceid, status, db);
            }
        }

        public List<Invoice> UpdateInvoiceBatchStatus(int[] invoiceids, InvoiceStatus status)
        {
            if (invoiceids == null || !invoiceids.Any())
                throw new ArgumentException();

            var invoices = new List<Invoice>();

            using (var db = GetDb())
            {
                foreach (var id in invoiceids)
                {
                    var inv = UpdateInvoiceStatus(id, status, db);
                    if (inv != null) {
                        invoices.Add(inv);
                    }
                }
            }
            return invoices;
        }

        private Invoice UpdateInvoiceStatus(int invoiceid, InvoiceStatus status, DbManager db)
        {
            var invoice = GetByID(invoiceid, db);
            if (invoice == null) {
                _log.Error("Invoice not found");
                return null;
            }
            CRMSecurity.DemandAccessTo(invoice);

            if (!invoiceStatusMap.Contains(new KeyValuePair<InvoiceStatus, InvoiceStatus>(invoice.Status, status))){
                _log.ErrorFormat("Status for invoice with ID={0} can't be changed. Return without changes", invoiceid);
                return invoice;
            }

            db.ExecuteNonQuery(
                Update("crm_invoice")
                    .Set("status", (int)status)
                    .Set("last_modifed_on", DateTime.UtcNow)
                    .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                    .Where(Exp.Eq("id", invoiceid)));

            invoice.Status = status;
            return invoice;
        }

        public virtual int UpdateInvoiceJsonData(int invoiceId, string jsonData)
        {
            using (var db = GetDb())
            {
                return UpdateInvoiceJsonData(invoiceId, jsonData, db);
            }
        }

        private int UpdateInvoiceJsonData(int invoiceId, string jsonData, DbManager db)
        {
            db.ExecuteNonQuery(
                Update("crm_invoice")
                    .Set("json_data", jsonData)
                    .Set("last_modifed_on", DateTime.UtcNow)
                    .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                    .Where(Exp.Eq("id", invoiceId)));

            return invoiceId;
        }

        public void UpdateInvoiceJsonData(Invoice invoice, int billingAddressID, int deliveryAddressID)
        {
            invoice.JsonData = JsonConvert.SerializeObject(InvoiceFormattedData.GetData(invoice, billingAddressID, deliveryAddressID));
            Global.DaoFactory.GetInvoiceDao().UpdateInvoiceJsonData(invoice.ID, invoice.JsonData);
        }

        public virtual int UpdateInvoiceFileID(int invoiceId, int fileId)
        {
            using (var db = GetDb())
            {
                return UpdateInvoiceFileID(invoiceId, fileId, db);
            }
        }

        private int UpdateInvoiceFileID(int invoiceId, int fileId, DbManager db)
        {
            db.ExecuteNonQuery(
                Update("crm_invoice")
                    .Set("file_id", fileId)
                    .Set("last_modifed_on", DateTime.UtcNow)
                    .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                    .Where(Exp.Eq("id", invoiceId)));

            return invoiceId;
        }

        public InvoiceSetting SaveInvoiceSettings(InvoiceSetting invoiceSetting)
        {
            var tenantSettings = Global.TenantSettings;

            tenantSettings.InvoiceSetting = invoiceSetting;

            SettingsManager.Instance.SaveSettings(tenantSettings, TenantProvider.CurrentTenantID);

            AdminLog.PostAction("CRM Settings: saved crm invoice settings to \"{0:Json}\"", tenantSettings.InvoiceSetting);

            return tenantSettings.InvoiceSetting;
        }

        #endregion

        #region Delete


        public virtual Invoice DeleteInvoice(int invoiceID)
        {
            if (invoiceID <= 0) return null;

            var invoice = GetByID(invoiceID);
            if (invoice == null) return null;

            CRMSecurity.DemandDelete(invoice);

            // Delete relative  keys
            _cache.Insert(_invoiceCacheKey, String.Empty);

            DeleteBatchInvoicesExecute(new List<Invoice> { invoice });

            return invoice;
        }

        public List<Invoice> DeleteBatchInvoices(int[] invoiceID)
        {
            var invoices = GetInvoices(invoiceID).Where(CRMSecurity.CanDelete).ToList();
            if (!invoices.Any()) return invoices;

            // Delete relative  keys
            _cache.Insert(_invoiceCacheKey, String.Empty);

            DeleteBatchInvoicesExecute(invoices);

            return invoices;
        }

        private void DeleteBatchInvoicesExecute(List<Invoice> invoices) {

            var invoiceID = invoices.Select(x => x.ID).ToArray();

            using (var db = GetDb())
            {
                using (var tx = db.BeginTransaction())
                {
                    db.ExecuteNonQuery(Delete("crm_invoice_line").Where(Exp.In("invoice_id", invoiceID)));
                    db.ExecuteNonQuery(Delete("crm_invoice").Where(Exp.In("id", invoiceID)));

                    tx.Commit();
                }
            }
        }

        #endregion

        #region Private Methods

        private static Invoice ToInvoice(object[] row)
        {
            return new Invoice
            {
                ID = Convert.ToInt32(row[0]),
                Status = (InvoiceStatus)Convert.ToInt32(row[1]),
                Number = Convert.ToString(row[2]),
                IssueDate = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[3].ToString())),
                TemplateType = (InvoiceTemplateType)Convert.ToInt32(row[4]),
                ContactID = Convert.ToInt32(row[5]),
                ConsigneeID = Convert.ToInt32(row[6]),
                EntityType = (EntityType)Convert.ToInt32(row[7]),
                EntityID = Convert.ToInt32(row[8]),
                DueDate = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[9].ToString())),
                Language = Convert.ToString(row[10]),
                Currency = Convert.ToString(row[11]),
                ExchangeRate = Convert.ToDecimal(row[12]),
                PurchaseOrderNumber = Convert.ToString(row[13]),
                Terms = Convert.ToString(row[14]),
                Description = Convert.ToString(row[15]),
                JsonData = Convert.ToString(row[16]),
                FileID = Convert.ToInt32(row[17]),
                CreateOn = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[18].ToString())),
                CreateBy = ToGuid(row[19]),
                LastModifedOn = TenantUtil.DateTimeFromUtc(DateTime.Parse(row[20].ToString())),
                LastModifedBy = ToGuid(row[21])
            };
        }

        private SqlQuery GetInvoiceSqlQuery(Exp where,  String alias)
        {
            var sqlQuery = Query("crm_invoice");

            if (!String.IsNullOrEmpty(alias))
            {
                sqlQuery = new SqlQuery(String.Concat("crm_invoice ", alias))
                           .Where(Exp.Eq(alias + ".tenant_id", TenantID));
                sqlQuery.Select(GetInvoiceColumnsTable(alias));

            }
            else
            {
                sqlQuery.Select(GetInvoiceColumnsTable(String.Empty));
            }

            if (where != null)
                sqlQuery.Where(where);

            return sqlQuery;
        }

        private String[] GetInvoiceColumnsTable(String alias)
        {
           if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";

            var result = new List<String>
                             {
                                "id",
                                "status",
                                "number",
                                "issue_date",
                                "template_type",
                                "contact_id",
                                "consignee_id",
                                "entity_type",
                                "entity_id",
                                "due_date",
                                "language",
                                "currency",
                                "exchange_rate",
                                "purchase_order_number",
                                "terms",
                                "description",
                                "json_data",
                                "file_id",
                                "create_on",
                                "create_by",
                                "last_modifed_on",
                                "last_modifed_by"
                             };

            if (String.IsNullOrEmpty(alias)) return result.ToArray();

            return result.ConvertAll(item => String.Concat(alias, item)).ToArray();
        }

        private Exp WhereConditional(String tblAlias,
                                ICollection<int> exceptIDs,
                                String searchText,
                                InvoiceStatus? status,
                                DateTime issueDateFrom,
                                DateTime issueDateTo,
                                DateTime dueDateFrom,
                                DateTime dueDateTo,
                                EntityType entityType,
                                int entityID,
                                String currency)
        {
            var tblAliasPrefix = !String.IsNullOrEmpty(tblAlias) ? tblAlias + ".": "";
            var conditions = new List<Exp>();

             if (entityID > 0)
                switch (entityType)
                {
                    case EntityType.Contact:
                    case EntityType.Person:
                    case EntityType.Company:
                        conditions.Add(Exp.Eq(tblAliasPrefix + "contact_id", entityID));
                        break;
                    case EntityType.Case:
                    case EntityType.Opportunity:
                        conditions.Add(Exp.Eq(tblAliasPrefix + "entity_id", entityID) &
                                       Exp.Eq(tblAliasPrefix + "entity_type", (int)entityType));
                        break;
                }

            var ids = new List<int>();

            if (status != null)
            {
                conditions.Add(Exp.Eq(tblAliasPrefix + "status", (int)status.Value));
            }


            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.Trim();

                var keywords = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                   .ToArray();

                if (keywords.Length > 0)
                    //if (FullTextSearch.SupportModule(FullTextSearch.CRMInvoiceModule))
                    //{
                    //    ids = FullTextSearch.Search(searchText, FullTextSearch.CRMInvoiceModule)
                    //        .GetIdentifiers()
                    //        .Select(item => Convert.ToInt32(item.Split('_')[1])).Distinct().ToList();

                    //    if (ids.Count == 0) return null;
                    //}
                    //else
                        conditions.Add(BuildLike(new[] { tblAliasPrefix + "number", tblAliasPrefix + "description" }, keywords));
            }

            if (exceptIDs.Count > 0)
            {
                conditions.Add(!Exp.In(tblAliasPrefix + "id", exceptIDs.ToArray()));
            }

            if (issueDateFrom != DateTime.MinValue && issueDateTo != DateTime.MinValue)
                conditions.Add(Exp.Between(tblAliasPrefix + "issue_date", TenantUtil.DateTimeToUtc(issueDateFrom), TenantUtil.DateTimeToUtc(issueDateTo.AddDays(1).AddMinutes(-1))));
            else if (issueDateFrom != DateTime.MinValue)
                conditions.Add(Exp.Ge(tblAliasPrefix + "issue_date", TenantUtil.DateTimeToUtc(issueDateFrom)));
            else if (issueDateTo != DateTime.MinValue)
                conditions.Add(Exp.Le(tblAliasPrefix + "issue_date", TenantUtil.DateTimeToUtc(issueDateTo.AddDays(1).AddMinutes(-1))));


            if (dueDateFrom != DateTime.MinValue && dueDateTo != DateTime.MinValue)
                conditions.Add(Exp.Between(tblAliasPrefix + "due_date", TenantUtil.DateTimeToUtc(dueDateFrom), TenantUtil.DateTimeToUtc(dueDateTo.AddDays(1).AddMinutes(-1))));
            else if (dueDateFrom != DateTime.MinValue)
                conditions.Add(Exp.Ge(tblAliasPrefix + "due_date", TenantUtil.DateTimeToUtc(dueDateFrom)));
            else if (dueDateTo != DateTime.MinValue)
                conditions.Add(Exp.Le(tblAliasPrefix + "due_date", TenantUtil.DateTimeToUtc(dueDateTo.AddDays(1).AddMinutes(-1))));

            if (!String.IsNullOrEmpty(currency)) {
                conditions.Add(Exp.Eq(tblAliasPrefix + "currency", currency));
            }

            if (conditions.Count == 0) return null;

            return conditions.Count == 1 ? conditions[0] : conditions.Aggregate((i, j) => i & j);
        }

        private bool hasParams(
                                String searchText,
                                InvoiceStatus? status,
                                DateTime issueDateFrom,
                                DateTime issueDateTo,
                                DateTime dueDateFrom,
                                DateTime dueDateTo,
                                EntityType entityType,
                                int entityID,
                                String currency)
        {
            return !(String.IsNullOrEmpty(searchText) && !status.HasValue &&
                issueDateFrom == DateTime.MinValue && issueDateTo == DateTime.MinValue &&
                dueDateFrom == DateTime.MinValue && dueDateTo == DateTime.MinValue &&
                entityID == 0 && String.IsNullOrEmpty(currency));
        }

        #endregion
    }
}