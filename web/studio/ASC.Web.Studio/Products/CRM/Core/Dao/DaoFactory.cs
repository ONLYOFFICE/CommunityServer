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


using ASC.VoipService.Dao;
using Autofac;

namespace ASC.CRM.Core.Dao
{
    public class DaoFactory
    {
        private readonly TypedParameter tenant;
        public ILifetimeScope Container { get; set; }

        #region Constructor

        public DaoFactory(int tenantID)
        {
            tenant = GetParameter(tenantID);
        }

        #endregion

        #region Methods

        public TaskDao TaskDao
        {
            get { return Container.Resolve<TaskDao>(tenant); }
        }

        public ListItemDao CachedListItem
        {
            get { return Container.Resolve<ListItemDao>(tenant); }
        }

        public ContactDao ContactDao
        {
            get { return Container.Resolve<ContactDao>(tenant); }
        }

        public CustomFieldDao CustomFieldDao
        {
            get { return Container.Resolve<CustomFieldDao>(tenant); }
        }

        public DealDao DealDao
        {
            get { return Container.Resolve<DealDao>(tenant); }
        }

        public DealMilestoneDao DealMilestoneDao
        {
            get { return Container.Resolve<DealMilestoneDao>(tenant); }
        }

        public ListItemDao ListItemDao
        {
            get { return Container.Resolve<ListItemDao>(tenant); }
        }

        public TagDao TagDao
        {
            get { return Container.Resolve<TagDao>(tenant); }
        }

        public SearchDao SearchDao
        {
            get { return Container.Resolve<SearchDao>(tenant, GetParameter(this)); }
        }

        public RelationshipEventDao RelationshipEventDao
        {
            get { return Container.Resolve<RelationshipEventDao>(tenant); }
        }

        public FileDao FileDao
        {
            get { return Container.Resolve<FileDao>(tenant); }
        }

        public CasesDao CasesDao
        {
            get { return Container.Resolve<CasesDao>(tenant); }
        }

        public TaskTemplateContainerDao TaskTemplateContainerDao
        {
            get { return Container.Resolve<TaskTemplateContainerDao>(tenant); }
        }

        public TaskTemplateDao TaskTemplateDao
        {
            get { return Container.Resolve<TaskTemplateDao>(tenant); }
        }

        public ReportDao ReportDao
        {
            get { return Container.Resolve<ReportDao>(tenant); }
        }

        public CurrencyRateDao CurrencyRateDao
        {
            get { return Container.Resolve<CurrencyRateDao>(tenant); }
        }

        public CurrencyInfoDao CurrencyInfoDao
        {
            get { return Container.Resolve<CurrencyInfoDao>(tenant); }
        }

        public ContactInfoDao ContactInfoDao
        {
            get { return Container.Resolve<ContactInfoDao>(tenant); }
        }

        public InvoiceDao InvoiceDao
        {
            get { return Container.Resolve<InvoiceDao>(tenant); }
        }

        public InvoiceItemDao InvoiceItemDao
        {
            get { return Container.Resolve<InvoiceItemDao>(tenant); }
        }

        public InvoiceTaxDao InvoiceTaxDao
        {
            get { return Container.Resolve<InvoiceTaxDao>(tenant); }
        }

        public InvoiceLineDao InvoiceLineDao
        {
            get { return Container.Resolve<InvoiceLineDao>(tenant); }
        }

        public VoipDao VoipDao
        {
            get { return Container.Resolve<VoipDao>(tenant); }
        }

        private TypedParameter GetParameter<T>(T data)
        {
            return new TypedParameter(typeof(T), data);
        }
        #endregion
    }
}