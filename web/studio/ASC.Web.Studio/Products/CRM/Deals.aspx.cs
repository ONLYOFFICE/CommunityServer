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
        protected string HelpLink { get; set; }
        
        protected override void PageLoad()
        {
            int dealID;

            HelpLink = CommonLinkUtility.GetHelpLink();

            if (int.TryParse(Request["id"], out dealID))
            {
                var targetDeal = DaoFactory.DealDao.GetByID(dealID);
                if (targetDeal == null || !CRMSecurity.CanAccessTo(targetDeal))
                {
                    Response.Redirect(PathProvider.StartURL() + "Deals.aspx");
                }


                if (String.Compare(Request["action"], "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!CRMSecurity.CanEdit(targetDeal))
                    {
                        Response.Redirect(PathProvider.StartURL() + "Deals.aspx");
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

            TabsHolder.Visible = true;

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