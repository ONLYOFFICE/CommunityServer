/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ASC.Api.Employee;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;
using ASC.Core.Tenants;
using ASC.Web.CRM.Classes;
#endregion

namespace ASC.Api.CRM.Wrappers
{

    /// <summary>
    ///  Invoice
    /// </summary>
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
            IssueDate = (ApiDateTime) invoice.IssueDate;
            TemplateType = invoice.TemplateType;
            DueDate = (ApiDateTime) invoice.DueDate;
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


        [DataMember]
        public InvoiceStatusWrapper Status { get; set; }

        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public ApiDateTime IssueDate { get; set; }

        [DataMember]
        public InvoiceTemplateType TemplateType { get; set; }

        [DataMember]
        public ContactBaseWithEmailWrapper Contact { get; set; }

        [DataMember]
        public ContactBaseWithEmailWrapper Consignee { get; set; }

        [DataMember]
        public EntityWrapper Entity { get; set; }

        [DataMember]
        public ApiDateTime DueDate { get; set; }

        [DataMember]
        public string Language { get; set; }

        [DataMember]
        public CurrencyInfoWrapper Currency { get; set; }

        [DataMember]
        public decimal ExchangeRate { get; set; }

        [DataMember]
        public string PurchaseOrderNumber { get; set; }

        [DataMember]
        public string Terms { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int FileID { get; set; }

        [DataMember]
        public ApiDateTime CreateOn { get; set; }

        [DataMember]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember]
        public decimal Cost { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }
    }

    /// <summary>
    ///  Invoice
    /// </summary>
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
            IssueDate = (ApiDateTime) invoice.IssueDate;
            TemplateType = invoice.TemplateType;
            DueDate = (ApiDateTime) invoice.DueDate;
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
                InvoiceLines = new List<InvoiceLineWrapper>{ InvoiceLineWrapper.GetSample() }
            };
        }
    }

    /// <summary>
    ///  Invoice Item
    /// </summary>
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
            Quantity = invoiceItem.Quantity;
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


        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string StockKeepingUnit { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public decimal Price { get; set; }

        [DataMember]
        public CurrencyInfoWrapper Currency { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public int StockQuantity { get; set; }

        [DataMember]
        public bool TrackInvenory { get; set; }

        [DataMember]
        public InvoiceTaxWrapper InvoiceTax1 { get; set; }

        [DataMember]
        public InvoiceTaxWrapper InvoiceTax2 { get; set; }

        [DataMember]
        public ApiDateTime CreateOn { get; set; }

        [DataMember]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }
    }

    /// <summary>
    ///  Invoice Tax
    /// </summary>
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



        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public decimal Rate { get; set; }

        [DataMember]
        public ApiDateTime CreateOn { get; set; }

        [DataMember]
        public EmployeeWraper CreateBy { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanEdit { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public bool CanDelete { get; set; }
    }

    /// <summary>
    ///  Invoice Line
    /// </summary>
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

        [DataMember]
        public int InvoiceID { get; set; }

        [DataMember]
        public int InvoiceItemID { get; set; }

        [DataMember]
        public int InvoiceTax1ID { get; set; }

        [DataMember]
        public int InvoiceTax2ID { get; set; }

        [DataMember]
        public int SortOrder { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Quantity { get; set; }

        [DataMember]
        public decimal Price { get; set; }

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
                    Quantity = 0
                };
        }
    }

    /// <summary>
    ///  Invoice Status
    /// </summary>
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


        [DataMember]
        public string Title { get; set; }
    }

}
