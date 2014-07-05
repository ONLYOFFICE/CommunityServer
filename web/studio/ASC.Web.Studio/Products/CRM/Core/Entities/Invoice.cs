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
