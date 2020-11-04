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
using System.Linq;
using ASC.Common.Security;
using ASC.CRM.Core.Dao;
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


        public List<InvoiceLine> GetInvoiceLines(DaoFactory daoFactory)
        {
            return daoFactory.InvoiceLineDao.GetInvoiceLines(ID);
        }

        public File GetInvoiceFile(DaoFactory daoFactory)
        {
            return daoFactory.FileDao.GetFile(FileID, 0);
        }

        public decimal GetInvoiceCost(DaoFactory daoFactory)
        {
            var lines = GetInvoiceLines(daoFactory);

            var taxIDs = new HashSet<int>();

            foreach (var line in lines)
            {
                taxIDs.Add(line.InvoiceTax1ID);
                taxIDs.Add(line.InvoiceTax2ID);
            }

            var taxes = daoFactory.InvoiceTaxDao.GetByID(taxIDs.ToArray());

            return daoFactory.InvoiceDao.CalculateInvoiceCost(lines, taxes);
        }
    }
}
