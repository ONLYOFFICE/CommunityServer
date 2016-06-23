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
using System.Globalization;
using System.Linq;
using ASC.Web.Core.Mobile;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Projects.Classes;

namespace ASC.Web.Projects.Controls.Tasks
{
    public partial class TaskDescriptionView : BaseCommentControl<Task>
    {
        #region Properties

        public Task Task { get; set; }
        public string TaskTimeSpend { get; set; }
        public int AttachmentsCount { get; set; }

        public int SubtasksCount { get; set; }

        public bool CanCreateTimeSpend { get; set; }
        public bool CanAddFiles { get; set; }
        public bool CanEditTask { get; set; }
        public bool CanCreateSubtask { get; set; }
        public bool CanDeleteTask { get; set; }
        public bool DoInitAttachments { get; set; }

        public int ProjectFolderId { get; set; }
        protected string CommentsCountTitle { get; set; }

        public bool ShowGanttChartFlag
        {
            get
            {
                return !MobileDetector.IsMobile && ProjectSecurity.CanReadGantt(Task.Project);
            }
        }

        #endregion

        public void InitAttachments()
        {
            var attachments = Page.EngineFactory.TaskEngine.GetFiles(Task);
            AttachmentsCount = attachments.Count();

            CanAddFiles = CanEditTask && Task.Project.Status == ProjectStatus.Open;
            DoInitAttachments = ProjectSecurity.CanReadFiles(Task.Project) && (CanAddFiles || AttachmentsCount > 0);

            if(!DoInitAttachments) return;

            ProjectFolderId = (int)Page.EngineFactory.FileEngine.GetRoot(Task.Project.ID);

            var taskAttachments = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            taskAttachments.EmptyScreenVisible = false;
            taskAttachments.EntityType = "task";
            taskAttachments.ModuleName = "projects";
            taskAttachments.CanAddFile = CanAddFiles;
            phAttachmentsControl.Controls.Add(taskAttachments);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            _hintPopup.Options.IsPopup = true;
            _hintPopupTaskRemove.Options.IsPopup = true;
            _newLinkError.Options.IsPopup = true;

            SubtasksCount = Task.SubTasks.Count;

            CanEditTask = ProjectSecurity.CanEdit(Task);
            CanCreateSubtask = ProjectSecurity.CanCreateSubtask(Task);
            CanCreateTimeSpend = ProjectSecurity.CanCreateTimeSpend(Task);
            CanDeleteTask = ProjectSecurity.CanDelete(Task);

            InitAttachments();
            InitCommentBlock(commentList, Task);

            var timeList = Page.EngineFactory.TimeTrackingEngine.GetByTask(Task.ID);
            TaskTimeSpend = timeList.Sum(timeSpend => timeSpend.Hours).ToString();
            TaskTimeSpend = TaskTimeSpend.Replace(',', '.');

            var taskCount = Page.EngineFactory.CommentEngine.Count(Task);
            CommentsCountTitle = taskCount != 0 ? string.Format("({0})", taskCount.ToString(CultureInfo.InvariantCulture)) : "";
        }
    }
}