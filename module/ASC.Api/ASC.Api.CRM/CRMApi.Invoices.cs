/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Linq;
using System.Web;

using ASC.Api.Attributes;
using ASC.Api.Collections;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Documents;
using ASC.Api.Exceptions;
using ASC.Core;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Web.CRM.Classes;
using ASC.Web.Studio.Utility;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        /// <summary>
        ///  Returns the detailed information about an invoice with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="invoiceid">Invoice ID</param>
        /// <short>Get an invoice by ID</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceWrapper, ASC.Api.CRM">Invoice</returns>
        /// <path>api/2.0/crm/invoice/{invoiceid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/{invoiceid:[0-9]+}")]
        public InvoiceWrapper GetInvoiceByID(int invoiceid)
        {
            if (invoiceid <= 0) throw new ArgumentException();

            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            return ToInvoiceWrapper(invoice);
        }

        /// <summary>
        ///  Returns the detailed information about an invoice sample.
        /// </summary>
        /// <short>Get an invoice sample</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceWrapper, ASC.Api.CRM">Invoice</returns>
        /// <path>api/2.0/crm/invoice/sample</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/sample")]
        public InvoiceWrapper GetInvoiceSample()
        {
            var sample = InvoiceWrapper.GetSample();
            sample.Number = DaoFactory.InvoiceDao.GetNewInvoicesNumber();
            sample.Terms = DaoFactory.InvoiceDao.GetSettings().Terms ?? string.Empty;

            sample.IssueDate = (ApiDateTime)DateTime.UtcNow;
            sample.DueDate = (ApiDateTime)DateTime.UtcNow.AddDays(30);
            sample.CreateOn = (ApiDateTime)DateTime.UtcNow;

            sample.Currency = new CurrencyInfoWrapper(Global.TenantSettings.DefaultCurrency);

            sample.InvoiceLines.First().Quantity = 1;

            return sample;
        }

        /// <summary>
        ///  Returns the JSON data of an invoice with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="invoiceid">Invoice ID</param>
        /// <short>Get the invoice JSON data</short> 
        /// <category>Invoices</category>
        /// <returns>Invoice JSON data</returns>
        /// <path>api/2.0/crm/invoice/jsondata/{invoiceid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/jsondata/{invoiceid:[0-9]+}")]
        public string GetInvoiceJsonData(int invoiceid)
        {
            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            return invoice.JsonData;
        }

        /// <summary>
        /// Returns a list of invoices matching the parameters specified in the request.
        /// </summary>
        /// <param type="System.Nullable{ASC.CRM.Core.InvoiceStatus}, System" method="url" name="status">Invoice status</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="issueDateFrom">Invoice start issue date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="issueDateTo">Invoice end issue date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="dueDateFrom">Invoice start due date</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" method="url" name="dueDateTo">Invoice end due date</param>
        /// <param type="System.String, System" method="url" name="entityType">Invoice entity type</param>
        /// <param type="System.Int32, System" method="url" name="entityid">Invoice entity ID</param>
        /// <param type="System.String, System" method="url" name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by API">Invoice currency (abbreviation)</param>
        /// <short>Get invoices</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceBaseWrapper, ASC.Api.CRM">List of invoices</returns>
        /// <path>api/2.0/crm/invoice/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"invoice/filter")]
        public IEnumerable<InvoiceBaseWrapper> GetInvoices(
            InvoiceStatus? status,
            ApiDateTime issueDateFrom,
            ApiDateTime issueDateTo,
            ApiDateTime dueDateFrom,
            ApiDateTime dueDateTo,
            String entityType,
            int entityid,
            String currency
            )
        {
            if (!String.IsNullOrEmpty(entityType) && !(
                                                          String.Compare(entityType, "contact", true) == 0 ||
                                                          String.Compare(entityType, "opportunity", true) == 0 ||
                                                          String.Compare(entityType, "case", true) == 0))
                throw new ArgumentException();

            IEnumerable<InvoiceBaseWrapper> result;

            InvoiceSortedByType sortBy;

            OrderBy invoiceOrderBy;

            var searchString = _context.FilterValue;

            if (InvoiceSortedByType.TryParse(_context.SortBy, true, out sortBy))
            {
                invoiceOrderBy = new OrderBy(sortBy, !_context.SortDescending);
            }
            else if (String.IsNullOrEmpty(_context.SortBy))
            {
                invoiceOrderBy = new OrderBy(InvoiceSortedByType.Number, true);
            }
            else
            {
                invoiceOrderBy = null;
            }

            var fromIndex = (int)_context.StartIndex;
            var count = (int)_context.Count;

            if (invoiceOrderBy != null)
            {
                result = ToListInvoiceBaseWrappers(
                    DaoFactory.InvoiceDao.GetInvoices(
                        searchString,
                        status,
                        issueDateFrom, issueDateTo,
                        dueDateFrom, dueDateTo,
                        ToEntityType(entityType), entityid,
                        currency,
                        fromIndex, count,
                        invoiceOrderBy));

                _context.SetDataPaginated();
                _context.SetDataFiltered();
                _context.SetDataSorted();
            }
            else
            {
                result = ToListInvoiceBaseWrappers(
                    DaoFactory.InvoiceDao.GetInvoices(
                        searchString,
                        status,
                        issueDateFrom, issueDateTo,
                        dueDateFrom, dueDateTo,
                        ToEntityType(entityType), entityid,
                        currency,
                        0,
                        0,
                        null));
            }

            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory.InvoiceDao.GetInvoicesCount(
                    searchString,
                    status,
                    issueDateFrom, issueDateTo,
                    dueDateFrom, dueDateTo,
                    ToEntityType(entityType), entityid,
                    currency);
            }

            _context.SetTotalCount(totalCount);

            return result.ToSmartList();
        }

        /// <summary>
        ///  Returns a list of all the invoices related to the entity with the ID and type specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="entityType">Invoice entity type</param>
        /// <param type="System.Int32, System" method="url" name="entityid">Invoice entity ID</param>
        /// <short>Get entity invoices</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM">List of invoices</returns>
        /// <path>api/2.0/crm/{entityType}/invoicelist/{entityid}</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"{entityType:(contact|person|company|opportunity)}/invoicelist/{entityid:[0-9]+}")]
        public IEnumerable<InvoiceBaseWrapper> GetEntityInvoices(String entityType, int entityid)
        {
            if (String.IsNullOrEmpty(entityType) || entityid <= 0) throw new ArgumentException();

            return ToListInvoiceBaseWrappers(DaoFactory.InvoiceDao.GetEntityInvoices(ToEntityType(entityType), entityid));
        }

        /// <summary>
        /// Updates a status of invoices with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Int32[], System" name="invoiceids">List of invoice IDs</param>
        /// <param type="ASC.CRM.Core.InvoiceStatus, ASC.CRM.Core" method="url" name="status">New invoice status</param>
        /// <short>Update an invoice group status</short> 
        /// <category>Invoices</category>
        /// <returns>Invoice information</returns>
        /// <path>api/2.0/crm/invoice/status/{status}</path>
        /// <httpMethod>PUT</httpMethod>
        /// <collection>list</collection>
        [Update(@"invoice/status/{status:[\w\d-]+}")]
        public KeyValuePair<IEnumerable<InvoiceBaseWrapper>, IEnumerable<InvoiceItemWrapper>> UpdateInvoiceBatchStatus(
            int[] invoiceids,
            InvoiceStatus status
            )
        {
            if (invoiceids == null || !invoiceids.Any()) throw new ArgumentException();

            var oldInvoices = DaoFactory.InvoiceDao.GetByID(invoiceids).Where(CRMSecurity.CanAccessTo).ToList();

            var updatedInvoices = DaoFactory.InvoiceDao.UpdateInvoiceBatchStatus(oldInvoices.ToList().Select(i => i.ID).ToArray(), status);

            // detect what really changed
            var realUpdatedInvoices = updatedInvoices
                .Select(t => oldInvoices.FirstOrDefault(x => x.ID == t.ID && x.Status != t.Status))
                .Where(inv => inv != null)
                .ToList();

            if (realUpdatedInvoices.Any())
            {
                MessageService.Send(Request, MessageAction.InvoicesUpdatedStatus, MessageTarget.Create(realUpdatedInvoices.Select(x => x.ID)), realUpdatedInvoices.Select(x => x.Number), status.ToLocalizedString());
            }

            var invoiceItemsUpdated = new List<InvoiceItem>();

            if (status == InvoiceStatus.Sent || status == InvoiceStatus.Rejected)
            {
                var invoiceItemsAll = DaoFactory.InvoiceItemDao.GetAll();
                var invoiceItemsWithTrackInventory = invoiceItemsAll.Where(item => item.TrackInventory).ToList();

                if (status == InvoiceStatus.Sent && invoiceItemsWithTrackInventory != null && invoiceItemsWithTrackInventory.Count != 0)
                {
                    foreach (var inv in updatedInvoices)
                    {
                        if (inv.Status == InvoiceStatus.Sent)
                        {
                            //could be changed
                            var oldInv = oldInvoices.FirstOrDefault(i => i.ID == inv.ID);
                            if (oldInv != null && oldInv.Status == InvoiceStatus.Draft)
                            {
                                //was changed to Sent
                                var invoiceLines = DaoFactory.InvoiceLineDao.GetInvoiceLines(inv.ID);

                                foreach (var line in invoiceLines)
                                {
                                    var item = invoiceItemsWithTrackInventory.FirstOrDefault(ii => ii.ID == line.InvoiceItemID);
                                    if (item != null)
                                    {
                                        item.StockQuantity -= line.Quantity;
                                        DaoFactory.InvoiceItemDao.SaveOrUpdateInvoiceItem(item);
                                        var oldItem = invoiceItemsUpdated.Find(i => i.ID == item.ID);
                                        if (oldItem != null)
                                        {
                                            invoiceItemsUpdated.Remove(oldItem);
                                        }
                                        invoiceItemsUpdated.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }

                if (status == InvoiceStatus.Rejected && invoiceItemsWithTrackInventory != null && invoiceItemsWithTrackInventory.Count != 0)
                {
                    foreach (var inv in updatedInvoices)
                    {
                        if (inv.Status == InvoiceStatus.Rejected)
                        {
                            //could be changed
                            var oldInv = oldInvoices.FirstOrDefault(i => i.ID == inv.ID);
                            if (oldInv != null && oldInv.Status == InvoiceStatus.Sent)
                            {
                                //was changed from Sent to Rejectes
                                var invoiceLines = DaoFactory.InvoiceLineDao.GetInvoiceLines(inv.ID);

                                foreach (var line in invoiceLines)
                                {
                                    var item = invoiceItemsWithTrackInventory.FirstOrDefault(ii => ii.ID == line.InvoiceItemID);
                                    if (item != null)
                                    {
                                        item.StockQuantity += line.Quantity;
                                        DaoFactory.InvoiceItemDao.SaveOrUpdateInvoiceItem(item);
                                        var oldItem = invoiceItemsUpdated.Find(i => i.ID == item.ID);
                                        if (oldItem != null)
                                        {
                                            invoiceItemsUpdated.Remove(oldItem);
                                        }
                                        invoiceItemsUpdated.Add(item);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            var listInvoiceBaseWrappers = ToListInvoiceBaseWrappers(updatedInvoices);

            return new KeyValuePair<IEnumerable<InvoiceBaseWrapper>, IEnumerable<InvoiceItemWrapper>>(listInvoiceBaseWrappers, invoiceItemsUpdated.ConvertAll(i => ToInvoiceItemWrapper(i)));
        }

        /// <summary>
        /// Deletes an invoice with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="invoiceid">Invoice ID</param>
        /// <short>Delete an invoice</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceBaseWrapper, ASC.Api.CRM">Invoice</returns>
        /// <path>api/2.0/crm/invoice/{invoiceid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"invoice/{invoiceid:[0-9]+}")]
        public InvoiceBaseWrapper DeleteInvoice(int invoiceid)
        {
            if (invoiceid <= 0) throw new ArgumentException();

            var invoice = DaoFactory.InvoiceDao.DeleteInvoice(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            MessageService.Send(Request, MessageAction.InvoiceDeleted, MessageTarget.Create(invoice.ID), invoice.Number);
            return ToInvoiceBaseWrapper(invoice);
        }

        /// <summary>
        /// Deletes a group of invoices with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="invoiceids">List of invoice IDs</param>
        /// <short>Delete invoices</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceBaseWrapper, ASC.Api.CRM">List of invoices</returns>
        /// <path>api/2.0/crm/invoice</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
        [Delete(@"invoice")]
        public IEnumerable<InvoiceBaseWrapper> DeleteBatchInvoices(IEnumerable<int> invoiceids)
        {
            if (invoiceids == null || !invoiceids.Any()) throw new ArgumentException();

            var invoices = DaoFactory.InvoiceDao.DeleteBatchInvoices(invoiceids.ToArray());
            MessageService.Send(Request, MessageAction.InvoicesDeleted, MessageTarget.Create(invoices.Select(x => x.ID)), invoices.Select(x => x.Number));

            return ToListInvoiceBaseWrappers(invoices);
        }

        /// <summary>
        ///  Creates an invoice with the parameters (contact ID, consignee ID, etc.) specified in the request.
        /// </summary>
        /// <param type="System.String, System" optional="false" name="number">Invoice number</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="false" name="issueDate">Invoice issue date</param>
        /// <param type="System.Int32, System" optional="true" name="templateType">Invoice template type</param>
        /// <param type="System.Int32, System" optional="false" name="contactId">Invoice contact ID</param>
        /// <param type="System.Int32, System" optional="true" name="consigneeId">Invoice consignee ID</param>
        /// <param type="System.Int32, System" optional="true" name="entityId">Invoice entity ID</param>
        /// <param type="System.Int32, System" optional="true" name="billingAddressID">Invoice billing address ID</param>
        /// <param type="System.Int32, System" optional="true" name="deliveryAddressID">Invoice delivery address ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="false" name="dueDate">Invoice due date</param>
        /// <param type="System.String, System" optional="false" name="language">Invoice language</param>
        /// <param type="System.String, System" optional="false" name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by API">Invoice currency (abbreviation)</param>
        /// <param type="System.Decimal, System" optional="false" name="exchangeRate">Invoice exchange rate</param>
        /// <param type="System.String, System" optional="true" name="purchaseOrderNumber">Invoice purchase order number</param>
        /// <param type="System.String, System" optional="false" name="terms">Invoice terms</param>
        /// <param type="System.String, System" optional="true" name="description">Invoice description</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.CRM.Core.Entities.InvoiceLine}, System.Collections.Generic" optional="false" name="invoiceLines">List of invoice lines</param>
        /// <short>Create an invoice</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceWrapper, ASC.Api.CRM">Invoice</returns>
        /// <example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// data: {
        ///    number: "invoice000001",
        ///    issueDate: "2015-06-01T00:00:00",
        ///    contactId: 10,
        ///    dueDate: "2025-06-01T00:00:00",
        ///    language: "es-ES",
        ///    currency: "rub",
        ///    exchangeRate: 54.32,
        ///    terms: "Terms for this invoice",
        ///    invoiceLines:
        ///    [{
        ///          invoiceItemID: 1,
        ///          invoiceTax1ID: 1,
        ///          invoiceTax2ID: 2,
        ///          description: "description for invoice line 1",
        ///          quantity: 100,
        ///          price: 7.7,
        ///          discount: 25
        ///    }]  
        /// }
        /// 
        /// where invoiceItemID, invoiceTax1ID, invoiceTax2ID - IDs of the real existing invoice item and invoice taxes, contactId - ID of the existing contact.
        /// 
        /// ]]>
        /// </example>
        /// <path>api/2.0/crm/invoice</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"invoice")]
        public InvoiceWrapper CreateInvoice(
            string number,
            ApiDateTime issueDate,
            int templateType,
            int contactId,
            int consigneeId,
            int entityId,
            int billingAddressID,
            int deliveryAddressID,
            ApiDateTime dueDate,
            string language,
            string currency,
            decimal exchangeRate,
            string purchaseOrderNumber,
            string terms,
            string description,
            IEnumerable<InvoiceLine> invoiceLines
            )
        {
            var invoiceLinesList = invoiceLines != null ? invoiceLines.ToList() : new List<InvoiceLine>();
            if (!invoiceLinesList.Any() || !IsLinesForInvoiceCorrect(invoiceLinesList)) throw new ArgumentException();

            var invoice = new Invoice
            {
                Status = InvoiceStatus.Draft,
                Number = number,
                IssueDate = issueDate,
                TemplateType = (InvoiceTemplateType)templateType,
                ContactID = contactId,
                ConsigneeID = consigneeId,
                EntityType = EntityType.Opportunity,
                EntityID = entityId,
                DueDate = dueDate,
                Language = language,
                Currency = !String.IsNullOrEmpty(currency) ? currency.ToUpper() : null,
                ExchangeRate = exchangeRate,
                PurchaseOrderNumber = purchaseOrderNumber,
                Terms = terms,
                Description = description
            };

            CRMSecurity.DemandCreateOrUpdate(invoice);

            if (billingAddressID > 0)
            {
                var address = DaoFactory.ContactInfoDao.GetByID(billingAddressID);
                if (address == null || address.InfoType != ContactInfoType.Address || address.Category != (int)AddressCategory.Billing || address.ContactID != contactId)
                    throw new ArgumentException();
            }

            if (deliveryAddressID > 0)
            {
                var address = DaoFactory.ContactInfoDao.GetByID(deliveryAddressID);
                if (address == null || address.InfoType != ContactInfoType.Address || address.Category != (int)AddressCategory.Postal || address.ContactID != consigneeId)
                    throw new ArgumentException();
            }


            invoice.ID = DaoFactory.InvoiceDao.SaveOrUpdateInvoice(invoice);

            CreateInvoiceLines(invoiceLinesList, invoice);

            DaoFactory.InvoiceDao.UpdateInvoiceJsonData(invoice, billingAddressID, deliveryAddressID);
            return ToInvoiceWrapper(invoice);
        }


        private bool IsLinesForInvoiceCorrect(List<InvoiceLine> invoiceLines)
        {
            foreach (var line in invoiceLines)
            {
                if (line.InvoiceItemID <= 0 ||
                    line.Quantity < 0 || line.Price < 0 ||
                    line.Discount < 0 || line.Discount > 100 ||
                    line.InvoiceTax1ID < 0 || line.InvoiceTax2ID < 0)
                    return false;
                if (!DaoFactory.InvoiceItemDao.IsExist(line.InvoiceItemID))
                    return false;

                if (line.InvoiceTax1ID > 0 && !DaoFactory.InvoiceTaxDao.IsExist(line.InvoiceTax1ID))
                    return false;

                if (line.InvoiceTax2ID > 0 && !DaoFactory.InvoiceTaxDao.IsExist(line.InvoiceTax2ID))
                    return false;
            }
            return true;
        }

        private List<InvoiceLine> CreateInvoiceLines(List<InvoiceLine> invoiceLines, Invoice invoice)
        {
            var result = new List<InvoiceLine>();
            for (var i = 0; i < invoiceLines.Count; i++)
            {
                var line = new InvoiceLine
                {
                    ID = 0,
                    InvoiceID = invoice.ID,
                    InvoiceItemID = invoiceLines[i].InvoiceItemID,
                    InvoiceTax1ID = invoiceLines[i].InvoiceTax1ID,
                    InvoiceTax2ID = invoiceLines[i].InvoiceTax2ID,
                    SortOrder = i,
                    Description = invoiceLines[i].Description,
                    Quantity = invoiceLines[i].Quantity,
                    Price = invoiceLines[i].Price,
                    Discount = invoiceLines[i].Discount
                };

                line.ID = DaoFactory.InvoiceLineDao.SaveOrUpdateInvoiceLine(line);
                result.Add(line);
            }
            return result;
        }

        /// <summary>
        ///  Updates the selected invoice with the parameters (contact ID, consignee ID, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" optional="false" name="id">Invoice ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" optional="false" name="issueDate">New invoice issue date</param>
        /// <param type="System.Int32, System" optional="true" name="templateType">New invoice template type</param>
        /// <param type="System.Int32, System" optional="false" name="contactId">New invoice contact ID</param>
        /// <param type="System.Int32, System" optional="true" name="consigneeId">New invoice consignee ID</param>
        /// <param type="System.Int32, System" optional="true" name="entityId">New invoice entity ID</param>
        /// <param type="System.Int32, System" optional="true" name="billingAddressID">New invoice billing address ID</param>
        /// <param type="System.Int32, System" optional="true" name="deliveryAddressID">New invoice delivery address ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="dueDate">New invoice due date</param>
        /// <param type="System.String, System" optional="false" name="language">New invoice language</param>
        /// <param type="System.String, System" optional="false" name="currency" remark="Allowed values: EUR, RUB etc. You can get the whole list of available currencies by API">New invoice currency (abbreviation)</param>
        /// <param type="System.Decimal, System" optional="false" name="exchangeRate">New invoice exchange rate</param>
        /// <param type="System.String, System" optional="true" name="purchaseOrderNumber">New invoice purchase order number</param>
        /// <param type="System.String, System" optional="false" name="terms">New invoice terms</param>
        /// <param type="System.String, System" optional="true" name="description">New invoice description</param>
        /// <param type="System.Collections.Generic.IEnumerable{ASC.CRM.Core.Entities.InvoiceLine}, System.Collections.Generic" optional="false" name="invoiceLines">New list of invoice lines</param>
        /// <short>Update an invoice</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceWrapper, ASC.Api.CRM">Updated invoice</returns>
        /// <example>
        /// <![CDATA[
        /// 
        /// Data transfer in application/json format:
        /// 
        /// data: {
        ///    id: 5,
        ///    issueDate: "2015-06-01T00:00:00",
        ///    contactId: 10,
        ///    dueDate: "2025-06-01T00:00:00",
        ///    language: "es-ES",
        ///    currency: "rub",
        ///    exchangeRate: 54.32,
        ///    terms: "Terms for this invoice",
        ///    invoiceLines:
        ///    [{
        ///          invoiceItemID: 1,
        ///          invoiceTax1ID: 1,
        ///          invoiceTax2ID: 2,
        ///          description: "description for invoice line 1",
        ///          quantity: 100,
        ///          price: 7.7,
        ///          discount: 25
        ///    }]
        /// }
        /// 
        /// where invoiceItemID, invoiceTax1ID, invoiceTax2ID - IDs of the real existing invoice item and invoice taxes, contactId - ID of the existing contact.
        /// 
        /// ]]>
        /// </example>
        /// <path>api/2.0/crm/invoice/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"invoice/{id:[0-9]+}")]
        public InvoiceWrapper UpdateInvoice(
            int id,
            ApiDateTime issueDate,
            int templateType,
            int contactId,
            int consigneeId,
            int entityId,
            int billingAddressID,
            int deliveryAddressID,
            ApiDateTime dueDate,
            string language,
            string currency,
            decimal exchangeRate,
            string purchaseOrderNumber,
            string terms,
            string description,
            IEnumerable<InvoiceLine> invoiceLines)
        {

            var invoiceLinesList = invoiceLines != null ? invoiceLines.ToList() : new List<InvoiceLine>();
            if (!invoiceLinesList.Any() || !IsLinesForInvoiceCorrect(invoiceLinesList)) throw new ArgumentException();

            var invoice = DaoFactory.InvoiceDao.GetByID(id);
            if (invoice == null || !CRMSecurity.CanEdit(invoice)) throw new ItemNotFoundException();

            invoice.IssueDate = issueDate;
            invoice.TemplateType = (InvoiceTemplateType)templateType;
            invoice.ContactID = contactId;
            invoice.ConsigneeID = consigneeId;
            invoice.EntityType = EntityType.Opportunity;
            invoice.EntityID = entityId;
            invoice.DueDate = dueDate;
            invoice.Language = language;
            invoice.Currency = !String.IsNullOrEmpty(currency) ? currency.ToUpper() : null; ;
            invoice.ExchangeRate = exchangeRate;
            invoice.PurchaseOrderNumber = purchaseOrderNumber;
            invoice.Terms = terms;
            invoice.Description = description;
            invoice.JsonData = null;

            CRMSecurity.DemandCreateOrUpdate(invoice);

            if (billingAddressID > 0)
            {
                var address = DaoFactory.ContactInfoDao.GetByID(billingAddressID);
                if (address == null || address.InfoType != ContactInfoType.Address || address.Category != (int)AddressCategory.Billing || address.ContactID != contactId)
                    throw new ArgumentException();
            }

            if (deliveryAddressID > 0)
            {
                var address = DaoFactory.ContactInfoDao.GetByID(deliveryAddressID);
                if (address == null || address.InfoType != ContactInfoType.Address || address.Category != (int)AddressCategory.Postal || address.ContactID != consigneeId)
                    throw new ArgumentException();
            }

            DaoFactory.InvoiceDao.SaveOrUpdateInvoice(invoice);


            DaoFactory.InvoiceLineDao.DeleteInvoiceLines(invoice.ID);
            CreateInvoiceLines(invoiceLinesList, invoice);

            DaoFactory.InvoiceDao.UpdateInvoiceJsonData(invoice, billingAddressID, deliveryAddressID);

            if (Global.CanDownloadInvoices)
            {
                PdfQueueWorker.StartTask(HttpContext.Current, TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, invoice.ID);
            }

            return ToInvoiceWrapper(invoice);
        }

        /// <summary>
        ///  Returns the pdf file related to an invoice with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="invoiceid">Invoice ID</param>
        /// <short>Get the invoice pdf file</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.Documents.FileWrapper, ASC.Api.Documents">Pdf file</returns>
        /// <path>api/2.0/crm/invoice/{invoiceid}/pdf</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/{invoiceid:[0-9]+}/pdf")]
        public FileWrapper GetInvoicePdfExistOrCreate(int invoiceid)
        {
            if (invoiceid <= 0) throw new ArgumentException();

            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            return new FileWrapper(Global.GetInvoicePdfExistingOrCreate(invoice, DaoFactory));
        }

        /// <summary>
        ///  Returns information about the generation of the invoice pdf file.
        /// </summary>
        /// <param type="System.Int32, System" name="invoiceId">Invoice ID</param>
        /// <param type="System.String, System" name="storageUrl">Storage URL</param>
        /// <param type="System.String, System" name="revisionId">Revision ID</param>
        /// <short>Get invoice converter data</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Web.CRM.Classes.ConverterData, ASC.Web.CRM">Converter data</returns>
        /// <path>api/2.0/crm/invoice/converter/data</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"invoice/converter/data")]
        public ConverterData GetInvoiceConverterData(int invoiceId, string storageUrl, string revisionId)
        {
            if (invoiceId <= 0) throw new ArgumentException();

            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceId);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            var converterData = new ConverterData
            {
                StorageUrl = storageUrl,
                RevisionId = revisionId,
                InvoiceId = invoiceId
            };

            var existingFile = invoice.GetInvoiceFile(DaoFactory);
            if (existingFile != null)
            {
                converterData.FileId = invoice.FileID;
                return converterData;
            }

            if (string.IsNullOrEmpty(storageUrl) || string.IsNullOrEmpty(revisionId))
            {
                return PdfCreator.StartCreationFileAsync(invoice);
            }
            else
            {
                var convertedFile = PdfCreator.GetConvertedFile(converterData, DaoFactory);
                if (convertedFile != null)
                {
                    invoice.FileID = Int32.Parse(convertedFile.ID.ToString());
                    DaoFactory.InvoiceDao.UpdateInvoiceFileID(invoice.ID, invoice.FileID);
                    DaoFactory.RelationshipEventDao.AttachFiles(invoice.ContactID, invoice.EntityType, invoice.EntityID, new[] { invoice.FileID });

                    converterData.FileId = invoice.FileID;
                    return converterData;
                }
                else
                {
                    return converterData;
                }
            }
        }

        /// <summary>
        ///  Returns the existence of an invoice with the number specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="number">Invoice number</param>
        /// <short>Check invoice existence by number</short> 
        /// <category>Invoices</category>
        /// <returns>Invoice existence</returns>
        /// <path>api/2.0/crm/invoice/bynumber/exist</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/bynumber/exist")]
        public Boolean GetInvoiceByNumberExistence(string number)
        {
            if (String.IsNullOrEmpty(number)) throw new ArgumentException();
            return DaoFactory.InvoiceDao.IsExist(number);
        }

        /// <summary>
        ///  Returns the detailed information about an invoice with the number specified in the request.
        /// </summary>
        /// <param type="System.String, System" method="url" name="number">Invoice number</param>
        /// <short>Get an invoice by number</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceWrapper, ASC.Api.CRM">Invoice</returns>
        /// <path>api/2.0/crm/invoice/bynumber</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/bynumber")]
        public InvoiceWrapper GetInvoiceByNumber(string number)
        {
            if (String.IsNullOrEmpty(number)) throw new ArgumentException();

            var invoice = DaoFactory.InvoiceDao.GetByNumber(number);
            if (invoice == null) throw new ItemNotFoundException();
            if (!CRMSecurity.CanAccessTo(invoice))
            {
                throw CRMSecurity.CreateSecurityException();
            }

            return new InvoiceWrapper(invoice);
        }

        /// <summary>
        /// Returns a list of invoice items matching the parameters specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="status">Invoice status</param>
        /// <param type="System.Nullable{System.Boolean}, System" method="url" optional="true" name="inventoryStock">Specifies if the inventory is tracked or not</param>
        /// <short>Get filtered invoice items</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceItemWrapper, ASC.Api.CRM">List of invoice items</returns>
        /// <path>api/2.0/crm/invoiceitem/filter</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"invoiceitem/filter")]
        public IEnumerable<InvoiceItemWrapper> GetInvoiceItems(int status, bool? inventoryStock)
        {
            IEnumerable<InvoiceItemWrapper> result;

            InvoiceItemSortedByType sortBy;

            OrderBy invoiceOrderBy;

            var searchString = _context.FilterValue;

            if (InvoiceItemSortedByType.TryParse(_context.SortBy, true, out sortBy))
            {
                invoiceOrderBy = new OrderBy(sortBy, !_context.SortDescending);
            }
            else if (String.IsNullOrEmpty(_context.SortBy))
            {
                invoiceOrderBy = new OrderBy(InvoiceItemSortedByType.Name, true);
            }
            else
            {
                invoiceOrderBy = null;
            }

            var fromIndex = (int)_context.StartIndex;
            var count = (int)_context.Count;

            if (invoiceOrderBy != null)
            {
                result = DaoFactory.InvoiceItemDao.GetInvoiceItems(
                    searchString,
                    status,
                    inventoryStock,
                    fromIndex, count,
                    invoiceOrderBy)
                                   .ConvertAll(ToInvoiceItemWrapper);

                _context.SetDataPaginated();
                _context.SetDataFiltered();
                _context.SetDataSorted();
            }
            else
            {
                result = DaoFactory.InvoiceItemDao.GetInvoiceItems(
                    searchString,
                    status,
                    inventoryStock,
                    0, 0,
                    null)
                                   .ConvertAll(ToInvoiceItemWrapper);
            }

            int totalCount;

            if (result.Count() < count)
            {
                totalCount = fromIndex + result.Count();
            }
            else
            {
                totalCount = DaoFactory.InvoiceItemDao.GetInvoiceItemsCount(
                    searchString,
                    status,
                    inventoryStock);
            }

            _context.SetTotalCount(totalCount);

            return result.ToSmartList();
        }

        /// <summary>
        ///  Returns the detailed information about an invoice item with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="invoiceitemid">Invoice item ID</param>
        /// <short>Get an invoice item by ID</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceItemWrapper, ASC.Api.CRM">Invoice item</returns>
        /// <path>api/2.0/crm/invoiceitem/{invoiceitemid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoiceitem/{invoiceitemid:[0-9]+}")]
        public InvoiceItemWrapper GetInvoiceItemByID(int invoiceitemid)
        {
            if (invoiceitemid <= 0) throw new ArgumentException();

            var invoiceItem = DaoFactory.InvoiceItemDao.GetByID(invoiceitemid);
            if (invoiceItem == null) throw new ItemNotFoundException();

            return ToInvoiceItemWrapper(invoiceItem);
        }

        /// <summary>
        ///  Creates an invoice line with the parameters (invoice ID, invoice item ID, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" optional="false" name="invoiceId">Invoice ID</param>
        /// <param type="System.Int32, System" optional="false" name="invoiceItemId">Invoice item ID</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax1Id">First invoice tax ID</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax2Id">Second invoice tax ID</param>
        /// <param type="System.Int32, System" optional="true" name="sortOrder">Sort order</param>
        /// <param type="System.String, System" optional="true" name="description">Description</param>
        /// <param type="System.Decimal, System" optional="true" name="quantity">Quantity</param>
        /// <param type="System.Decimal, System" optional="true" name="price">Price</param>
        /// <param type="System.Decimal, System" optional="true" name="discount">Discount</param>
        /// <short>Create an invoice line</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceLineWrapper, ASC.Api.CRM">Invoice line</returns>
        /// <path>api/2.0/crm/invoiceline</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"invoiceline")]
        public InvoiceLineWrapper CreateInvoiceLine(
            int invoiceId,
            int invoiceItemId,
            int invoiceTax1Id,
            int invoiceTax2Id,
            int sortOrder,
            string description,
            decimal quantity,
            decimal price,
            decimal discount
            )
        {
            var invoiceLine = new InvoiceLine
            {
                InvoiceID = invoiceId,
                InvoiceItemID = invoiceItemId,
                InvoiceTax1ID = invoiceTax1Id,
                InvoiceTax2ID = invoiceTax2Id,
                SortOrder = sortOrder,
                Description = description,
                Quantity = quantity,
                Price = price,
                Discount = discount
            };

            if (invoiceId <= 0)
                throw new ArgumentException();
            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceId);
            CRMSecurity.DemandCreateOrUpdate(invoiceLine, invoice);

            invoiceLine.ID = DaoFactory.InvoiceLineDao.SaveOrUpdateInvoiceLine(invoiceLine);

            DaoFactory.InvoiceDao.UpdateInvoiceJsonDataAfterLinesUpdated(invoice);
            if (Global.CanDownloadInvoices)
            {
                PdfQueueWorker.StartTask(HttpContext.Current, TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, invoice.ID);
            }

            return ToInvoiceLineWrapper(invoiceLine);
        }

        /// <summary>
        ///  Updates the selected invoice line with the parameters (invoice ID, invoice item ID, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" optional="false" name="id">Invoice line ID</param>
        /// <param type="System.Int32, System" optional="false" name="invoiceId">Invoice ID</param>
        /// <param type="System.Int32, System" optional="false" name="invoiceItemId">Invoice item ID</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax1Id">New first invoice tax ID</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax2Id">New second invoice tax ID</param>
        /// <param type="System.Int32, System" optional="true" name="sortOrder">New sort order</param>
        /// <param type="System.String, System" optional="true" name="description">New description</param>
        /// <param type="System.Decimal, System" optional="true" name="quantity">New quantity</param>
        /// <param type="System.Decimal, System" optional="true" name="price">New price</param>
        /// <param type="System.Decimal, System" optional="true" name="discount">New discount</param>
        /// <short>Update an invoice line</short>
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceLineWrapper, ASC.Api.CRM">Updated invoice line</returns>
        /// <path>api/2.0/crm/invoiceline/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"invoiceline/{id:[0-9]+}")]
        public InvoiceLineWrapper UpdateInvoiceLine(
            int id,
            int invoiceId,
            int invoiceItemId,
            int invoiceTax1Id,
            int invoiceTax2Id,
            int sortOrder,
            string description,
            decimal quantity,
            decimal price,
            decimal discount
            )
        {
            if (invoiceId <= 0)
                throw new ArgumentException();

            var invoiceLine = DaoFactory.InvoiceLineDao.GetByID(id);
            if (invoiceLine == null || invoiceLine.InvoiceID != invoiceId) throw new ItemNotFoundException();


            invoiceLine.InvoiceID = invoiceId;
            invoiceLine.InvoiceItemID = invoiceItemId;
            invoiceLine.InvoiceTax1ID = invoiceTax1Id;
            invoiceLine.InvoiceTax2ID = invoiceTax2Id;
            invoiceLine.SortOrder = sortOrder;
            invoiceLine.Description = description;
            invoiceLine.Quantity = quantity;
            invoiceLine.Price = price;
            invoiceLine.Discount = discount;

            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceId);
            CRMSecurity.DemandCreateOrUpdate(invoiceLine, invoice);

            DaoFactory.InvoiceLineDao.SaveOrUpdateInvoiceLine(invoiceLine);

            DaoFactory.InvoiceDao.UpdateInvoiceJsonDataAfterLinesUpdated(invoice);
            if (Global.CanDownloadInvoices)
            {
                PdfQueueWorker.StartTask(HttpContext.Current, TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, invoice.ID);
            }

            return ToInvoiceLineWrapper(invoiceLine);
        }

        /// <summary>
        /// Deletes an invoice line with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" optional="false" name="id">Invoice line ID</param>
        /// <short>Delete an invoice line</short> 
        /// <category>Invoices</category>
        /// <returns>Invoice line ID</returns>
        /// <path>api/2.0/crm/invoiceline/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"invoiceline/{id:[0-9]+}")]
        public int DeleteInvoiceLine(int id)
        {
            var invoiceLine = DaoFactory.InvoiceLineDao.GetByID(id);
            if (invoiceLine == null) throw new ItemNotFoundException();
            if (!DaoFactory.InvoiceLineDao.CanDelete(invoiceLine.ID)) throw new Exception("Can't delete invoice line");

            var invoice = DaoFactory.InvoiceDao.GetByID(invoiceLine.InvoiceID);
            if (invoice == null) throw new ItemNotFoundException();
            if (!CRMSecurity.CanEdit(invoice)) throw CRMSecurity.CreateSecurityException();

            DaoFactory.InvoiceLineDao.DeleteInvoiceLine(id);

            DaoFactory.InvoiceDao.UpdateInvoiceJsonDataAfterLinesUpdated(invoice);
            if (Global.CanDownloadInvoices)
            {
                PdfQueueWorker.StartTask(HttpContext.Current, TenantProvider.CurrentTenantID, SecurityContext.CurrentAccount.ID, invoice.ID);
            }

            return id;
        }

        private InvoiceBaseWrapper ToInvoiceBaseWrapper(Invoice invoice)
        {
            var result = new InvoiceBaseWrapper(invoice);

            if (invoice.ContactID > 0)
            {
                result.Contact = ToContactBaseWrapperQuick(DaoFactory.ContactDao.GetByID(invoice.ContactID));
            }

            if (invoice.ConsigneeID > 0)
            {
                result.Consignee = ToContactBaseWrapperQuick(DaoFactory.ContactDao.GetByID(invoice.ConsigneeID));
            }

            if (invoice.EntityID > 0)
            {
                result.Entity = ToEntityWrapper(invoice.EntityType, invoice.EntityID);
            }

            result.Cost = invoice.GetInvoiceCost(DaoFactory);

            return result;
        }

        /// <summary>
        ///  Creates an invoice item with the parameters (title, description, price, etc.) specified in the request.
        /// </summary>
        /// <param type="System.String, System" optional="false" name="title">Invoice item title</param>
        /// <param type="System.String, System" optional="true" name="description">Invoice item description</param>
        /// <param type="System.Decimal, System" optional="false" name="price">Invoice item price</param>
        /// <param type="System.String, System" optional="true" name="sku">Invoice item stock keeping unit</param>
        /// <param type="System.Decimal, System" optional="true" name="stockQuantity">Invoice item stock quantity</param>
        /// <param type="System.Boolean, System" optional="true" name="trackInventory">Specifies if the inventory is tracked or not</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax1id">First invoice item tax ID</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax2id">Second invoice item tax ID</param>
        /// <short>Create an invoice item</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceItemWrapper, ASC.Api.CRM">Invoice item</returns>
        /// <path>api/2.0/crm/invoiceitem</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"invoiceitem")]
        public InvoiceItemWrapper CreateInvoiceItem(
            string title,
            string description,
            decimal price,
            string sku,
            decimal stockQuantity,
            bool trackInventory,
            int invoiceTax1id,
            int invoiceTax2id)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (String.IsNullOrEmpty(title) || price <= 0) throw new ArgumentException();

            var invoiceItem = new InvoiceItem
            {
                Title = title,
                Description = description,
                Price = price,
                StockKeepingUnit = sku,
                StockQuantity = stockQuantity,
                TrackInventory = trackInventory,
                InvoiceTax1ID = invoiceTax1id,
                InvoiceTax2ID = invoiceTax2id
            };

            invoiceItem = DaoFactory.InvoiceItemDao.SaveOrUpdateInvoiceItem(invoiceItem);
            MessageService.Send(Request, MessageAction.InvoiceItemCreated, MessageTarget.Create(invoiceItem.ID), invoiceItem.Title);

            return ToInvoiceItemWrapper(invoiceItem);
        }

        /// <summary>
        /// Updates the selected invoice item with the parameters (title, description, price, etc.) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" optional="false" name="id">Invoice item ID</param>
        /// <param type="System.String, System" optional="false" name="title">New invoice item title</param>
        /// <param type="System.String, System" optional="true" name="description">New invoice item description</param>
        /// <param type="System.Decimal, System" optional="false" name="price">New invoice item price</param>
        /// <param type="System.String, System" optional="true" name="sku">New invoice item stock keeping unit</param>
        /// <param type="System.Decimal, System" optional="true" name="stockQuantity">New invoice item stock quantity</param>
        /// <param type="System.Boolean, System" optional="true" name="trackInventory">Specifies if the inventory is tracked or not</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax1id">New first invoice item tax ID</param>
        /// <param type="System.Int32, System" optional="true" name="invoiceTax2id">New second invoice item tax ID</param>
        /// <short>Update an invoice item</short>
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceItemWrapper, ASC.Api.CRM">Updated invoice item</returns>
        /// <path>api/2.0/crm/invoiceitem/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"invoiceitem/{id:[0-9]+}")]
        public InvoiceItemWrapper UpdateInvoiceItem(int id,
                                                    string title,
                                                    string description,
                                                    decimal price,
                                                    string sku,
                                                    decimal stockQuantity,
                                                    bool trackInventory,
                                                    int invoiceTax1id,
                                                    int invoiceTax2id)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0 || String.IsNullOrEmpty(title) || price <= 0) throw new ArgumentException();

            if (!DaoFactory.InvoiceItemDao.IsExist(id)) throw new ItemNotFoundException();

            var invoiceItem = new InvoiceItem
            {
                ID = id,
                Title = title,
                Description = description,
                Price = price,
                StockKeepingUnit = sku,
                StockQuantity = stockQuantity,
                TrackInventory = trackInventory,
                InvoiceTax1ID = invoiceTax1id,
                InvoiceTax2ID = invoiceTax2id
            };

            invoiceItem = DaoFactory.InvoiceItemDao.SaveOrUpdateInvoiceItem(invoiceItem);
            MessageService.Send(Request, MessageAction.InvoiceItemUpdated, MessageTarget.Create(invoiceItem.ID), invoiceItem.Title);

            return ToInvoiceItemWrapper(invoiceItem);
        }

        /// <summary>
        /// Deletes an invoice item with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Invoice item ID</param>
        /// <short>Delete an invoice item</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceItemWrapper, ASC.Api.CRM">Invoice item</returns>
        /// <path>api/2.0/crm/invoiceitem/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"invoiceitem/{id:[0-9]+}")]
        public InvoiceItemWrapper DeleteInvoiceItem(int id)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0) throw new ArgumentException();

            var invoiceItem = DaoFactory.InvoiceItemDao.DeleteInvoiceItem(id);
            if (invoiceItem == null) throw new ItemNotFoundException();

            MessageService.Send(Request, MessageAction.InvoiceItemDeleted, MessageTarget.Create(invoiceItem.ID), invoiceItem.Title);
            return ToInvoiceItemWrapper(invoiceItem);
        }

        /// <summary>
        /// Deletes a group of invoice items with the IDs specified in the request.
        /// </summary>
        /// <param type="System.Collections.Generic.IEnumerable{System.Int32}, System.Collections.Generic" name="ids">List of invoice item IDs</param>
        /// <short>Delete invoice items</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceItemWrapper, ASC.Api.CRM">List of invoice items</returns>
        /// <path>api/2.0/crm/invoiceitem</path>
        /// <httpMethod>DELETE</httpMethod>
        /// <collection>list</collection>
        [Delete(@"invoiceitem")]
        public IEnumerable<InvoiceItemWrapper> DeleteBatchItems(IEnumerable<int> ids)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (ids == null) throw new ArgumentException();
            ids = ids.Distinct();

            var items = DaoFactory.InvoiceItemDao.DeleteBatchInvoiceItems(ids.ToArray());
            MessageService.Send(Request, MessageAction.InvoiceItemsDeleted, MessageTarget.Create(ids), items.Select(x => x.Title));

            return items.ConvertAll(ToInvoiceItemWrapper);
        }

        /// <summary>
        /// Returns a list of invoice taxes.
        /// </summary>
        /// <short>Get invoice taxes</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceTaxWrapper, ASC.Api.CRM">List of invoice taxes</returns>
        /// <path>api/2.0/crm/invoice/tax</path>
        /// <httpMethod>GET</httpMethod>
        /// <collection>list</collection>
        [Read(@"invoice/tax")]
        public IEnumerable<InvoiceTaxWrapper> GetInvoiceTaxes()
        {
            return DaoFactory.InvoiceTaxDao.GetAll().ConvertAll(ToInvoiceTaxWrapper);
        }

        /// <summary>
        ///  Creates an invoice tax with the parameters (name, description, rate) specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="name">Tax name</param>
        /// <param type="System.String, System" name="description">Tax description</param>
        /// <param type="System.Decimal, System" name="rate">Tax rate</param>
        /// <short>Create an invoice tax</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceTaxWrapper, ASC.Api.CRM">Invoice tax</returns>
        /// <path>api/2.0/crm/invoice/tax</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"invoice/tax")]
        public InvoiceTaxWrapper CreateInvoiceTax(
            string name,
            string description,
            decimal rate)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (String.IsNullOrEmpty(name)) throw new ArgumentException(Web.CRM.Resources.CRMInvoiceResource.EmptyTaxNameError);
            if (DaoFactory.InvoiceTaxDao.IsExist(name)) throw new ArgumentException(Web.CRM.Resources.CRMInvoiceResource.ExistTaxNameError);

            var invoiceTax = new InvoiceTax
            {
                Name = name,
                Description = description,
                Rate = rate
            };

            invoiceTax = DaoFactory.InvoiceTaxDao.SaveOrUpdateInvoiceTax(invoiceTax);
            MessageService.Send(Request, MessageAction.InvoiceTaxCreated, MessageTarget.Create(invoiceTax.ID), invoiceTax.Name);

            return ToInvoiceTaxWrapper(invoiceTax);
        }

        /// <summary>
        ///  Updates the selected invoice tax with the parameters (name, description, rate) specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Tax ID</param>
        /// <param type="System.String, System" name="name">New tax name</param>
        /// <param type="System.String, System" name="description">New tax description</param>
        /// <param type="System.Decimal, System" name="rate">New tax rate</param>
        /// <short>Update an invoice tax</short>
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceTaxWrapper, ASC.Api.CRM">Updated invoice tax</returns>
        /// <path>api/2.0/crm/invoice/tax/{id}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"invoice/tax/{id:[0-9]+}")]
        public InvoiceTaxWrapper UpdateInvoiceTax(
            int id,
            string name,
            string description,
            decimal rate)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0 || String.IsNullOrEmpty(name)) throw new ArgumentException(Web.CRM.Resources.CRMInvoiceResource.EmptyTaxNameError);

            if (!DaoFactory.InvoiceTaxDao.IsExist(id)) throw new ItemNotFoundException();

            var invoiceTax = new InvoiceTax
            {
                ID = id,
                Name = name,
                Description = description,
                Rate = rate
            };

            invoiceTax = DaoFactory.InvoiceTaxDao.SaveOrUpdateInvoiceTax(invoiceTax);
            MessageService.Send(Request, MessageAction.InvoiceTaxUpdated, MessageTarget.Create(invoiceTax.ID), invoiceTax.Name);

            return ToInvoiceTaxWrapper(invoiceTax);
        }

        /// <summary>
        ///  Deletes an invoice tax with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" method="url" name="id">Tax ID</param>
        /// <short>Delete an invoice tax</short> 
        /// <category>Invoices</category>
        /// <returns type="ASC.Api.CRM.Wrappers.InvoiceTaxWrapper, ASC.Api.CRM">Invoice tax</returns>
        /// <path>api/2.0/crm/invoice/tax/{id}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete(@"invoice/tax/{id:[0-9]+}")]
        public InvoiceTaxWrapper DeleteInvoiceTax(int id)
        {
            if (!CRMSecurity.IsAdmin)
            {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0) throw new ArgumentException();

            var invoiceTax = DaoFactory.InvoiceTaxDao.DeleteInvoiceTax(id);
            if (invoiceTax == null) throw new ItemNotFoundException();

            MessageService.Send(Request, MessageAction.InvoiceTaxDeleted, MessageTarget.Create(invoiceTax.ID), invoiceTax.Name);
            return ToInvoiceTaxWrapper(invoiceTax);
        }

        /// <summary>
        ///  Returns the default invoice settings.
        /// </summary>
        /// <short>Get the default invoice settings</short>
        /// <category>Invoices</category>
        /// <returns type="ASC.Web.CRM.Classes.InvoiceSetting, ASC.Web.CRM">Default invoice settings</returns>
        /// <path>api/2.0/crm/invoice/settings</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"invoice/settings")]
        public InvoiceSetting GetSettings()
        {
            return DaoFactory.InvoiceDao.GetSettings();
        }

        /// <summary>
        /// Saves the default settings for the invoice number specified in the request.
        /// </summary>
        /// <param type="System.Boolean, System" name="autogenerated">Defines if the default invoice number is autogenerated or not</param>
        /// <param type="System.String, System" name="prefix">Invoice prefix</param>
        /// <param type="System.String, System" name="number">Invoice number</param>
        /// <short>Save the invoice number default settings</short>
        /// <category>Invoices</category>
        /// <returns type="ASC.Web.CRM.Classes.InvoiceSetting, ASC.Web.CRM">Invoice settings</returns>
        /// <path>api/2.0/crm/invoice/settings/name</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"invoice/settings/name")]
        public InvoiceSetting SaveNumberSettings(bool autogenerated, string prefix, string number)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            if (autogenerated && string.IsNullOrEmpty(number))
                throw new ArgumentException();

            if (autogenerated && DaoFactory.InvoiceDao.IsExist(prefix + number))
                throw new ArgumentException();

            var invoiceSetting = GetSettings();

            invoiceSetting.Autogenerated = autogenerated;
            invoiceSetting.Prefix = prefix;
            invoiceSetting.Number = number;

            var settings = DaoFactory.InvoiceDao.SaveInvoiceSettings(invoiceSetting);
            MessageService.Send(Request, MessageAction.InvoiceNumberFormatUpdated);

            return settings;
        }

        /// <summary>
        ///  Saves the default settings for the invoice terms specified in the request.
        /// </summary>
        /// <param type="System.String, System" name="terms">Invoice terms</param>
        /// <short>Save the invoice terms default settings</short>
        /// <category>Invoices</category>
        /// <returns type="ASC.Web.CRM.Classes.InvoiceSetting, ASC.Web.CRM">Invoice settings</returns>
        /// <path>api/2.0/crm/invoice/settings/terms</path>
        /// <httpMethod>PUT</httpMethod>
        [Update(@"invoice/settings/terms")]
        public InvoiceSetting SaveTermsSettings(string terms)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var invoiceSetting = GetSettings();

            invoiceSetting.Terms = terms;

            var result = DaoFactory.InvoiceDao.SaveInvoiceSettings(invoiceSetting);
            MessageService.Send(Request, MessageAction.InvoiceDefaultTermsUpdated);

            return result;
        }

        /// <summary>
        ///  Sets the creation date to an invoice with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="invoiceid">Invoice ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="creationDate">Invoice creation date</param>
        /// <short>Set the invoice creation date</short>
        /// <category>Invoices</category>
        /// <path>api/2.0/crm/invoice/{invoiceid}/creationdate</path>
        /// <httpMethod>PUT</httpMethod>
        /// <visible>false</visible>
        [Update(@"invoice/{invoiceid:[0-9]+}/creationdate")]
        public void SetInvoiceCreationDate(int invoiceid, ApiDateTime creationDate)
        {
            var dao = DaoFactory.InvoiceDao;
            var invoice = dao.GetByID(invoiceid);

            if (invoice == null || !CRMSecurity.CanAccessTo(invoice))
                throw new ItemNotFoundException();

            dao.SetInvoiceCreationDate(invoiceid, creationDate);
        }

        /// <summary>
        ///  Sets the last modified date to an invoice with the ID specified in the request.
        /// </summary>
        /// <param type="System.Int32, System" name="invoiceid">Invoice ID</param>
        /// <param type="ASC.Specific.ApiDateTime, ASC.Specific" name="lastModifedDate">Invoice last modified date</param>
        /// <short>Set the invoice last modified date</short>
        /// <category>Invoices</category>
        /// <visible>false</visible>
        [Update(@"invoice/{invoiceid:[0-9]+}/lastmodifeddate")]
        public void SetInvoiceLastModifedDate(int invoiceid, ApiDateTime lastModifedDate)
        {
            var dao = DaoFactory.InvoiceDao;
            var invoice = dao.GetByID(invoiceid);

            if (invoice == null || !CRMSecurity.CanAccessTo(invoice))
                throw new ItemNotFoundException();

            dao.SetInvoiceLastModifedDate(invoiceid, lastModifedDate);
        }

        private InvoiceWrapper ToInvoiceWrapper(Invoice invoice)
        {
            var result = new InvoiceWrapper(invoice);

            if (invoice.ContactID > 0)
            {
                result.Contact = ToContactBaseWrapperQuick(DaoFactory.ContactDao.GetByID(invoice.ContactID));
            }

            if (invoice.ConsigneeID > 0)
            {
                result.Consignee = ToContactBaseWrapperQuick(DaoFactory.ContactDao.GetByID(invoice.ConsigneeID));
            }

            if (invoice.EntityID > 0)
            {
                result.Entity = ToEntityWrapper(invoice.EntityType, invoice.EntityID);
            }

            result.Cost = invoice.GetInvoiceCost(DaoFactory);

            result.InvoiceLines = invoice.GetInvoiceLines(DaoFactory).Select(ToInvoiceLineWrapper).ToList();

            return result;
        }

        private InvoiceItemWrapper ToInvoiceItemWrapper(InvoiceItem invoiceItem)
        {
            var result = new InvoiceItemWrapper(invoiceItem);

            if (invoiceItem.InvoiceTax1ID > 0)
            {
                result.InvoiceTax1 = ToInvoiceTaxWrapper(DaoFactory.InvoiceTaxDao.GetByID(invoiceItem.InvoiceTax1ID));
            }
            if (invoiceItem.InvoiceTax2ID > 0)
            {
                result.InvoiceTax2 = ToInvoiceTaxWrapper(DaoFactory.InvoiceTaxDao.GetByID(invoiceItem.InvoiceTax2ID));
            }

            return result;
        }

        private InvoiceTaxWrapper ToInvoiceTaxWrapper(InvoiceTax invoiceTax)
        {
            return new InvoiceTaxWrapper(invoiceTax);
        }

        private InvoiceLineWrapper ToInvoiceLineWrapper(InvoiceLine invoiceLine)
        {
            return new InvoiceLineWrapper(invoiceLine);
        }

        private IEnumerable<InvoiceBaseWrapper> ToListInvoiceBaseWrappers(ICollection<Invoice> invoices)
        {
            if (invoices == null || invoices.Count == 0) return new List<InvoiceWrapper>();

            var result = new List<InvoiceBaseWrapper>();

            var invoiceIDs = new HashSet<int>();
            var contactIDs = new HashSet<int>();
            var dealsIDs = new HashSet<int>();

            foreach (var invoice in invoices)
            {
                invoiceIDs.Add(invoice.ID);
                contactIDs.Add(invoice.ContactID);
                contactIDs.Add(invoice.ConsigneeID);
                dealsIDs.Add(invoice.EntityID);
            }

            var contacts = DaoFactory.ContactDao.GetContacts(contactIDs.ToArray())
                            .ToDictionary(contact => contact.ID, ToContactBaseWrapperQuick);

            var deals = DaoFactory.DealDao.GetDeals(dealsIDs.ToArray())
                            .ToDictionary(
                                deal => deal.ID,
                                deal => new EntityWrapper
                                {
                                    EntityId = deal.ID,
                                    EntityTitle = deal.Title,
                                    EntityType = "opportunity"
                                }
                            );

            var invoiceLines = DaoFactory.InvoiceLineDao.GetInvoicesLines(invoiceIDs.ToArray());

            var invoiceTaxIDs = new HashSet<int>();

            foreach (var invoiceLine in invoiceLines)
            {
                invoiceTaxIDs.Add(invoiceLine.InvoiceTax1ID);
                invoiceTaxIDs.Add(invoiceLine.InvoiceTax2ID);
            }

            var invoiceTaxes = DaoFactory.InvoiceTaxDao.GetByID(invoiceTaxIDs.ToArray());

            var invoiceCosts = invoiceLines
                                .GroupBy(invoiceLine => invoiceLine.InvoiceID)
                                .ToDictionary(
                                    item => item.Key,
                                    item => DaoFactory.InvoiceDao.CalculateInvoiceCost(item.Select(invoiceLine => invoiceLine), invoiceTaxes)
                                );

            foreach (var invoice in invoices)
            {
                var invoiceWrapper = new InvoiceBaseWrapper(invoice);

                if (contacts.ContainsKey(invoice.ContactID))
                {
                    invoiceWrapper.Contact = contacts[invoice.ContactID];
                }

                if (contacts.ContainsKey(invoice.ConsigneeID))
                {
                    invoiceWrapper.Consignee = contacts[invoice.ConsigneeID];
                }

                if (invoice.EntityID > 0 && deals.ContainsKey(invoice.EntityID))
                {
                    invoiceWrapper.Entity = deals[invoice.EntityID];
                }

                if (invoiceCosts.ContainsKey(invoice.ID))
                {
                    invoiceWrapper.Cost = invoiceCosts[invoice.ID];
                }

                result.Add(invoiceWrapper);
            }

            return result;
        }
    }
}