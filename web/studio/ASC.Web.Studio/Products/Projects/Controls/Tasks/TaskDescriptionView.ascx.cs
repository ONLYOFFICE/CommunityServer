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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.MessagingSystem;
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.Web.Core.Mobile;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Classes;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Controls.Tasks
{
    [AjaxNamespace("AjaxPro.TaskDescriptionView")]
    public partial class TaskDescriptionView : BaseUserControl
    {
        #region Properties

        public Task Task { get; set; }
        public string TaskTimeSpend { get; set; }
        public int AttachmentsCount { get; set; }

        public int SubtasksCount { get; set; }

        public bool CanCreateTimeSpend { get; set; }
        public bool CanReadFiles { get; set; }
        public bool CanEditTask { get; set; }
        public bool CanCreateSubtask { get; set; }
        public bool CanDeleteTask { get; set; }

        public int ProjectFolderId { get; set; }

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
            var attachments = FileEngine2.GetTaskFiles(Task);
            AttachmentsCount = attachments.Count();

            ProjectFolderId = (int)FileEngine2.GetRoot(Task.Project.ID);

            var taskAttachments = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            taskAttachments.EmptyScreenVisible = false;
            taskAttachments.EntityType = "task";
            taskAttachments.ModuleName = "projects";
            taskAttachments.CanAddFile = CanEditTask;
            phAttachmentsControl.Controls.Add(taskAttachments);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(TaskDescriptionView), Page);

            _hintPopup.Options.IsPopup = true;
            _hintPopupTaskRemove.Options.IsPopup = true;
            _newLinkError.Options.IsPopup = true;

            CanReadFiles = ProjectSecurity.CanReadFiles(Task.Project);
            CanEditTask = ProjectSecurity.CanEdit(Task);
            CanCreateSubtask = ProjectSecurity.CanCreateSubtask(Task);
            CanCreateTimeSpend = ProjectSecurity.CanCreateTimeSpend(Task);
            CanDeleteTask = ProjectSecurity.CanDelete(Task);
            SubtasksCount = Task.SubTasks.Count;

            if (CanReadFiles) InitAttachments();

            InitCommentBlock();

            var timeList = Global.EngineFactory.GetTimeTrackingEngine().GetByTask(Task.ID);
            TaskTimeSpend = timeList.Sum(timeSpend => timeSpend.Hours).ToString();
            TaskTimeSpend = TaskTimeSpend.Replace(',', '.');
        }

        #region Comment List Control Block

        private void InitCommentBlock()
        {
            commentList.Items = BuilderCommentInfo();
            ConfigureComments(commentList, Task);
        }

        private IList<CommentInfo> BuilderCommentInfo()
        {
            var comments = Global.EngineFactory.GetCommentEngine().GetComments(Task).ToList();
            comments.Sort((x, y) => DateTime.Compare(x.CreateOn, y.CreateOn));

            return comments.Where(r => r.Parent == Guid.Empty).Select(comment => GetCommentInfo(comments, comment)).ToList();
        }

        private CommentInfo GetCommentInfo(IEnumerable<Comment> allComments, Comment parent)
        {
            var creator = Global.EngineFactory.GetParticipantEngine().GetByID(parent.CreateBy).UserInfo;
            var commentInfo = new CommentInfo
                {
                    TimeStampStr = parent.CreateOn.Ago(),
                    Inactive = parent.Inactive,
                    IsRead = true,
                    IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                    IsEditPermissions = ProjectSecurity.CanEditComment(Task.Project, parent),
                    CommentID = parent.ID.ToString(),
                    CommentBody = parent.Content,
                    UserID = parent.CreateBy,
                    UserFullName = creator.DisplayUserName(),
                    UserPost = creator.Title,
                    UserAvatar = Global.GetHTMLUserAvatar(creator),
                    CommentList = new List<CommentInfo>(),
                };

            if (allComments != null)
                foreach (var comment in allComments.Where(comment => comment.Parent == parent.ID))
                {
                    commentInfo.CommentList.Add(GetCommentInfo(allComments, comment));
                }

            return commentInfo;
        }

        #endregion

        #region Comment Block Managment

        [AjaxMethod]
        public AjaxResponse AddComment(string parrentCommentID, int taskID, string text, string pid)
        {
            var taskEngine = Global.EngineFactory.GetTaskEngine();
            ProjectSecurity.DemandCreateComment();

            var comment = new Comment
                {
                    Content = text,
                    TargetUniqID = ProjectEntity.BuildUniqId<Task>(taskID)
                };

            if (!String.IsNullOrEmpty(parrentCommentID))
                comment.Parent = new Guid(parrentCommentID);

            Task = taskEngine.GetByID(taskID);

            if (Task == null) return new AjaxResponse { status = "error", message = "Access denied." };

            comment = taskEngine.SaveOrUpdateComment(Task, comment);
            MessageService.Send(HttpContext.Current.Request, MessageAction.TaskCommentCreated, Task.Project.Title, Task.Title);

            return new AjaxResponse { rs1 = parrentCommentID, rs2 = GetHTMLComment(comment) };
        }

        private string GetHTMLComment(Comment comment)
        {
            var creator = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo;
            var oCommentInfo = new CommentInfo
                {
                    TimeStamp = comment.CreateOn,
                    TimeStampStr = comment.CreateOn.Ago(),
                    CommentBody = comment.Content,
                    CommentID = comment.ID.ToString(),
                    UserID = comment.CreateBy,
                    UserFullName = creator.DisplayUserName(),
                    Inactive = comment.Inactive,
                    IsEditPermissions = ProjectSecurity.CanEditComment(Task != null ? Task.Project : null, comment),
                    IsResponsePermissions = ProjectSecurity.CanCreateComment(),
                    IsRead = true,
                    UserAvatar = Global.GetHTMLUserAvatar(creator),
                    UserPost = creator.Title
                };

            if (commentList == null)
            {
                commentList = new CommentsList();
                ConfigureComments(commentList, null);

            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                commentList,
                oCommentInfo,
                comment.Parent == Guid.Empty,
                false);
        }

        [AjaxMethod]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {
            var taskEngine = Global.EngineFactory.GetTaskEngine();
            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
            comment.Content = text;

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var target = taskEngine.GetByID(targetID);

            if (target == null) return new AjaxResponse { status = "error", message = "Access denied." };

            var targetProject = target.Project;
            ProjectSecurity.DemandEditComment(targetProject, comment);

            comment = taskEngine.SaveOrUpdateComment(target, comment);
            MessageService.Send(HttpContext.Current.Request, MessageAction.TaskCommentUpdated, target.Project.Title, target.Title);

            return new AjaxResponse { rs1 = commentID, rs2 = HtmlUtility.GetFull(text) + CodeHighlighter.GetJavaScriptLiveHighlight(true) };
        }

        [AjaxMethod]
        public string RemoveComment(string commentID, string pid)
        {
            var commentEngine = Global.EngineFactory.GetCommentEngine();
            var comment = commentEngine.GetByID(new Guid(commentID));
            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var target = Global.EngineFactory.GetTaskEngine().GetByID(targetID);
            var targetProject = target.Project;

            ProjectSecurity.DemandEditComment(targetProject, comment);
            ProjectSecurity.DemandRead(target);

            comment.Inactive = true;

            comment = commentEngine.SaveOrUpdate(comment);
            MessageService.Send(HttpContext.Current.Request, MessageAction.TaskCommentDeleted, target.Project.Title, target.Title);

            return commentID;
        }

        [AjaxMethod]
        public string GetPreview(string text, string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            return GetHTMLComment(text, commentID);
        }

        [AjaxMethod]
        public string LoadCommentBBCode(string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            var finded = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));

            return finded != null ? finded.Content : String.Empty;
        }

        private string GetHTMLComment(string text, string commentID)
        {
            var comment = new Comment
                {
                    Content = text,
                    CreateOn = TenantUtil.DateTimeNow(),
                    CreateBy = SecurityContext.CurrentAccount.ID

                };

            if (!String.IsNullOrEmpty(commentID))
            {
                comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));
                comment.Content = text;
            }

            return GetHTMLComment(comment, true);
        }

        private string GetHTMLComment(Comment comment, bool isPreview)
        {
            var creator = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy).UserInfo;
            var info = new CommentInfo
                {
                    CommentID = comment.ID.ToString(),
                    UserID = comment.CreateBy,
                    TimeStamp = comment.CreateOn,
                    TimeStampStr = comment.CreateOn.Ago(),
                    UserPost = creator.Title,
                    Inactive = comment.Inactive,
                    CommentBody = comment.Content,
                    UserFullName = DisplayUserSettings.GetFullUserName(creator),
                    UserAvatar = Global.GetHTMLUserAvatar(creator)
                };

            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            if (!isPreview)
            {
                info.IsRead = true;
                info.IsEditPermissions = ProjectSecurity.CanEditComment(Task.Project, comment);
                info.IsResponsePermissions = ProjectSecurity.CanCreateComment();
            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                defComment,
                info,
                comment.Parent == Guid.Empty,
                false);
        }

        private static void ConfigureComments(CommentsList commentList, Task taskToUpdate)
        {
            var commentsCount = Global.EngineFactory.GetCommentEngine().Count(taskToUpdate);

            CommonControlsConfigurer.CommentsConfigure(commentList);

            commentList.IsShowAddCommentBtn = ProjectSecurity.CanCreateComment();

            commentList.CommentsCountTitle = commentsCount != 0 ? commentsCount.ToString() : "";

            commentList.ObjectID = taskToUpdate != null ? taskToUpdate.ID.ToString() : "";
            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "AjaxPro.TaskDescriptionView.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "AjaxPro.TaskDescriptionView.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "AjaxPro.TaskDescriptionView.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "AjaxPro.TaskDescriptionView.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "AjaxPro.TaskDescriptionView.UpdateComment";
            commentList.FckDomainName = "projects_comments";
            commentList.OnRemovedCommentJS = "ASC.Projects.TaskDescroptionPage.onDeleteComment";

            commentList.TotalCount = commentsCount;
        }

        #endregion
    }
}