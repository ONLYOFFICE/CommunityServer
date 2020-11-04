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


#region Import

using System;
using System.Web;
using System.Collections.Generic;
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Resources;
using System.Threading;

#endregion

namespace ASC.Web.CRM.Controls.Deals
{
    public partial class DealFullCardView : BaseUserControl
    {
        #region Property

        public Deal TargetDeal { get; set; }
        public Contact TargetDealContact { get; set; }

        public static String Location
        {
            get { return PathProvider.GetFileStaticRelativePath("Deals/DealFullCardView.ascx"); }
        }

        public List<DealMilestone> AllDealMilestones;

        #endregion

        #region Events

        protected void Page_Load(object sender, EventArgs e)
        {
            if (TargetDeal.ContactID != 0) {
                TargetDealContact = DaoFactory.ContactDao.GetByID(TargetDeal.ContactID);
            }

            RegisterClientScriptHelper.DataDealFullCardView(Page, TargetDeal);
            AllDealMilestones = DaoFactory.DealMilestoneDao.GetAll();

            ExecHistoryView();
            RegisterScript();
        }

        #endregion

        #region Methods

        private void ExecHistoryView()
        {
            var historyViewControl = (HistoryView)LoadControl(HistoryView.Location);
            historyViewControl.TargetEntityID = TargetDeal.ID;
            historyViewControl.TargetEntityType = EntityType.Opportunity;

            _phHistoryView.Controls.Add(historyViewControl);
        }

        protected String RenderExpectedValue()
        {
            switch (TargetDeal.BidType)
            {
                case BidType.PerYear:
                    return String.Concat(CRMDealResource.BidType_PerYear, " ",
                                         String.Format(CRMJSResource.PerPeriodYears, TargetDeal.PerPeriodValue));
                case BidType.PerWeek:
                    return String.Concat(CRMDealResource.BidType_PerWeek, " ",
                                         String.Format(CRMJSResource.PerPeriodWeeks, TargetDeal.PerPeriodValue));
                case BidType.PerMonth:
                    return String.Concat(CRMDealResource.BidType_PerMonth, " ",
                                         String.Format(CRMJSResource.PerPeriodMonths, TargetDeal.PerPeriodValue));
                case BidType.PerHour:
                    return String.Concat(CRMDealResource.BidType_PerHour, " ",
                                         String.Format(CRMJSResource.PerPeriodHours, TargetDeal.PerPeriodValue));
                case BidType.PerDay:
                    return String.Concat(CRMDealResource.BidType_PerDay, " ",
                                         String.Format(CRMJSResource.PerPeriodDays, TargetDeal.PerPeriodValue));
                default:
                    return String.Empty;
            }
        }

        protected String GetExpectedCloseDateStr()
        {

            return TargetDeal.ExpectedCloseDate == DateTime.MinValue ?
                                            CRMJSResource.NoCloseDate :
                                            TargetDeal.ExpectedCloseDate.ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern);

        }

        protected String GetActualCloseDateStr()
        {
            return TargetDeal.ActualCloseDate == DateTime.MinValue ?
                                                CRMJSResource.NoCloseDate :
                                                TargetDeal.ActualCloseDate.ToString(Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern);

        }


        protected String GetExpectedValueStr()
        {
            if (TargetDeal.BidValue == 0)
                return CRMDealResource.NoExpectedValue;

            var currencyInfo = CurrencyProvider.Get(TargetDeal.BidCurrency);

            return String.Format("{2}{0:N} {1} <br/> <span>{3}</span>",
                                TargetDeal.BidValue,
                                currencyInfo.Abbreviation,
                                currencyInfo.Symbol,
                                RenderExpectedValue());
        }

        private void RegisterScript()
        {
            var script = @"ASC.CRM.DealFullCardView.init();";

            Page.RegisterInlineScript(script);
        }

        #endregion
    }
}