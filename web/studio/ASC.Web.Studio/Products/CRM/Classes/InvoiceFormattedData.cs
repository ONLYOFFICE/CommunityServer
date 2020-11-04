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


using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Resources;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Core;
using Autofac;

namespace ASC.Web.CRM.Classes
{

    public class InvoiceFormattedData
    {
        public int TemplateType { get; set; }
        public Tuple<string, string> Seller { get; set; }
        public int LogoBase64Id { get; set; }
        public string LogoBase64 { get; set; }
        public string LogoSrcFormat { get; set; }
        public Tuple<string, string> Number { get; set; }
        public List<Tuple<string, string>> Invoice { get; set; }
        public Tuple<string, string> Customer { get; set; }
        public List<string> TableHeaderRow { get; set; }
        public List<List<string>> TableBodyRows { get; set; }
        public List<Tuple<string, string>> TableFooterRows { get; set; }
        public Tuple<string, string> TableTotalRow { get; set; }
        public Tuple<string, string> Terms { get; set; }
        public Tuple<string, string> Notes { get; set; }
        public Tuple<string, string> Consignee { get; set; }

        public int DeliveryAddressID { get; set; }
        public int BillingAddressID { get; set; }

        public static InvoiceFormattedData GetData(Invoice invoice, int billingAddressID, int deliveryAddressID)
        {
            return invoice.JsonData != null ? ReadData(invoice.JsonData) : CreateData(invoice, billingAddressID, deliveryAddressID);
        }

        public static InvoiceFormattedData GetDataAfterLinesUpdated(Invoice invoice)
        {
            if (invoice.JsonData != null)
            {
                var oldData = ReadData(invoice.JsonData);
                return CreateDataAfterLinesUpdated(invoice, oldData);
            }
            else
            {
                return CreateData(invoice, 0, 0);
            }
        }

        private static InvoiceFormattedData CreateData(Invoice invoice, int billingAddressID, int deliveryAddressID)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();

                var data = new InvoiceFormattedData();
                var sb = new StringBuilder();
                var list = new List<string>();
                var cultureInfo = string.IsNullOrEmpty(invoice.Language)
                    ? CultureInfo.CurrentCulture
                    : CultureInfo.GetCultureInfo(invoice.Language);


                #region TemplateType

                data.TemplateType = (int) invoice.TemplateType;

                #endregion


                #region Seller, LogoBase64, LogoSrcFormat

                var invoiceSettings = daoFactory.InvoiceDao.GetSettings();

                if (!string.IsNullOrEmpty(invoiceSettings.CompanyName))
                {
                    sb.Append(invoiceSettings.CompanyName);
                }

                if (!string.IsNullOrEmpty(invoiceSettings.CompanyAddress))
                {
                    var obj = JObject.Parse(invoiceSettings.CompanyAddress);

                    var str = obj.Value<string>("street");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("city");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("state");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("zip");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    str = obj.Value<string>("country");
                    if (!string.IsNullOrEmpty(str))
                        list.Add(str);

                    if (list.Count > 0)
                    {
                        sb.AppendLine();
                        sb.Append(string.Join(", ", list));
                    }
                }

                data.Seller =
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Seller", cultureInfo),
                        sb.ToString());

                if (invoiceSettings.CompanyLogoID != 0)
                {
                    data.LogoBase64Id = invoiceSettings.CompanyLogoID;
                    //data.LogoBase64 = OrganisationLogoManager.GetOrganisationLogoBase64(invoiceSettings.CompanyLogoID);
                    data.LogoSrcFormat = OrganisationLogoManager.OrganisationLogoSrcFormat;
                }

                #endregion


                #region Number

                data.Number =
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Invoice", cultureInfo),
                        invoice.Number);

                #endregion


                #region Invoice

                data.Invoice = new List<Tuple<string, string>>();
                data.Invoice.Add(
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("IssueDate", cultureInfo),
                        invoice.IssueDate.ToShortDateString()));
                if (!string.IsNullOrEmpty(invoice.PurchaseOrderNumber))
                {
                    data.Invoice.Add(
                        new Tuple<string, string>(
                            CRMInvoiceResource.ResourceManager.GetString("PONumber", cultureInfo),
                            invoice.PurchaseOrderNumber));
                }
                data.Invoice.Add(
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("DueDate", cultureInfo),
                        invoice.DueDate.ToShortDateString()));

                #endregion


                #region Customer

                var customer = daoFactory.ContactDao.GetByID(invoice.ContactID);

                if (customer != null)
                {
                    sb = new StringBuilder();

                    sb.Append(customer.GetTitle());

                    var billingAddress = billingAddressID != 0
                        ? daoFactory.ContactInfoDao.GetByID(billingAddressID)
                        : null;
                    if (billingAddress != null && billingAddress.InfoType == ContactInfoType.Address &&
                        billingAddress.Category == (int) AddressCategory.Billing)
                    {
                        list = new List<string>();

                        var obj = JObject.Parse(billingAddress.Data);

                        var str = obj.Value<string>("street");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("city");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("state");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("zip");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("country");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        if (list.Count > 0)
                        {
                            sb.AppendLine();
                            sb.Append(string.Join(", ", list));
                        }
                    }

                    data.Customer =
                        new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("BillTo", cultureInfo),
                            sb.ToString());
                }

                #endregion


                #region TableHeaderRow, TableBodyRows, TableFooterRows, TableTotalRow

                data.TableHeaderRow = new List<string>
                {
                    CRMInvoiceResource.ResourceManager.GetString("ItemCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("QuantityCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("PriceCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("DiscountCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("TaxCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("TaxCol", cultureInfo),
                    CRMInvoiceResource.ResourceManager.GetString("AmountCol", cultureInfo)
                };

                data.TableBodyRows = new List<List<string>>();

                var invoiceLines = invoice.GetInvoiceLines(daoFactory);
                var invoiceTaxes = new Dictionary<int, decimal>();

                decimal subtotal = 0;
                decimal discount = 0;
                decimal amount = 0;

                foreach (var line in invoiceLines)
                {
                    var item = daoFactory.InvoiceItemDao.GetByID(line.InvoiceItemID);
                    var tax1 = line.InvoiceTax1ID > 0
                        ? daoFactory.InvoiceTaxDao.GetByID(line.InvoiceTax1ID)
                        : null;
                    var tax2 = line.InvoiceTax2ID > 0
                        ? daoFactory.InvoiceTaxDao.GetByID(line.InvoiceTax2ID)
                        : null;

                    var subtotalValue = Math.Round(line.Quantity*line.Price, 2);
                    var discountValue = Math.Round(subtotalValue*line.Discount/100, 2);

                    decimal rate = 0;
                    if (tax1 != null)
                    {
                        rate += tax1.Rate;
                        if (invoiceTaxes.ContainsKey(tax1.ID))
                        {
                            invoiceTaxes[tax1.ID] = invoiceTaxes[tax1.ID] +
                                                    Math.Round((subtotalValue - discountValue)*tax1.Rate/100, 2);
                        }
                        else
                        {
                            invoiceTaxes.Add(tax1.ID, Math.Round((subtotalValue - discountValue)*tax1.Rate/100, 2));
                        }
                    }
                    if (tax2 != null)
                    {
                        rate += tax2.Rate;
                        if (invoiceTaxes.ContainsKey(tax2.ID))
                        {
                            invoiceTaxes[tax2.ID] = invoiceTaxes[tax2.ID] +
                                                    Math.Round((subtotalValue - discountValue)*tax2.Rate/100, 2);
                        }
                        else
                        {
                            invoiceTaxes.Add(tax2.ID, Math.Round((subtotalValue - discountValue)*tax2.Rate/100, 2));
                        }
                    }

                    decimal taxValue = Math.Round((subtotalValue - discountValue)*rate/100, 2);
                    decimal amountValue = Math.Round(subtotalValue - discountValue + taxValue, 2);

                    subtotal += subtotalValue;
                    discount += discountValue;
                    amount += amountValue;

                    data.TableBodyRows.Add(new List<string>
                    {
                        item.Title + (string.IsNullOrEmpty(line.Description) ? string.Empty : ": " + line.Description),
                        line.Quantity.ToString(CultureInfo.InvariantCulture),
                        line.Price.ToString(CultureInfo.InvariantCulture),
                        line.Discount.ToString(CultureInfo.InvariantCulture),
                        tax1 != null ? tax1.Name : string.Empty,
                        tax2 != null ? tax2.Name : string.Empty,
                        (subtotalValue - discountValue).ToString(CultureInfo.InvariantCulture)
                    });
                }

                data.TableFooterRows = new List<Tuple<string, string>>();
                data.TableFooterRows.Add(
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Subtotal", cultureInfo),
                        (subtotal - discount).ToString(CultureInfo.InvariantCulture)));

                foreach (var invoiceTax in invoiceTaxes)
                {
                    var iTax = daoFactory.InvoiceTaxDao.GetByID(invoiceTax.Key);
                    data.TableFooterRows.Add(new Tuple<string, string>(
                        string.Format("{0} ({1}%)", iTax.Name, iTax.Rate),
                        invoiceTax.Value.ToString(CultureInfo.InvariantCulture)));
                }

                //data.TableFooterRows.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Discount", cultureInfo), "-" + discount.ToString(CultureInfo.InvariantCulture)));

                data.TableTotalRow =
                    new Tuple<string, string>(
                        string.Format("{0} ({1})", CRMInvoiceResource.ResourceManager.GetString("Total", cultureInfo),
                            invoice.Currency), amount.ToString(CultureInfo.InvariantCulture));


                #endregion


                #region Terms

                data.Terms =
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Terms", cultureInfo),
                        invoice.Terms);

                #endregion


                #region Notes

                if (!string.IsNullOrEmpty(invoice.Description))
                {
                    data.Notes =
                        new Tuple<string, string>(
                            CRMInvoiceResource.ResourceManager.GetString("ClientNotes", cultureInfo),
                            invoice.Description);
                }

                #endregion


                #region Consignee

                var consignee = daoFactory.ContactDao.GetByID(invoice.ConsigneeID);

                if (consignee != null)
                {
                    sb = new StringBuilder();

                    sb.Append(consignee.GetTitle());

                    var deliveryAddress = deliveryAddressID != 0
                        ? daoFactory.ContactInfoDao.GetByID(deliveryAddressID)
                        : null;
                    if (deliveryAddress != null && deliveryAddress.InfoType == ContactInfoType.Address &&
                        deliveryAddress.Category == (int) AddressCategory.Postal)
                    {
                        list = new List<string>();

                        var obj = JObject.Parse(deliveryAddress.Data);

                        var str = obj.Value<string>("street");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("city");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("state");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("zip");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        str = obj.Value<string>("country");
                        if (!string.IsNullOrEmpty(str))
                            list.Add(str);

                        if (list.Count > 0)
                        {
                            sb.AppendLine();
                            sb.Append(string.Join(", ", list));
                        }
                    }

                    data.Consignee =
                        new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("ShipTo", cultureInfo),
                            sb.ToString());
                }

                #endregion

                #region Addresses

                data.BillingAddressID = billingAddressID;
                data.DeliveryAddressID = deliveryAddressID;

                #endregion

                return data;
            }
        }

        private static InvoiceFormattedData ReadData(string jsonData)
        {
            var data = new InvoiceFormattedData();
            var jsonObj = JObject.Parse(jsonData);


            #region TemplateType

            data.TemplateType = jsonObj.Value<int>("TemplateType");

            #endregion


            #region Seller, LogoBase64, LogoSrcFormat

            var seller = jsonObj.Value<JObject>("Seller");
            if (seller != null)
            {
                data.Seller = seller.ToObject<Tuple<string, string>>();
            }

            data.LogoBase64 = jsonObj.Value<string>("LogoBase64");
            data.LogoBase64Id = !String.IsNullOrEmpty(jsonObj.Value<string>("LogoBase64Id")) ? jsonObj.Value<int>("LogoBase64Id") : 0;

            if (string.IsNullOrEmpty(data.LogoBase64) && data.LogoBase64Id != 0)
            {
                 data.LogoBase64 = OrganisationLogoManager.GetOrganisationLogoBase64(data.LogoBase64Id);
            }


            data.LogoSrcFormat = jsonObj.Value<string>("LogoSrcFormat");

            #endregion


            #region Number

            var number = jsonObj.Value<JObject>("Number");
            if (number != null)
            {
                data.Number = number.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Invoice

            var invoice = jsonObj.Value<JArray>("Invoice");
            if (invoice != null)
            {
                data.Invoice = invoice.ToObject<List<Tuple<string, string>>>();
            }

            #endregion


            #region Customer

            var customer = jsonObj.Value<JObject>("Customer");
            if (customer != null)
            {
                data.Customer = customer.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region TableHeaderRow, TableBodyRows, TableFooterRows, Total

            var tableHeaderRow = jsonObj.Value<JArray>("TableHeaderRow");
            if (tableHeaderRow != null)
            {
                data.TableHeaderRow = tableHeaderRow.ToObject<List<string>>();
            }

            var tableBodyRows = jsonObj.Value<JArray>("TableBodyRows");
            if (tableBodyRows != null)
            {
                data.TableBodyRows = tableBodyRows.ToObject<List<List<string>>>();
            }

            var tableFooterRows = jsonObj.Value<JArray>("TableFooterRows");
            if (tableFooterRows != null)
            {
                data.TableFooterRows = tableFooterRows.ToObject<List<Tuple<string, string>>>();
            }

            var tableTotalRow = jsonObj.Value<JObject>("TableTotalRow");
            if (tableTotalRow != null)
            {
                data.TableTotalRow = tableTotalRow.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Terms

            var terms = jsonObj.Value<JObject>("Terms");
            if (terms != null)
            {
                data.Terms = terms.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Notes

            var notes = jsonObj.Value<JObject>("Notes");
            if (notes != null)
            {
                data.Notes = notes.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Consignee

            var consignee = jsonObj.Value<JObject>("Consignee");
            if (consignee != null)
            {
                data.Consignee = consignee.ToObject<Tuple<string, string>>();
            }

            #endregion


            #region Addresses

            data.DeliveryAddressID = !String.IsNullOrEmpty(jsonObj.Value<string>("DeliveryAddressID")) ? jsonObj.Value<int>("DeliveryAddressID") : 0;
            data.BillingAddressID = !String.IsNullOrEmpty(jsonObj.Value<string>("BillingAddressID")) ? jsonObj.Value<int>("BillingAddressID") : 0;

            #endregion

            return data;
        }

        private static InvoiceFormattedData CreateDataAfterLinesUpdated(Invoice invoice,
            InvoiceFormattedData invoiceOldData)
        {
            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();

                var data = invoiceOldData;

                var cultureInfo = string.IsNullOrEmpty(invoice.Language)
                    ? CultureInfo.CurrentCulture
                    : CultureInfo.GetCultureInfo(invoice.Language);

                #region TableBodyRows, TableFooterRows, TableTotalRow

                data.TableBodyRows = new List<List<string>>();

                var invoiceLines = invoice.GetInvoiceLines(daoFactory);
                var invoiceTaxes = new Dictionary<int, decimal>();

                decimal subtotal = 0;
                decimal discount = 0;
                decimal amount = 0;

                foreach (var line in invoiceLines)
                {
                    var item = daoFactory.InvoiceItemDao.GetByID(line.InvoiceItemID);
                    var tax1 = line.InvoiceTax1ID > 0
                        ? daoFactory.InvoiceTaxDao.GetByID(line.InvoiceTax1ID)
                        : null;
                    var tax2 = line.InvoiceTax2ID > 0
                        ? daoFactory.InvoiceTaxDao.GetByID(line.InvoiceTax2ID)
                        : null;

                    var subtotalValue = Math.Round(line.Quantity*line.Price, 2);
                    var discountValue = Math.Round(subtotalValue*line.Discount/100, 2);

                    decimal rate = 0;
                    if (tax1 != null)
                    {
                        rate += tax1.Rate;
                        if (invoiceTaxes.ContainsKey(tax1.ID))
                        {
                            invoiceTaxes[tax1.ID] = invoiceTaxes[tax1.ID] +
                                                    Math.Round((subtotalValue - discountValue)*tax1.Rate/100, 2);
                        }
                        else
                        {
                            invoiceTaxes.Add(tax1.ID, Math.Round((subtotalValue - discountValue)*tax1.Rate/100, 2));
                        }
                    }
                    if (tax2 != null)
                    {
                        rate += tax2.Rate;
                        if (invoiceTaxes.ContainsKey(tax2.ID))
                        {
                            invoiceTaxes[tax2.ID] = invoiceTaxes[tax2.ID] +
                                                    Math.Round((subtotalValue - discountValue)*tax2.Rate/100, 2);
                        }
                        else
                        {
                            invoiceTaxes.Add(tax2.ID, Math.Round((subtotalValue - discountValue)*tax2.Rate/100, 2));
                        }
                    }

                    decimal taxValue = Math.Round((subtotalValue - discountValue)*rate/100, 2);
                    decimal amountValue = Math.Round(subtotalValue - discountValue + taxValue, 2);

                    subtotal += subtotalValue;
                    discount += discountValue;
                    amount += amountValue;

                    data.TableBodyRows.Add(new List<string>
                    {
                        item.Title + (string.IsNullOrEmpty(line.Description) ? string.Empty : ": " + line.Description),
                        line.Quantity.ToString(CultureInfo.InvariantCulture),
                        line.Price.ToString(CultureInfo.InvariantCulture),
                        line.Discount.ToString(CultureInfo.InvariantCulture),
                        tax1 != null ? tax1.Name : string.Empty,
                        tax2 != null ? tax2.Name : string.Empty,
                        (subtotalValue - discountValue).ToString(CultureInfo.InvariantCulture)
                    });
                }

                data.TableFooterRows = new List<Tuple<string, string>>();
                data.TableFooterRows.Add(
                    new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Subtotal", cultureInfo),
                        (subtotal - discount).ToString(CultureInfo.InvariantCulture)));

                foreach (var invoiceTax in invoiceTaxes)
                {
                    var iTax = daoFactory.InvoiceTaxDao.GetByID(invoiceTax.Key);
                    data.TableFooterRows.Add(new Tuple<string, string>(
                        string.Format("{0} ({1}%)", iTax.Name, iTax.Rate),
                        invoiceTax.Value.ToString(CultureInfo.InvariantCulture)));
                }

                //data.TableFooterRows.Add(new Tuple<string, string>(CRMInvoiceResource.ResourceManager.GetString("Discount", cultureInfo), "-" + discount.ToString(CultureInfo.InvariantCulture)));

                data.TableTotalRow =
                    new Tuple<string, string>(
                        string.Format("{0} ({1})", CRMInvoiceResource.ResourceManager.GetString("Total", cultureInfo),
                            invoice.Currency), amount.ToString(CultureInfo.InvariantCulture));


                #endregion


                return data;
            }
        }
    }

}