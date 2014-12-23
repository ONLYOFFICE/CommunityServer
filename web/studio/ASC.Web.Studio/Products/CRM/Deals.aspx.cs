/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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

            var headerTitle = targetDeal.Title.HtmlEncode();

            Master.CurrentPageCaption = headerTitle;

            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(headerTitle, EntityType.Opportunity, CRMSecurity.IsPrivate(targetDeal), true);

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