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


using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Controls.Common;
using ASC.Web.Projects.Controls.Tasks;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Utility;

using ASC.Projects.Core.Domain;
using ASC.Web.Studio.UserControls.Common.LoaderPage;

namespace ASC.Web.Projects
{
    public partial class Tasks : BasePage
    {
        protected override void PageLoad()
        {
            var taskID = UrlParameters.EntityID;

            if (taskID >= 0)
            {
                var task = EngineFactory.TaskEngine.GetByID(taskID);

                if (task == null || task.Project.ID != Project.ID)
                {
                    RedirectNotFound(string.Format("tasks.aspx?prjID={0}", Project.ID));
                }
                else
                {
                    InitTaskPage(task);
                }
            }
            else
            {
                _content.Controls.Add(LoadControl(CommonList.Location));

                Title = HeaderStringHelper.GetPageTitle(TaskResource.Tasks);
                loaderHolder.Controls.Add(LoadControl(LoaderPage.Location));
            }
        }

        private void InitTaskPage(Task task)
        {
            var taskDescriptionView = (TaskDescriptionView)LoadControl(PathProvider.GetFileStaticRelativePath("Tasks/TaskDescriptionView.ascx"));
            taskDescriptionView.Task = task;
            _content.Controls.Add(taskDescriptionView);

            EssenceTitle = task.Title;
            IsSubcribed = EngineFactory.TaskEngine.IsSubscribed(task);

            if ((int)task.Status == 2)
            {
                EssenceStatus = TaskResource.Closed;
            }

            Title = HeaderStringHelper.GetPageTitle(task.Title);
        }

    }
}