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

using System;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Cases;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.CRM.Core;

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
                ASC.CRM.Core.Entities.Cases targetCase = Global.DaoFactory.GetCasesDao().GetByID(caseID);

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
        }

        protected void ExecCasesDetailsView(ASC.CRM.Core.Entities.Cases targetCase)
        {
            var casesDetailsViewControl = (CasesDetailsView)LoadControl(CasesDetailsView.Location);
            casesDetailsViewControl.TargetCase = targetCase;
            CommonContainerHolder.Controls.Add(casesDetailsViewControl);

            var title = targetCase.Title.HtmlEncode();

            Master.CurrentPageCaption = title;
            Master.CommonContainerHeader = Global.RenderItemHeaderWithMenu(title, EntityType.Case, CRMSecurity.IsPrivate(targetCase), true);

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