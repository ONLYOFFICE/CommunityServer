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
using ASC.CRM.Core;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Tasks;
using ASC.Web.CRM.Resources;
using ASC.Web.Studio.Utility;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.CRM
{
    public partial class Tasks : BasePage
    {
        #region Events

        protected override void PageLoad()
        {
            if (string.Compare(UrlParameters.Action, "import", StringComparison.OrdinalIgnoreCase) == 0)
            {
                ExecImportView();
            }
            else
            {
                ExecListTaskView();
            }
        }

        #endregion

        #region Methods

        protected void ExecImportView()
        {
            var importViewControl = (ImportFromCSVView)LoadControl(ImportFromCSVView.Location);
            importViewControl.EntityType = EntityType.Task;
            CommonContainerHolder.Controls.Add(importViewControl);

            Master.CurrentPageCaption = CRMTaskResource.ImportTasks;
            Title = HeaderStringHelper.GetPageTitle(CRMTaskResource.ImportTasks);
        }

        protected void ExecListTaskView()
        {
            CommonContainerHolder.Controls.Add(LoadControl(ListTaskView.Location));
            Title = HeaderStringHelper.GetPageTitle(Master.CurrentPageCaption ?? CRMTaskResource.Tasks);
            loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
        }

        protected void ExecTaskDetailsView(int taskID)
        {
            var task = DaoFactory.TaskDao.GetByID(taskID);

            if (!CRMSecurity.CanAccessTo(task))
            {
                Response.Redirect(PathProvider.StartURL());
            }

            Master.CurrentPageCaption = task.Title;

            var closedBy = string.Empty;

            if (task.IsClosed)
            {
                closedBy = string.Format("<div class='crm_taskTitleClosedByPanel'>{0}<div>", CRMTaskResource.ClosedTask);
            }

            Master.CommonContainerHeader = string.Format("{0}{1}", task.Title.HtmlEncode(), closedBy);

            Title = HeaderStringHelper.GetPageTitle(task.Title);
        }

        #endregion
    }
}