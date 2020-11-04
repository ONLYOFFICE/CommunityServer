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
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Cases;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.CRM
{
    public partial class Cases : BasePage
    {
        #region Events

        protected override void PageLoad()
        {
            int caseID;

            if (int.TryParse(UrlParameters.ID, out caseID))
            {
                ASC.CRM.Core.Entities.Cases targetCase = DaoFactory.CasesDao.GetByID(caseID);

                if (targetCase == null || !CRMSecurity.CanAccessTo(targetCase))
                    Response.Redirect(PathProvider.StartURL() + "Cases.aspx");

                if (string.Compare(UrlParameters.Action, "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!CRMSecurity.CanEdit(targetCase))
                    {
                        Response.Redirect(PathProvider.StartURL() + "Cases.aspx");
                    }
                    ExecCasesActionView(targetCase);
                }
                else
                {
                    ExecCasesDetailsView(targetCase);
                }
            }
            else
            {
                if (string.Compare(UrlParameters.Action, "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecCasesActionView(null);
                }
                else if (string.Compare(UrlParameters.Action, "import", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    ExecImportView();
                }
                else
                {
                    ExecListCasesView();
                }
            }
        }

        #endregion

        #region Methods

        protected void ExecImportView()
        {
            var importViewControl = (ImportFromCSVView)LoadControl(ImportFromCSVView.Location);
            importViewControl.EntityType = EntityType.Case;
            CommonContainerHolder.Controls.Add(importViewControl);

            Master.CurrentPageCaption = CRMCasesResource.ImportCases;
            Title = HeaderStringHelper.GetPageTitle(CRMCasesResource.ImportCases);
        }

        protected void ExecListCasesView()
        {
            CommonContainerHolder.Controls.Add(LoadControl(ListCasesView.Location));
            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? CRMCasesResource.AllCases);
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
        }

        protected void ExecCasesDetailsView(ASC.CRM.Core.Entities.Cases targetCase)
        {
            var casesDetailsViewControl = (CasesDetailsView)LoadControl(CasesDetailsView.Location);
            casesDetailsViewControl.TargetCase = targetCase;
            CommonContainerHolder.Controls.Add(casesDetailsViewControl);

            var title = targetCase.Title;

            Master.CurrentPageCaption = title;

            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(title.HtmlEncode(), EntityType.Case, CRMSecurity.IsPrivate(targetCase), true);

            TabsHolder.Visible = true;

            Title = HeaderStringHelper.GetPageTitle(title);
        }

        protected void ExecCasesActionView(ASC.CRM.Core.Entities.Cases targetCase)
        {
            var casesActionViewControl = (CasesActionView)LoadControl(CasesActionView.Location);

            casesActionViewControl.TargetCase = targetCase;
            CommonContainerHolder.Controls.Add(casesActionViewControl);

            var headerTitle = targetCase == null ? CRMCasesResource.CreateNewCase : String.Format(CRMCasesResource.EditCaseHeader, targetCase.Title);

            Master.CurrentPageCaption = headerTitle;
            Title = HeaderStringHelper.GetPageTitle(headerTitle);
        }

        #endregion
    }
}