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
using ASC.Common.Security;
using ASC.Files.Core;
using ASC.Web.CRM.Classes;


namespace ASC.CRM.Core.Entities
{
    public class Invoice : DomainObject, ISecurityObjectId
    {
        public InvoiceStatus Status { get; set; }

        public string Number { get; set; }

        public DateTime IssueDate { get; set; }

        public InvoiceTemplateType TemplateType { get; set; }

        public int ContactID { get; set; }

        public int ConsigneeID { get; set; }

        public EntityType EntityType { get; set; }

        public int EntityID { get; set; }

        public DateTime DueDate { get; set; }

        public string Language { get; set; }

        public string Currency { get; set; }

        public decimal ExchangeRate { get; set; }

        public string PurchaseOrderNumber { get; set; }

        public string Terms { get; set; }

        public string Description { get; set; }

        public string JsonData { get; set; }

        public int FileID { get; set; }


        public DateTime CreateOn { get; set; }

        public Guid CreateBy { get; set; }

        public DateTime? LastModifedOn { get; set; }
        
        public Guid? LastModifedBy { get; set; }



        public object SecurityId
        {
            get { return ID; }
        }

        public Type ObjectType
        {
            get { return GetType(); }
        }


        public List<InvoiceLine> GetInvoiceLines()
        {
            return Global.DaoFactory.GetInvoiceLineDao().GetInvoiceLines(ID);
        }

        public File GetInvoiceFile()
        {
            return Global.DaoFactory.GetFileDao().GetFile(FileID, 0);
        }

        public decimal GetInvoiceCost()
        {
            var lines = GetInvoiceLines();
            decimal cost = 0;
            foreach (var line in lines)
            {
                InvoiceTax tax;
                decimal taxRate = 0;
                
                if (line.InvoiceTax1ID > 0)
                {
                    tax = Global.DaoFactory.GetInvoiceTaxDao().GetByID(line.InvoiceTax1ID);
                    if (tax != null)
                    {
                        taxRate += tax.Rate;
                    } 
                }
                if (line.InvoiceTax2ID > 0)
                {
                    tax = Global.DaoFactory.GetInvoiceTaxDao().GetByID(line.InvoiceTax2ID);
                    if (tax != null)
                    {
                        taxRate += tax.Rate;
                    }
                }

                cost += (line.Price * line.Quantity) * (1 - line.Discount / 100 + taxRate / 100);
            }
            return Math.Round(cost, 2);
        }
    }
}
