/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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

using ASC.VoipService.Dao;

namespace ASC.CRM.Core.Dao
{
    public class DaoFactory
    {

        #region Members

        private readonly int tenantID;
        private readonly String storageKey;

        #endregion

        #region Constructor

        public DaoFactory(int tenantID, String storageKey)
        {
            this.tenantID = tenantID;
            this.storageKey = storageKey;
        }

        #endregion

        #region Methods

        public TaskDao GetTaskDao()
        {
            return new TaskDao(tenantID, storageKey);
        }

        public CachedListItem GetCachedListItem()
        {

            return new CachedListItem(tenantID, storageKey);
        }

        public CachedContactDao GetContactDao()
        {
            return new CachedContactDao(tenantID, storageKey);
        }

        public CustomFieldDao GetCustomFieldDao()
        {
           
            return new CustomFieldDao(tenantID, storageKey);
        }

        public DealDao GetDealDao()
        {
            return new CachedDealDao(tenantID, storageKey);
        }

        public DealMilestoneDao GetDealMilestoneDao()
        {
            return new CachedDealMilestoneDao(tenantID,storageKey);
        }

        public ListItemDao GetListItemDao()
        {
            return new ListItemDao(tenantID, storageKey);
        }
        
        public TagDao GetTagDao()
        {
            return new TagDao(tenantID, storageKey);
        }

        public SearchDao GetSearchDao()
        {
            return new SearchDao(tenantID, storageKey);
        }

        public RelationshipEventDao GetRelationshipEventDao()
        {
            return new RelationshipEventDao(tenantID, storageKey);
        }

        public FileDao GetFileDao()
        {
            return new FileDao(tenantID, storageKey);
        }

        public CasesDao GetCasesDao()
        {
            return new CachedCasesDao(tenantID, storageKey);
        }

        public TaskTemplateContainerDao GetTaskTemplateContainerDao()
        {
            return new TaskTemplateContainerDao(tenantID, storageKey);
        }

        public TaskTemplateDao GetTaskTemplateDao()
        {
            return new TaskTemplateDao(tenantID, storageKey);
        }

        public ReportDao GetReportDao()
        {

            return new ReportDao(tenantID, storageKey);

        }

        public CurrencyRateDao GetCurrencyRateDao()
        {
            return new CurrencyRateDao(tenantID, storageKey);
        }

        public CurrencyInfoDao GetCurrencyInfoDao()
        {
            return new CurrencyInfoDao(tenantID, storageKey);
        }

        public ContactInfoDao GetContactInfoDao()
        {
            return new ContactInfoDao(tenantID, storageKey);
        }

        public InvoiceDao GetInvoiceDao()
        {
            return new CachedInvoiceDao(tenantID, storageKey);
        }

        public InvoiceItemDao GetInvoiceItemDao()
        {
            return new CachedInvoiceItemDao(tenantID, storageKey);
        }

        public InvoiceTaxDao GetInvoiceTaxDao()
        {
            return new CachedInvoiceTaxDao(tenantID, storageKey);
        }

        public InvoiceLineDao GetInvoiceLineDao()
        {
            return new CachedInvoiceLineDao(tenantID, storageKey);
        }

        public VoipDao GetVoipDao()
        {
            return new CachedVoipDao(tenantID, storageKey);
        }

        #endregion 
    }
}
