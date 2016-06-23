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
                TargetDealContact = Global.DaoFactory.GetContactDao().GetByID(TargetDeal.ContactID);
            }

            RegisterClientScriptHelper.DataDealFullCardView(Page, TargetDeal);
            AllDealMilestones = Global.DaoFactory.GetDealMilestoneDao().GetAll();

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
                                String.Equals(currencyInfo.Abbreviation, "RUB", StringComparison.OrdinalIgnoreCase) ? "<span class='rub'>Р</span>" : currencyInfo.Symbol,
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