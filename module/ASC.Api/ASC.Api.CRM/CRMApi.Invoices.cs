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
using System.Linq;
using ASC.Api.Attributes;
using ASC.Api.CRM.Wrappers;
using ASC.Api.Exceptions;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Api.Collections;
using ASC.MessagingSystem;
using ASC.Specific;
using ASC.Web.CRM.Classes;
using ASC.Api.Documents;
using System.Security;

namespace ASC.Api.CRM
{
    public partial class CRMApi
    {
        [Read(@"invoice/{invoiceid:[0-9]+}")]
        public InvoiceWrapper GetInvoiceByID(int invoiceid)
        {
            if (invoiceid <= 0) throw new ArgumentException();

            var invoice = DaoFactory.GetInvoiceDao().GetByID(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice)) {
                throw CRMSecurity.CreateSecurityException();
            }

            return ToInvoiceWrapper(invoice);
        }

        [Read(@"invoice/sample")]
        public InvoiceWrapper GetInvoiceSample()
        {
            var sample = InvoiceWrapper.GetSample();
            sample.Number = DaoFactory.GetInvoiceDao().GetNewInvoicesNumber();
            sample.Terms = DaoFactory.GetInvoiceDao().GetSettings().Terms ?? string.Empty;

            return sample;
        }

        [Read(@"invoice/jsondata/{invoiceid:[0-9]+}")]
        public string GetInvoiceJsonData(int invoiceid)
        {
            var invoice = DaoFactory.GetInvoiceDao().GetByID(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice)) {
                throw CRMSecurity.CreateSecurityException();
            }

            return invoice.JsonData;
        }

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
                    DaoFactory.GetInvoiceDao().GetInvoices(
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
                    DaoFactory.GetInvoiceDao().GetInvoices(
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
                totalCount = DaoFactory.GetInvoiceDao().GetInvoicesCount(
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

        [Read("{entityType:(contact|person|company|opportunity)}/invoicelist/{entityid:[0-9]+}")]
        public IEnumerable<InvoiceBaseWrapper> GetEntityInvoices(String entityType, int entityid)
        {
            if (String.IsNullOrEmpty(entityType) || entityid <= 0) throw new ArgumentException();

            return ToListInvoiceBaseWrappers(DaoFactory.GetInvoiceDao().GetEntityInvoices(ToEntityType(entityType), entityid));
        }

        [Update(@"invoice/status/{status:[\w\d-]+}")]
        public KeyValuePair<IEnumerable<InvoiceBaseWrapper>,IEnumerable<InvoiceItemWrapper>>  UpdateInvoiceBatchStatus(
            int[] invoiceids,
            InvoiceStatus status
            )
        {
            if (invoiceids == null || !invoiceids.Any()) throw new ArgumentException();

            var oldInvoices = DaoFactory.GetInvoiceDao().GetByID(invoiceids).Where(CRMSecurity.CanAccessTo).ToList();

            var updatedInvoices = DaoFactory.GetInvoiceDao().UpdateInvoiceBatchStatus(oldInvoices.ToList().Select(i => i.ID).ToArray(), status);

            // detect what really changed
            var realUpdatedInvoices = updatedInvoices
                .Select(t => oldInvoices.FirstOrDefault(x => x.ID == t.ID && x.Status != t.Status))
                .Where(inv => inv != null)
                .ToList();

            if (realUpdatedInvoices.Any())
            {
                MessageService.Send(_context, MessageAction.InvoicesUpdatedStatus, realUpdatedInvoices.Select(x => x.Number), status.ToLocalizedString());
            }

            var invoiceItemsUpdated = new List<InvoiceItem>();

            if (status == InvoiceStatus.Sent || status == InvoiceStatus.Rejected)
            {
                var invoiceItemsAll = DaoFactory.GetInvoiceItemDao().GetAll();
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
                                var invoiceLines = DaoFactory.GetInvoiceLineDao().GetInvoiceLines(inv.ID);

                                foreach (var line in invoiceLines)
                                {
                                    var item = invoiceItemsWithTrackInventory.FirstOrDefault(ii => ii.ID == line.InvoiceItemID);
                                    if (item != null)
                                    {
                                        item.StockQuantity -= line.Quantity;
                                        DaoFactory.GetInvoiceItemDao().SaveOrUpdateInvoiceItem(item);
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
                                var invoiceLines = DaoFactory.GetInvoiceLineDao().GetInvoiceLines(inv.ID);

                                foreach (var line in invoiceLines)
                                {
                                    var item = invoiceItemsWithTrackInventory.FirstOrDefault(ii => ii.ID == line.InvoiceItemID);
                                    if (item != null)
                                    {
                                        item.StockQuantity += line.Quantity;
                                        DaoFactory.GetInvoiceItemDao().SaveOrUpdateInvoiceItem(item);
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

            return new KeyValuePair<IEnumerable<InvoiceBaseWrapper>,IEnumerable<InvoiceItemWrapper>>(listInvoiceBaseWrappers,invoiceItemsUpdated.ConvertAll(i => ToInvoiceItemWrapper(i)));
        }

        [Delete(@"invoice/{invoiceid:[0-9]+}")]
        public InvoiceBaseWrapper DeleteInvoice(int invoiceid)
        {
            if (invoiceid <= 0) throw new ArgumentException();

            var invoice = DaoFactory.GetInvoiceDao().DeleteInvoice(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            MessageService.Send(_context, MessageAction.InvoiceDeleted, invoice.Number);
            return ToInvoiceBaseWrapper(invoice);
        }

        [Delete(@"invoice")]
        public IEnumerable<InvoiceBaseWrapper> DeleteBatchInvoices(IEnumerable<int> invoiceids)
        {
            if (invoiceids == null || !invoiceids.Any()) throw new ArgumentException();

            var invoices = DaoFactory.GetInvoiceDao().DeleteBatchInvoices(invoiceids.ToArray());
            MessageService.Send(_context, MessageAction.InvoicesDeleted, invoices.Select(x => x.Number));

            return ToListInvoiceBaseWrappers(invoices);
        }

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
            IEnumerable<InvoiceLineWrapper> invoiceLines
            )
        {
            var invoiceLinesList = invoiceLines.ToList();
            if (invoiceLines == null || !invoiceLinesList.Any() || !isLinesForInvoiceCorrect(invoiceLinesList)) throw new ArgumentException();

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
                    Currency = !String.IsNullOrEmpty(currency) ? currency.ToUpper(): null,
                    ExchangeRate = exchangeRate,
                    PurchaseOrderNumber = purchaseOrderNumber,
                    Terms = terms,
                    Description = description
                };

            CRMSecurity.DemandCreateOrUpdate(invoice);

            if (billingAddressID > 0)
            {
                var address = DaoFactory.GetContactInfoDao().GetByID(billingAddressID);
                if (address == null || address.InfoType != ContactInfoType.Address || address.Category != (int)AddressCategory.Billing || address.ContactID != contactId)
                    throw new ArgumentException();
            }

            if (deliveryAddressID > 0)
            {
                var address = DaoFactory.GetContactInfoDao().GetByID(deliveryAddressID);
                if (address == null || address.InfoType != ContactInfoType.Address || address.Category != (int)AddressCategory.Postal || address.ContactID != consigneeId)
                    throw new ArgumentException();
            }


            invoice.ID = DaoFactory.GetInvoiceDao().SaveOrUpdateInvoice(invoice);

            CreateInvoiceLines(invoiceLinesList, invoice);

                //RemoveUnusedLines(invoice.ID, invoiceLines);

            DaoFactory.GetInvoiceDao().UpdateInvoiceJsonData(invoice, billingAddressID, deliveryAddressID);


            return ToInvoiceWrapper(invoice);
        }


        private bool isLinesForInvoiceCorrect(List<InvoiceLineWrapper> invoiceLines)
        {
            foreach (var line in invoiceLines)
            {
                if (line.InvoiceItemID <= 0 ||
                    line.Quantity < 0 || line.Price < 0 ||
                    line.Discount < 0 || line.Discount > 100 ||
                    line.InvoiceTax1ID < 0 || line.InvoiceTax2ID < 0)
                    return false;
                if (!DaoFactory.GetInvoiceItemDao().IsExist(line.InvoiceItemID))
                    return false;

                if (line.InvoiceTax1ID > 0 && !DaoFactory.GetInvoiceTaxDao().IsExist(line.InvoiceTax1ID))
                    return false;

                if (line.InvoiceTax2ID > 0 && !DaoFactory.GetInvoiceTaxDao().IsExist(line.InvoiceTax2ID))
                    return false;
            }
            return true;
        }

        private List<InvoiceLine> CreateInvoiceLines(List<InvoiceLineWrapper> invoiceLines, Invoice invoice)
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
                    Discount = Convert.ToInt32(invoiceLines[i].Discount)
                };

                line.ID = DaoFactory.GetInvoiceLineDao().SaveOrUpdateInvoiceLine(line);
                result.Add(line);
            }
            return result;
        }


        [Update("invoice/{invoiceid:[0-9]+}")]
        public InvoiceWrapper UpdateInvoice(
            int id,
            string number,
            ApiDateTime issueDate,
            int templateType,
            int contactId,
            int consigneeId,
            int entityId,
            ApiDateTime dueDate,
            string language,
            string currency,
            decimal exchangeRate,
            string purchaseOrderNumber,
            string terms,
            string description,
            IEnumerable<InvoiceLineWrapper> invoiceLines)
        {
            var invoice = DaoFactory.GetInvoiceDao().GetByID(id);
            if (invoice == null) throw new ItemNotFoundException();

            invoice.Number = number;
            invoice.IssueDate = issueDate;
            invoice.TemplateType = (InvoiceTemplateType)templateType;
            invoice.ContactID = contactId;
            invoice.ConsigneeID = consigneeId;
            invoice.EntityType = EntityType.Opportunity;
            invoice.EntityID = entityId;
            invoice.DueDate = dueDate;
            invoice.Language = language;
            invoice.Currency = !String.IsNullOrEmpty(currency) ? currency.ToUpper(): null;;
            invoice.ExchangeRate = exchangeRate;
            invoice.PurchaseOrderNumber = purchaseOrderNumber;
            invoice.Terms = terms;
            invoice.Description = description;

            CRMSecurity.DemandCreateOrUpdate(invoice);

            DaoFactory.GetInvoiceDao().SaveOrUpdateInvoice(invoice);

            return ToInvoiceWrapper(invoice);
        }

        [Read(@"invoice/{invoiceid:[0-9]+}/pdf")]
        public FileWrapper GetInvoicePdfExistOrCreate(int invoiceid)
        {
            if (invoiceid <= 0) throw new ArgumentException();

            var invoice = DaoFactory.GetInvoiceDao().GetByID(invoiceid);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice)) {
                throw CRMSecurity.CreateSecurityException();
            }

            return new FileWrapper(Global.GetInvoicePdfExistingOrCreate(invoice));
        }

        [Create(@"invoice/converter/data")]
        public ConverterData GetInvoiceConverterData(int invoiceId, string converterUrl, string storageUrl, string revisionId)
        {
            if (invoiceId <= 0) throw new ArgumentException();

            var invoice = DaoFactory.GetInvoiceDao().GetByID(invoiceId);
            if (invoice == null) throw new ItemNotFoundException();

            if (!CRMSecurity.CanAccessTo(invoice)) {
                throw CRMSecurity.CreateSecurityException();
            }

            var converterData = new ConverterData
            {
                ConverterUrl = converterUrl,
                StorageUrl = storageUrl,
                RevisionId = revisionId,
                InvoiceId = invoiceId
            };

            var existingFile = invoice.GetInvoiceFile();
            if (existingFile != null)
            {
                converterData.FileId = invoice.FileID;
                return converterData;
            }

            if (string.IsNullOrEmpty(converterUrl) || string.IsNullOrEmpty(storageUrl) || string.IsNullOrEmpty(revisionId))
            {
                return PdfCreator.StartCreationFileAsync(invoice);
            }
            else
            {
                var convertedFile = PdfCreator.GetConvertedFile(converterData);
                if (convertedFile != null)
                {
                    invoice.FileID = Int32.Parse(convertedFile.ID.ToString());
                    Global.DaoFactory.GetInvoiceDao().UpdateInvoiceFileID(invoice.ID, invoice.FileID);
                    Global.DaoFactory.GetRelationshipEventDao().AttachFiles(invoice.ContactID, invoice.EntityType, invoice.EntityID, new[] { invoice.FileID });

                    converterData.FileId = invoice.FileID;
                    return converterData;
                }
                else
                {
                    return converterData;
                }
            }
        }


        [Read(@"invoice/bynumber/exist")]
        public Boolean GetInvoiceByNumberExistence(string number)
        {
            if (String.IsNullOrEmpty(number)) throw new ArgumentException();
            return DaoFactory.GetInvoiceDao().IsExist(number);
        }

        [Read(@"invoice/bynumber")]
        public InvoiceWrapper GetInvoiceByNumber(string number)
        {
            if (String.IsNullOrEmpty(number)) throw new ArgumentException();

            var invoice = DaoFactory.GetInvoiceDao().GetByNumber(number);
            if (invoice == null) throw new ItemNotFoundException();
            if (!CRMSecurity.CanAccessTo(invoice)) {
                throw CRMSecurity.CreateSecurityException();
            }

            return new InvoiceWrapper(invoice);
        }


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
                result = DaoFactory.GetInvoiceItemDao().GetInvoiceItems(
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
                result = DaoFactory.GetInvoiceItemDao().GetInvoiceItems(
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
                totalCount = DaoFactory.GetInvoiceItemDao().GetInvoiceItemsCount(
                    searchString,
                    status,
                    inventoryStock);
            }

            _context.SetTotalCount(totalCount);

            return result.ToSmartList();
        }

        [Create(@"invoiceline")]
        public InvoiceLineWrapper CreateInvoiceLine(
            int invoiceId,
            int invoiceItemId,
            int invoiceTax1Id,
            int invoiceTax2Id,
            int sortOrder,
            string description,
            int quantity,
            decimal price,
            int discount
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

            CRMSecurity.DemandCreateOrUpdate(invoiceLine);

            invoiceLine.ID = DaoFactory.GetInvoiceLineDao().SaveOrUpdateInvoiceLine(invoiceLine);

            return ToInvoiceLineWrapper(invoiceLine);
        }

        [Update(@"invoiceline/{invoicelineid:[0-9]+}")]
        public InvoiceLineWrapper UpdateInvoiceLine(
            int id,
            int invoiceId,
            int invoiceItemId,
            int invoiceTax1Id,
            int invoiceTax2Id,
            int sortOrder,
            string description,
            int quantity,
            decimal price,
            int discount
            )
        {
            var invoiceLine = DaoFactory.GetInvoiceLineDao().GetByID(id);
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

            CRMSecurity.DemandCreateOrUpdate(invoiceLine);

            DaoFactory.GetInvoiceLineDao().SaveOrUpdateInvoiceLine(invoiceLine);

            return ToInvoiceLineWrapper(invoiceLine);
        }

        [Delete(@"invoiceline/{invoicelineid:[0-9]+}")]
        public int DeleteInvoiceLine(int id)
        {
            var invoiceLine = DaoFactory.GetInvoiceLineDao().GetByID(id);
            if (invoiceLine == null) throw new ItemNotFoundException();
            if (!DaoFactory.GetInvoiceLineDao().CanDelete(invoiceLine.ID)) throw new ArgumentException();

            var invoice = DaoFactory.GetInvoiceDao().GetByID(invoiceLine.InvoiceID);
            if (invoice == null) throw new ItemNotFoundException();
            if (!CRMSecurity.CanEdit(invoice)) throw CRMSecurity.CreateSecurityException();

            DaoFactory.GetInvoiceLineDao().DeleteInvoiceLine(id);
            return id;
        }

        private InvoiceBaseWrapper ToInvoiceBaseWrapper(Invoice invoice)
        {
            var result = new InvoiceBaseWrapper(invoice);

            if (invoice.ContactID > 0)
            {
                result.Contact = ToContactBaseWithEmailWrapper(DaoFactory.GetContactDao().GetByID(invoice.ContactID));
            }

            if (invoice.ConsigneeID > 0)
            {
                result.Consignee = ToContactBaseWithEmailWrapper(DaoFactory.GetContactDao().GetByID(invoice.ConsigneeID));
            }

            if (invoice.EntityID > 0)
            {
                result.Entity = ToEntityWrapper(invoice.EntityType, invoice.EntityID);
            }

            result.Cost = invoice.GetInvoiceCost();

            return result;
        }


        [Create(@"invoiceitem")]
        public InvoiceItemWrapper CreateInvoiceItem(
            string title,
            string description,
            decimal price,
            string sku,
            int quantity,
            int stockQuantity,
            bool trackInventory,
            int invoiceTax1id,
            int invoiceTax2id)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (String.IsNullOrEmpty(title) || price == 0) throw new ArgumentException();

            var invoiceItem = new InvoiceItem
                {
                    Title = title,
                    Description = description,
                    Price = price,
                    StockKeepingUnit = sku,
                    Quantity = quantity,
                    StockQuantity = stockQuantity,
                    TrackInventory = trackInventory,
                    InvoiceTax1ID = invoiceTax1id,
                    InvoiceTax2ID = invoiceTax2id
                };

            invoiceItem = DaoFactory.GetInvoiceItemDao().SaveOrUpdateInvoiceItem(invoiceItem);
            MessageService.Send(_context, MessageAction.InvoiceItemCreated, invoiceItem.Title);

            return ToInvoiceItemWrapper(invoiceItem);
        }


        [Update(@"invoiceitem/{id:[0-9]+}")]
        public InvoiceItemWrapper UpdateInvoiceItem(int id,
                                                    string title,
                                                    string description,
                                                    decimal price,
                                                    string sku,
                                                    int quantity,
                                                    int stockQuantity,
                                                    bool trackInventory,
                                                    int invoiceTax1id,
                                                    int invoiceTax2id)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0 || String.IsNullOrEmpty(title) || price == 0) throw new ArgumentException();

            if (!DaoFactory.GetInvoiceItemDao().IsExist(id)) throw new ItemNotFoundException();

            var invoiceItem = new InvoiceItem
                {
                    ID = id,
                    Title = title,
                    Description = description,
                    Price = price,
                    StockKeepingUnit = sku,
                    Quantity = quantity,
                    StockQuantity = stockQuantity,
                    TrackInventory = trackInventory,
                    InvoiceTax1ID = invoiceTax1id,
                    InvoiceTax2ID = invoiceTax2id
                };

            invoiceItem = DaoFactory.GetInvoiceItemDao().SaveOrUpdateInvoiceItem(invoiceItem);
            MessageService.Send(_context, MessageAction.InvoiceItemUpdated, invoiceItem.Title);

            return ToInvoiceItemWrapper(invoiceItem);
        }

        [Delete(@"invoiceitem/{id:[0-9]+}")]
        public InvoiceItemWrapper DeleteInvoiceItem(int id)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0) throw new ArgumentException();

            var invoiceItem = DaoFactory.GetInvoiceItemDao().DeleteInvoiceItem(id);
            if (invoiceItem == null) throw new ItemNotFoundException();

            MessageService.Send(_context, MessageAction.InvoiceItemDeleted, invoiceItem.Title);
            return ToInvoiceItemWrapper(invoiceItem);
        }

        [Delete(@"invoiceitem")]
        public IEnumerable<InvoiceItemWrapper> DeleteBatchItems(IEnumerable<int> ids)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (ids == null) throw new ArgumentException();
            ids = ids.Distinct();

            var items = DaoFactory.GetInvoiceItemDao().DeleteBatchInvoiceItems(ids.ToArray());
            MessageService.Send(_context, MessageAction.InvoiceItemsDeleted, items.Select(x => x.Title));

            return items.ConvertAll(ToInvoiceItemWrapper);
        }


        [Read(@"invoice/tax")]
        public IEnumerable<InvoiceTaxWrapper> GetInvoiceTaxes()
        {
            return DaoFactory.GetInvoiceTaxDao().GetAll().ConvertAll(ToInvoiceTaxWrapper);
        }


        [Create(@"invoice/tax")]
        public InvoiceTaxWrapper CreateInvoiceTax(
            string name,
            string description,
            int rate)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (String.IsNullOrEmpty(name)) throw new ArgumentException();
            if(DaoFactory.GetInvoiceTaxDao().IsExist(name)) throw new ArgumentException();

            var invoiceTax = new InvoiceTax
                {
                    Name = name,
                    Description = description,
                    Rate = rate
                };

            invoiceTax = DaoFactory.GetInvoiceTaxDao().SaveOrUpdateInvoiceTax(invoiceTax);
            MessageService.Send(_context, MessageAction.InvoiceTaxCreated, invoiceTax.Name);

            return ToInvoiceTaxWrapper(invoiceTax);
        }

        [Update(@"invoice/tax/{id:[0-9]+}")]
        public InvoiceTaxWrapper UpdateInvoiceTax(
            int id,
            string name,
            string description,
            int rate)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0 || String.IsNullOrEmpty(name)) throw new ArgumentException();

            if (!DaoFactory.GetInvoiceTaxDao().IsExist(id)) throw new ItemNotFoundException();

            var invoiceTax = new InvoiceTax
                {
                    ID = id,
                    Name = name,
                    Description = description,
                    Rate = rate
                };

            invoiceTax = DaoFactory.GetInvoiceTaxDao().SaveOrUpdateInvoiceTax(invoiceTax);
            MessageService.Send(_context, MessageAction.InvoiceTaxUpdated, invoiceTax.Name);

            return ToInvoiceTaxWrapper(invoiceTax);
        }


        [Delete(@"invoice/tax/{id:[0-9]+}")]
        public InvoiceTaxWrapper DeleteInvoiceTax(int id)
        {
            if (!CRMSecurity.IsAdmin) {
                throw CRMSecurity.CreateSecurityException();
            }

            if (id <= 0) throw new ArgumentException();

            var invoiceTax = DaoFactory.GetInvoiceTaxDao().DeleteInvoiceTax(id);
            if (invoiceTax == null) throw new ItemNotFoundException();

            MessageService.Send(_context, MessageAction.InvoiceTaxDeleted, invoiceTax.Name);
            return ToInvoiceTaxWrapper(invoiceTax);
        }


        [Read(@"invoice/settings")]
        public InvoiceSetting GetSettings()
        {
            return DaoFactory.GetInvoiceDao().GetSettings();
        }

        [Update(@"invoice/settings/name")]
        public InvoiceSetting SaveNumberSettings(bool autogenerated, string prefix, string number)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            if (autogenerated && string.IsNullOrEmpty(number))
                throw new ArgumentException();

            if (autogenerated && DaoFactory.GetInvoiceDao().IsExist(prefix + number))
                throw new ArgumentException();

            var invoiceSetting = GetSettings();

            invoiceSetting.Autogenerated = autogenerated;
            invoiceSetting.Prefix = prefix;
            invoiceSetting.Number = number;

            var settings = DaoFactory.GetInvoiceDao().SaveInvoiceSettings(invoiceSetting);
            MessageService.Send(_context, MessageAction.InvoiceNumberFormatUpdated);

            return settings;
        }

        [Update(@"invoice/settings/terms")]
        public InvoiceSetting SaveTermsSettings(string terms)
        {
            if (!CRMSecurity.IsAdmin) throw CRMSecurity.CreateSecurityException();

            var invoiceSetting = GetSettings();

            invoiceSetting.Terms = terms;

            return DaoFactory.GetInvoiceDao().SaveInvoiceSettings(invoiceSetting);
        }


        private InvoiceWrapper ToInvoiceWrapper(Invoice invoice)
        {
            var result = new InvoiceWrapper(invoice);

            if (invoice.ContactID > 0)
            {
                result.Contact = ToContactBaseWithEmailWrapper(DaoFactory.GetContactDao().GetByID(invoice.ContactID));
            }

            if (invoice.ConsigneeID > 0)
            {
                result.Consignee = ToContactBaseWithEmailWrapper(DaoFactory.GetContactDao().GetByID(invoice.ConsigneeID));
            }

            if (invoice.EntityID > 0)
            {
                result.Entity = ToEntityWrapper(invoice.EntityType, invoice.EntityID);
            }

            result.Cost = invoice.GetInvoiceCost();

            result.InvoiceLines = invoice.GetInvoiceLines().Select(ToInvoiceLineWrapper).ToList();

            return result;
        }

        private InvoiceItemWrapper ToInvoiceItemWrapper(InvoiceItem invoiceItem)
        {
            var result = new InvoiceItemWrapper(invoiceItem);

            if (invoiceItem.InvoiceTax1ID > 0)
            {
                result.InvoiceTax1 = ToInvoiceTaxWrapper(DaoFactory.GetInvoiceTaxDao().GetByID(invoiceItem.InvoiceTax1ID));
            }
            if (invoiceItem.InvoiceTax2ID > 0)
            {
                result.InvoiceTax2 = ToInvoiceTaxWrapper(DaoFactory.GetInvoiceTaxDao().GetByID(invoiceItem.InvoiceTax2ID));
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

        private IEnumerable<InvoiceBaseWrapper> ToListInvoiceBaseWrappers(ICollection<Invoice> items)
        {
            if (items == null || items.Count == 0) return new List<InvoiceWrapper>();

            var result = new List<InvoiceBaseWrapper>();


            var contactIDs = items.Select(item => item.ContactID);
            contactIDs.ToList().AddRange(items.Select(item => item.ConsigneeID));

            var contacts = DaoFactory.GetContactDao().GetContacts(contactIDs.Distinct().ToArray())
                                     .ToDictionary(item => item.ID, ToContactBaseWithEmailWrapper);


            foreach (var invoice in items)
            {
                var invoiceWrapper = new InvoiceBaseWrapper(invoice);

                if (contacts.ContainsKey(invoice.ContactID))
                {
                    invoiceWrapper.Contact = contacts[invoice.ContactID];
                }

                if (contacts.ContainsKey(invoice.ConsigneeID))
                {
                    invoiceWrapper.Consignee = contacts[invoice.ContactID];
                }

                if (invoice.EntityID > 0)
                {
                    invoiceWrapper.Entity = ToEntityWrapper(invoice.EntityType, invoice.EntityID); //Need to optimize
                }

                invoiceWrapper.Cost = invoice.GetInvoiceCost();

                result.Add(invoiceWrapper);
            }

            return result;
        }
    }
}