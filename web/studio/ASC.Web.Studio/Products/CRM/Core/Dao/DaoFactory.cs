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