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
using ASC.CRM.Core.Entities;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Deals;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.CRM
{
    public partial class Deals : BasePage
    {
        protected override void PageLoad()
        {
            int dealID;

            

            if (int.TryParse(Request["id"], out dealID))
            {
                var targetDeal = Global.DaoFactory.GetDealDao().GetByID(dealID);
                if (targetDeal == null || !CRMSecurity.CanAccessTo(targetDeal))
                {
                    Response.Redirect(PathProvider.StartURL() + "deals.aspx");
                }


                if (String.Compare(Request["action"], "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!CRMSecurity.CanEdit(targetDeal))
                    {
                        Response.Redirect(PathProvider.StartURL() + "deals.aspx");
                    }
                    ExecDealActionView(targetDeal);
                }
                else
                {
                    ExecDealDetailsView(targetDeal);
                }
            }
            else
            {
                if (String.Compare(Request["action"], "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecDealActionView(null);
                }
                else if (String.Compare(UrlParameters.Action, "import", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecImportView();
                }
                else
                {
                    ExecListDealView();
                }
            }
        }

        #region Methods

        protected void ExecImportView()
        {
            var importViewControl = (ImportFromCSVView)LoadControl(ImportFromCSVView.Location);
            importViewControl.EntityType = EntityType.Opportunity;
            CommonContainerHolder.Controls.Add(importViewControl);

            Master.CurrentPageCaption = CRMDealResource.ImportDeals;
            Title = HeaderStringHelper.GetPageTitle(CRMDealResource.ImportDeals);
        }

        protected void ExecDealDetailsView(Deal targetDeal)
        {
            if (!CRMSecurity.CanAccessTo(targetDeal))
            {
                Response.Redirect(PathProvider.StartURL());
            }

            var dealActionViewControl = (DealDetailsView)LoadControl(DealDetailsView.Location);
            dealActionViewControl.TargetDeal = targetDeal;
            CommonContainerHolder.Controls.Add(dealActionViewControl);

            var headerTitle = targetDeal.Title;

            Master.CurrentPageCaption = headerTitle;

            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(headerTitle.HtmlEncode(), EntityType.Opportunity, CRMSecurity.IsPrivate(targetDeal), true);

            Title = HeaderStringHelper.GetPageTitle(headerTitle);
        }

        protected void ExecListDealView()
        {
            var listDealViewControl = (ListDealView)LoadControl(ListDealView.Location);
            CommonContainerHolder.Controls.Add(listDealViewControl);

            var headerTitle = CRMDealResource.AllDeals;
            if (!String.IsNullOrEmpty(Request["userID"])) headerTitle = CRMDealResource.MyDeals;
            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? headerTitle);
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
        }

        protected void ExecDealActionView(Deal targetDeal)
        {
            var dealActionViewControl = (DealActionView)LoadControl(DealActionView.Location);
            dealActionViewControl.TargetDeal = targetDeal;
            CommonContainerHolder.Controls.Add(dealActionViewControl);

            var headerTitle = targetDeal == null ? CRMDealResource.CreateNewDeal : String.Format(CRMDealResource.EditDealLabel, targetDeal.Title);
            Master.CurrentPageCaption = headerTitle;
            Title = HeaderStringHelper.GetPageTitle(headerTitle);
        }

        #endregion
    }
}