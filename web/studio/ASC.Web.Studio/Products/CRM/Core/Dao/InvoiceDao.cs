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
using System.Text.RegularExpressions;
using ASC.Collections;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;
using ASC.Core.Tenants;
using ASC.CRM.Core.Entities;
using ASC.ElasticSearch;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Core.Search;
using Newtonsoft.Json;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.CRM.Core.Dao
{
    public class CachedInvoiceDao : InvoiceDao
    {
        private readonly HttpRequestDictionary<Invoice> _invoiceCache = new HttpRequestDictionary<Invoice>("crm_invoice");

        public CachedInvoiceDao(int tenantID)
            : base(tenantID)
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

        public InvoiceDao(int tenantID)
            : base(tenantID)
        {
        }

        public Boolean IsExist(int invoiceID)
        {
            return IsExistFromDb(invoiceID);
        }


        public Boolean IsExistFromDb(int invoiceID)
        {
            return Db.ExecuteScalar<bool>(@"select exists(select 1 from crm_invoice where tenant_id = @tid and id = @id)",
                new { tid = TenantID, id = invoiceID });
        }

        public Boolean IsExist(string number)
        {
            return IsExistFromDb(number);
        }

        public Boolean IsExistFromDb(string number)
        {
            var q = new SqlQuery("crm_invoice")
                .SelectCount()
                .Where("tenant_id", TenantID)
                .Where("number", number);
            return Db.ExecuteScalar<int>(q) > 0;
        }

        #region Get

        public virtual List<Invoice> GetAll()
        {
            return Db.ExecuteList(GetInvoiceSqlQuery(null, null)).ConvertAll(ToInvoice);
        }

        public virtual List<Invoice> GetByID(int[] ids)
        {
            return Db.ExecuteList(GetInvoiceSqlQuery(Exp.In("id", ids), null)).ConvertAll(ToInvoice);
        }

        public virtual Invoice GetByID(int id)
        {
            return GetByIDFromDb(id);
        }

        public virtual Invoice GetByIDFromDb(int id)
        {
            var invoices = Db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("id", id), null)).ConvertAll(ToInvoice);
            return invoices.Count > 0 ? invoices[0] : null;
        }

        public Invoice GetByNumber(string number)
        {
            var invoices = Db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("number", number), null)).ConvertAll(ToInvoice);
            return invoices.Count > 0 ? invoices[0] : null;
        }

        public Invoice GetByFileId(Int32 fileID)
        {
            var invoices = Db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("file_id", fileID), null)).ConvertAll(ToInvoice);
            return invoices.Count > 0 ? invoices[0] : null;
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
                        sqlQuery.LeftOuterJoin("crm_contact c_tbl", Exp.EqColumns(invoicesTableAlias + ".contact_id", "c_tbl.id") & Exp.EqColumns(invoicesTableAlias + ".tenant_id", "c_tbl.tenant_id"))
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

            return Db.ExecuteList(sqlQuery).ConvertAll(ToInvoice);
        }

        public int GetAllInvoicesCount()
        {
            return Db.ExecuteScalar<int>(Query("crm_invoice").SelectCount());
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

            var fromCache = _cache.Get<string>(cacheKey);

            if (fromCache != null) return Convert.ToInt32(fromCache);

            var withParams = hasParams(searchText, status, issueDateFrom, issueDateTo, dueDateFrom, dueDateTo, entityType, entityID, currency);

            var exceptIDs = CRMSecurity.GetPrivateItems(typeof(Invoice)).ToList();

            int result;

            if (withParams)
            {
                var whereConditional = WhereConditional(null, exceptIDs, searchText, status, issueDateFrom, issueDateTo, dueDateFrom, dueDateTo, entityType, entityID, currency);
                result = whereConditional != null ? Db.ExecuteScalar<int>(Query("crm_invoice").Where(whereConditional).SelectCount()) : 0;
            }
            else
            {
                var countWithoutPrivate = Db.ExecuteScalar<int>(Query("crm_invoice").SelectCount());
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

            if (result > 0)
            {
                _cache.Insert(cacheKey, result, TimeSpan.FromSeconds(30));
            }
            return result;
        }

        public List<Invoice> GetInvoices(int[] ids)
        {
            if (ids == null || !ids.Any()) return new List<Invoice>();

            return Db.ExecuteList(GetInvoiceSqlQuery(Exp.In("id", ids), null))
                .ConvertAll(ToInvoice).FindAll(CRMSecurity.CanAccessTo).ToList();
        }

        public List<Invoice> GetEntityInvoices(EntityType entityType, int entityID)
        {
            var result = new List<Invoice>();
            if (entityID <= 0)
                return result;

            if (entityType == EntityType.Opportunity)
                return Db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("entity_id", entityID) & Exp.Eq("entity_type", (int)entityType), null))
                    .ConvertAll(ToInvoice)
                    .FindAll(CRMSecurity.CanAccessTo)
                    .ToList();

            if (entityType == EntityType.Contact || entityType == EntityType.Person || entityType == EntityType.Company)
                return GetContactInvoices(entityID);

            return result;
        }

        public List<Invoice> GetContactInvoices(int contactID)
        {
            var result = new List<Invoice>();
            if (contactID <= 0)
                return result;

            return Db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("contact_id", contactID), null))
                    .ConvertAll(ToInvoice)
                    .FindAll(CRMSecurity.CanAccessTo)
                    .ToList();
        }

        public string GetNewInvoicesNumber()
        {
            var settings = GetSettings();

            if (!settings.Autogenerated)
                return string.Empty;

            var query = Query("crm_invoice")
                .Select("number")
                .OrderBy("id", false)
                .SetMaxResults(1);

            var stringNumber = Db.ExecuteScalar<string>(query);

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

        public InvoiceSetting GetSettings()
        {
            return Global.TenantSettings.InvoiceSetting ?? InvoiceSetting.DefaultSettings;
        }

        #endregion

        #region SaveOrUpdate

        public virtual int SaveOrUpdateInvoice(Invoice invoice)
        {
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "invoice.*"));

            var result =  SaveOrUpdateInvoiceInDb(invoice);

            FactoryIndexer<InvoicesWrapper>.IndexAsync(invoice);

            return result;
        }

        private int SaveOrUpdateInvoiceInDb(Invoice invoice)
        {
            if (String.IsNullOrEmpty(invoice.Number) ||
                invoice.IssueDate == DateTime.MinValue ||
                invoice.ContactID <= 0 ||
                invoice.DueDate == DateTime.MinValue ||
                String.IsNullOrEmpty(invoice.Currency) ||
                invoice.ExchangeRate <= 0 ||
                String.IsNullOrEmpty(invoice.Terms))
                throw new ArgumentException();



            invoice.PurchaseOrderNumber = !String.IsNullOrEmpty(invoice.PurchaseOrderNumber) ? invoice.PurchaseOrderNumber : String.Empty;

            if (!IsExistFromDb(invoice.ID))
            {
                if (IsExistFromDb(invoice.Number))
                    throw new ArgumentException();

                invoice.ID = Db.ExecuteScalar<int>(
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
                              .InColumnValue("create_on", invoice.CreateOn == DateTime.MinValue ? DateTime.UtcNow : invoice.CreateOn)
                              .InColumnValue("create_by", SecurityContext.CurrentAccount.ID)
                              .InColumnValue("last_modifed_on", invoice.CreateOn == DateTime.MinValue ? DateTime.UtcNow : invoice.CreateOn)
                              .InColumnValue("last_modifed_by", SecurityContext.CurrentAccount.ID)
                              .Identity(1, 0, true));
            }
            else
            {

                var oldInvoice = Db.ExecuteList(GetInvoiceSqlQuery(Exp.Eq("id", invoice.ID), null))
                    .ConvertAll(ToInvoice)
                    .FirstOrDefault();

                CRMSecurity.DemandEdit(oldInvoice);

                Db.ExecuteNonQuery(
                    Update("crm_invoice")
                        .Set("status", (int)invoice.Status)
                        .Set("issue_date", TenantUtil.DateTimeToUtc(invoice.IssueDate))
                        .Set("template_type", invoice.TemplateType)
                        .Set("contact_id", invoice.ContactID)
                        .Set("consignee_id", invoice.ConsigneeID)
                        .Set("entity_type", (int)invoice.EntityType)
                        .Set("entity_id", invoice.EntityID)
                        .Set("due_date", TenantUtil.DateTimeToUtc(invoice.DueDate))
                        .Set("language", invoice.Language)
                        .Set("currency", invoice.Currency)
                        .Set("exchange_rate", invoice.ExchangeRate)
                        .Set("purchase_order_number", invoice.PurchaseOrderNumber)
                        .Set("terms", invoice.Terms)
                        .Set("description", invoice.Description)
                        .Set("json_data", null)
                        .Set("file_id", 0)
                        .Set("last_modifed_on", DateTime.UtcNow)
                        .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                        .Where(Exp.Eq("id", invoice.ID)));
            }

            return invoice.ID;
        }

        public virtual Invoice UpdateInvoiceStatus(int invoiceid, InvoiceStatus status)
        {
            return UpdateInvoiceStatusInDb(invoiceid, status);
        }

        public List<Invoice> UpdateInvoiceBatchStatus(int[] invoiceids, InvoiceStatus status)
        {
            if (invoiceids == null || !invoiceids.Any())
                throw new ArgumentException();

            var invoices = new List<Invoice>();

            foreach (var id in invoiceids)
            {
                var inv = UpdateInvoiceStatusInDb(id, status);
                if (inv != null)
                {
                    invoices.Add(inv);
                }
            }
            return invoices;
        }

        private Invoice UpdateInvoiceStatusInDb(int invoiceid, InvoiceStatus status)
        {
            var invoice = GetByIDFromDb(invoiceid);
            if (invoice == null)
            {
                _log.Error("Invoice not found");
                return null;
            }
            CRMSecurity.DemandAccessTo(invoice);

            if (!invoiceStatusMap.Contains(new KeyValuePair<InvoiceStatus, InvoiceStatus>(invoice.Status, status)))
            {
                _log.ErrorFormat("Status for invoice with ID={0} can't be changed. Return without changes", invoiceid);
                return invoice;
            }

            Db.ExecuteNonQuery(
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
            return UpdateInvoiceJsonDataInDb(invoiceId, jsonData);
        }

        private int UpdateInvoiceJsonDataInDb(int invoiceId, string jsonData)
        {
            Db.ExecuteNonQuery(
                Update("crm_invoice")
                    .Set("json_data", jsonData)
                    .Set("last_modifed_on", DateTime.UtcNow)
                    .Set("last_modifed_by", SecurityContext.CurrentAccount.ID)
                    .Where(Exp.Eq("id", invoiceId)));

            return invoiceId;
        }

        public void UpdateInvoiceJsonData(Invoice invoice, int billingAddressID, int deliveryAddressID)
        {
            var jsonData = InvoiceFormattedData.GetData(invoice, billingAddressID, deliveryAddressID);
            if (jsonData.LogoBase64Id != 0)
            {
                jsonData.LogoBase64 = null;
            }
            invoice.JsonData = JsonConvert.SerializeObject(jsonData);
            UpdateInvoiceJsonData(invoice.ID, invoice.JsonData);
        }

        public void UpdateInvoiceJsonDataAfterLinesUpdated(Invoice invoice)
        {
            var jsonData = InvoiceFormattedData.GetDataAfterLinesUpdated(invoice);
            if (jsonData.LogoBase64Id != 0)
            {
                jsonData.LogoBase64 = null;
            }
            UpdateInvoiceJsonData(invoice.ID, invoice.JsonData);
        }

        public virtual int UpdateInvoiceFileID(int invoiceId, int fileId)
        {
            return UpdateInvoiceFileIDInDb(invoiceId, fileId);
        }

        private int UpdateInvoiceFileIDInDb(int invoiceId, int fileId)
        {
            Db.ExecuteNonQuery(
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
            tenantSettings.Save();

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
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "invoice.*"));

            DeleteBatchInvoicesExecute(new List<Invoice> { invoice });

            return invoice;
        }

        public List<Invoice> DeleteBatchInvoices(int[] invoiceID)
        {
            var invoices = GetInvoices(invoiceID).Where(CRMSecurity.CanDelete).ToList();
            if (!invoices.Any()) return invoices;

            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "invoice.*"));

            DeleteBatchInvoicesExecute(invoices);

            return invoices;
        }

        private void DeleteBatchInvoicesExecute(List<Invoice> invoices)
        {

            var invoiceID = invoices.Select(x => x.ID).ToArray();

            using (var tx = Db.BeginTransaction())
            {
                Db.ExecuteNonQuery(Delete("crm_invoice_line").Where(Exp.In("invoice_id", invoiceID)));
                Db.ExecuteNonQuery(Delete("crm_invoice").Where(Exp.In("id", invoiceID)));

                tx.Commit();
            }
            invoices.ForEach(invoice =>  FactoryIndexer<InvoicesWrapper>.DeleteAsync(invoice));
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

        private SqlQuery GetInvoiceSqlQuery(Exp where, String alias)
        {
            var q = Query("crm_invoice");

            if (!string.IsNullOrEmpty(alias))
            {
                q = new SqlQuery("crm_invoice " + alias)
                    .Where(alias + ".tenant_id", TenantID);
                q.Select(GetInvoiceColumnsTable(alias));

            }
            else
            {
                q.Select(GetInvoiceColumnsTable(String.Empty));
            }

            if (where != null)
                q.Where(where);

            return q;
        }

        private String[] GetInvoiceColumnsTable(String alias)
        {
            var result = new string[]
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

            return string.IsNullOrEmpty(alias) ? result : result.Select(c => alias + "." + c).ToArray();
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
            var tblAliasPrefix = !String.IsNullOrEmpty(tblAlias) ? tblAlias + "." : "";
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
                {
                    List<int> invoicesIds;
                    if (!FactoryIndexer<InvoicesWrapper>.TrySelectIds(s => s.MatchAll(searchText), out invoicesIds))
                    {
                        conditions.Add(BuildLike(new[] {tblAliasPrefix + "number", tblAliasPrefix + "description"}, keywords));
                    }
                    else
                    {
                        conditions.Add(Exp.In(tblAliasPrefix + "id", invoicesIds));
                    }
                }
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

            if (!String.IsNullOrEmpty(currency))
            {
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


        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="creationDate"></param>
        public void SetInvoiceCreationDate(int invoiceId, DateTime creationDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_invoice")
                    .Set("create_on", TenantUtil.DateTimeToUtc(creationDate))
                    .Where(Exp.Eq("id", invoiceId)));
            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "invoice.*"));
        }

        /// <summary>
        /// Test method
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <param name="lastModifedDate"></param>
        public void SetInvoiceLastModifedDate(int invoiceId, DateTime lastModifedDate)
        {
            Db.ExecuteNonQuery(
                Update("crm_invoice")
                    .Set("last_modifed_on", TenantUtil.DateTimeToUtc(lastModifedDate))
                    .Where(Exp.Eq("id", invoiceId)));
            // Delete relative  keys
            _cache.Remove(new Regex(TenantID.ToString(CultureInfo.InvariantCulture) + "invoice.*"));
        }

        public decimal CalculateInvoiceCost(IEnumerable<InvoiceLine> lines, IEnumerable<InvoiceTax> taxes)
        {
            decimal cost = 0;
            foreach (var line in lines)
            {
                var linePrice = Math.Round(line.Price * line.Quantity, 2);
                var lineDiscount = Math.Round(linePrice * line.Discount / 100, 2);

                linePrice -= lineDiscount;

                decimal lineTax1 = 0;
                if (line.InvoiceTax1ID > 0)
                {
                    var tax1 = taxes.FirstOrDefault(tax => tax.ID == line.InvoiceTax1ID);
                    if (tax1 != null)
                    {
                        lineTax1 = Math.Round(linePrice * tax1.Rate / 100, 2);
                    }
                }

                decimal lineTax2 = 0;
                if (line.InvoiceTax2ID > 0)
                {
                    var tax2 = taxes.FirstOrDefault(tax => tax.ID == line.InvoiceTax2ID);
                    if (tax2 != null)
                    {
                        lineTax2 = Math.Round(linePrice * tax2.Rate / 100, 2);
                    }
                }

                cost += linePrice + lineTax1 + lineTax2;
            }

            return Math.Round(cost, 2);
        }
    }
}