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

            return String.Format("{2}{0:N} {1} <br/> <span>{3}</span>", TargetDeal.BidValue,
                                 currencyInfo.Abbreviation, currencyInfo.Symbol,
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