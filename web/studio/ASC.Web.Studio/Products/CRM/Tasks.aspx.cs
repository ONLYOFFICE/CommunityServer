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