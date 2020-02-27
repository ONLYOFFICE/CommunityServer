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
                    Response.Redirect(PathProvider.StartURL() + "cases.aspx");

                if (string.Compare(UrlParameters.Action, "manage", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    if (!CRMSecurity.CanEdit(targetCase))
                    {
                        Response.Redirect(PathProvider.StartURL() + "cases.aspx");
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