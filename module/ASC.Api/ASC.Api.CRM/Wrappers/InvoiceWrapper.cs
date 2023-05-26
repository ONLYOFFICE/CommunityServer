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


#region Import

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;
using ASC.Web.CRM.Classes;
#endregion

namespace ASC.Api.CRM.Wrappers
{

    /// <summary>
    ///  Invoice
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "invoiceBase", Namespace = "")]
    public class InvoiceBaseWrapper : ObjectWrapperBase
    {
        public InvoiceBaseWrapper(int id)
            : base(id)
        {
        }

        public InvoiceBaseWrapper(Invoice invoice)
            : base(invoice.ID)
        {
            Status = new InvoiceStatusWrapper(invoice.Status);
            Number = invoice.Number;
            IssueDate = (ApiDateTime)invoice.IssueDate;
            TemplateType = invoice.TemplateType;
            DueDate = (ApiDateTime)invoice.DueDate;
            Currency = !String.IsNullOrEmpty(invoice.Currency) ?
                new CurrencyInfoWrapper(CurrencyProvider.Get(invoice.Currency)) :
                new CurrencyInfoWrapper(Global.TenantSettings.DefaultCurrency);
            ExchangeRate = invoice.ExchangeRate;
            Language = invoice.Language;
            PurchaseOrderNumber = invoice.PurchaseOrderNumber;
            Terms = invoice.Terms;
            Description = invoice.Description;
            FileID = invoice.FileID;
            CreateOn = (ApiDateTime)invoice.CreateOn;
            CreateBy = EmployeeWraper.Get(invoice.CreateBy);
            CanEdit = CRMSecurity.CanEdit(invoice);
            CanDelete = CRMSecurity.CanDelete(invoice);
        }

        ///<type>ASC.Api.CRM.Wrappers.InvoiceStatusWrapper, ASC.Api.CRM</type>
        [DataMember]
        public InvoiceStatusWrapper Status { get; set; }

        ///<example></example>
        [DataMember]
        public string Number { get; set; }

        ///<example>2020-12-14T22:13:41.5378233Z</example>
        [DataMember]
        public ApiDateTime IssueDate { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public InvoiceTemplateType TemplateType { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM</type>
        [DataMember]
        public ContactBaseWrapper Contact { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactBaseWrapper, ASC.Api.CRM</type>
        [DataMember]
        public ContactBaseWrapper Consignee { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.EntityWrapper, ASC.Api.CRM</type>
        [DataMember]
        public EntityWrapper Entity { get; set; }

        ///<example>2020-12-14T22:13:41.5378233Z</example>
        [DataMember]
        public ApiDateTime DueDate { get; set; }

        ///<example></example>
        [DataMember]
        public string Language { get; set; }

        ///<type>ASC.Api.CRM.CurrencyInfoWrapper, ASC.Api.CRM</type>
        [DataMember]
        public CurrencyInfoWrapper Currency { get; set; }

        ///<example type="double">1,0</example>
        [DataMember]
        public decimal ExchangeRate { get; set; }

        ///<example></example>
        [DataMember]
        public string PurchaseOrderNumber { get; set; }

        ///<example></example>
        [DataMember]
        public string Terms { get; set; }

        ///<example></example>
        [DataMember]
        public string Description { get; set; }

        ///<example type="int">-1</example>
        [DataMember]
        public int FileID { get; set; }

        ///<example>2020-12-14T22:13:41.5378233Z</example>
        [DataMember]
        public ApiDateTime CreateOn { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember]
        public EmployeeWraper CreateBy { get; set; }

        ///<example type="double">0,0</example>
        [DataMember]
        public decimal Cost { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }
    }

    /// <summary>
    ///  Invoice
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.InvoiceBaseWrapper, ASC.Api.CRM</inherited>
    [DataContract(Name = "invoice", Namespace = "")]
    public class InvoiceWrapper : InvoiceBaseWrapper
    {
        public InvoiceWrapper(int id)
            : base(id)
        {
        }

        public InvoiceWrapper(Invoice invoice)
            : base(invoice.ID)
        {
            Status = new InvoiceStatusWrapper(invoice.Status);
            Number = invoice.Number;
            IssueDate = (ApiDateTime)invoice.IssueDate;
            TemplateType = invoice.TemplateType;
            DueDate = (ApiDateTime)invoice.DueDate;
            Currency = !String.IsNullOrEmpty(invoice.Currency) ?
                new CurrencyInfoWrapper(CurrencyProvider.Get(invoice.Currency)) :
                new CurrencyInfoWrapper(Global.TenantSettings.DefaultCurrency);
            ExchangeRate = invoice.ExchangeRate;
            Language = invoice.Language;
            PurchaseOrderNumber = invoice.PurchaseOrderNumber;
            Terms = invoice.Terms;
            Description = invoice.Description;
            FileID = invoice.FileID;
            CreateOn = (ApiDateTime)invoice.CreateOn;
            CreateBy = EmployeeWraper.Get(invoice.CreateBy);
            CanEdit = CRMSecurity.CanEdit(invoice);
            CanDelete = CRMSecurity.CanDelete(invoice);
        }

        ///<type>ASC.Api.CRM.Wrappers.InvoiceLineWrapper, ASC.Api.CRM</type>
        ///<collection>list</collection>
        [DataMember]
        public List<InvoiceLineWrapper> InvoiceLines { get; set; }

        public static InvoiceWrapper GetSample()
        {
            return new InvoiceWrapper(0)
            {
                Status = new InvoiceStatusWrapper(InvoiceStatus.Draft),
                Number = string.Empty,
                IssueDate = ApiDateTime.GetSample(),
                TemplateType = InvoiceTemplateType.Eur,
                Language = string.Empty,
                DueDate = ApiDateTime.GetSample(),
                Currency = CurrencyInfoWrapper.GetSample(),
                ExchangeRate = (decimal)1.00,
                PurchaseOrderNumber = string.Empty,
                Terms = string.Empty,
                Description = string.Empty,
                FileID = -1,
                CreateOn = ApiDateTime.GetSample(),
                CreateBy = EmployeeWraper.GetSample(),
                CanEdit = true,
                CanDelete = true,
                Cost = 0,
                InvoiceLines = new List<InvoiceLineWrapper> { InvoiceLineWrapper.GetSample() }
            };
        }
    }

    /// <summary>
    ///  Invoice Item
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "invoiceItem", Namespace = "")]
    public class InvoiceItemWrapper : ObjectWrapperBase
    {
        public InvoiceItemWrapper(int id)
            : base(id)
        {
        }

        public InvoiceItemWrapper(InvoiceItem invoiceItem)
            : base(invoiceItem.ID)
        {
            Title = invoiceItem.Title;
            StockKeepingUnit = invoiceItem.StockKeepingUnit;
            Description = invoiceItem.Description;
            Price = invoiceItem.Price;
            StockQuantity = invoiceItem.StockQuantity;
            TrackInvenory = invoiceItem.TrackInventory;

            CreateOn = (ApiDateTime)invoiceItem.CreateOn;
            CreateBy = EmployeeWraper.Get(invoiceItem.CreateBy);
            Currency = !String.IsNullOrEmpty(invoiceItem.Currency) ?
                new CurrencyInfoWrapper(CurrencyProvider.Get(invoiceItem.Currency)) :
                new CurrencyInfoWrapper(Global.TenantSettings.DefaultCurrency);
            CanEdit = CRMSecurity.CanEdit(invoiceItem);
            CanDelete = CRMSecurity.CanDelete(invoiceItem);
        }

        ///<example>Title</example>
        [DataMember]
        public string Title { get; set; }

        ///<example>StockKeepingUnit</example>
        [DataMember]
        public string StockKeepingUnit { get; set; }

        ///<example>Description</example>
        [DataMember]
        public string Description { get; set; }

        ///<example type="double">1.2</example>
        [DataMember]
        public decimal Price { get; set; }

        ///<type>ASC.Api.CRM.CurrencyInfoWrapper, ASC.Api.CRM</type>
        [DataMember]
        public CurrencyInfoWrapper Currency { get; set; }

        ///<example type="decimal">2.2</example>
        [DataMember]
        public decimal StockQuantity { get; set; }

        ///<example>true</example>
        [DataMember]
        public bool TrackInvenory { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.InvoiceTaxWrapper, ASC.Api.CRM</type>
        [DataMember]
        public InvoiceTaxWrapper InvoiceTax1 { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.InvoiceTaxWrapper, ASC.Api.CRM</type>
        [DataMember]
        public InvoiceTaxWrapper InvoiceTax2 { get; set; }

        ///<example>2020-12-14T22:13:41.5378233Z</example>
        [DataMember]
        public ApiDateTime CreateOn { get; set; }

        ///<example>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</example>
        [DataMember]
        public EmployeeWraper CreateBy { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }
    }

    /// <summary>
    ///  Invoice Tax
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "invoiceTax", Namespace = "")]
    public class InvoiceTaxWrapper : ObjectWrapperBase
    {

        public InvoiceTaxWrapper(int id)
            : base(id)
        {
        }

        public InvoiceTaxWrapper(InvoiceTax invoiceTax)
            : base(invoiceTax.ID)
        {
            Name = invoiceTax.Name;
            Description = invoiceTax.Description;
            Rate = invoiceTax.Rate;
            CreateOn = (ApiDateTime)invoiceTax.CreateOn;
            CreateBy = EmployeeWraper.Get(invoiceTax.CreateBy);
            CanEdit = CRMSecurity.CanEdit(invoiceTax);
            CanDelete = CRMSecurity.CanDelete(invoiceTax);
        }


        ///<example>Name</example>
        [DataMember]
        public string Name { get; set; }

        ///<example>Description</example>
        [DataMember]
        public string Description { get; set; }

        ///<example type="double">Rate</example>
        [DataMember]
        public decimal Rate { get; set; }

        ///<example>2020-12-14T22:13:41.5378233Z</example>
        [DataMember]
        public ApiDateTime CreateOn { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        [DataMember]
        public EmployeeWraper CreateBy { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        ///<example>true</example>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }
    }

    /// <summary>
    ///  Invoice Line
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "invoiceLine", Namespace = "")]
    public class InvoiceLineWrapper : ObjectWrapperBase
    {
        public InvoiceLineWrapper(int id)
            : base(id)
        {
        }

        public InvoiceLineWrapper(InvoiceLine invoiceLine)
            : base(invoiceLine.ID)
        {
            InvoiceID = invoiceLine.InvoiceID;
            InvoiceItemID = invoiceLine.InvoiceItemID;
            InvoiceTax1ID = invoiceLine.InvoiceTax1ID;
            InvoiceTax2ID = invoiceLine.InvoiceTax2ID;
            SortOrder = invoiceLine.SortOrder;
            Description = invoiceLine.Description;
            Quantity = invoiceLine.Quantity;
            Price = invoiceLine.Price;
            Discount = invoiceLine.Discount;
        }

        ///<example type="int">0</example>
        [DataMember]
        public int InvoiceID { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int InvoiceItemID { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int InvoiceTax1ID { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int InvoiceTax2ID { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int SortOrder { get; set; }

        ///<example>Description</example>
        [DataMember]
        public string Description { get; set; }

        ///<example type="double">0,0</example>
        [DataMember]
        public decimal Quantity { get; set; }

        ///<example type="double">0,0</example>
        [DataMember]
        public decimal Price { get; set; }

        ///<example type="double">0,0</example>
        [DataMember]
        public decimal Discount { get; set; }

        public static InvoiceLineWrapper GetSample()
        {
            return new InvoiceLineWrapper(0)
            {
                Description = string.Empty,
                Discount = (decimal)0.00,
                InvoiceID = 0,
                InvoiceItemID = 0,
                InvoiceTax1ID = 0,
                InvoiceTax2ID = 0,
                Price = (decimal)0.00,
                Quantity = (decimal)0.00
            };
        }
    }

    /// <summary>
    ///  Invoice Status
    /// </summary>
    /// <inherited>ASC.Api.CRM.Wrappers.ObjectWrapperBase, ASC.Api.CRM</inherited>
    [DataContract(Name = "invoiceStatus", Namespace = "")]
    public class InvoiceStatusWrapper : ObjectWrapperBase
    {

        public InvoiceStatusWrapper(int id)
            : base(id)
        {
            Title = ((InvoiceStatus)id).ToLocalizedString();
        }

        public InvoiceStatusWrapper(InvoiceStatus status)
            : base((int)status)
        {
            Title = status.ToLocalizedString();
        }

        ///<example>Title</example>
        [DataMember]
        public string Title { get; set; }
    }

}
