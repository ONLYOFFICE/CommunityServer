/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Mobile;
using ASC.Web.Core.Users;
using ASC.Web.Projects.Resources;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;
using ASC.Web.Projects.Classes;
using ASC.Web.Projects.Configuration;
using ASC.Web.Studio.Utility.HtmlUtility;
using AjaxPro;

namespace ASC.Web.Projects.Controls.Messages
{
    [AjaxNamespace("AjaxPro.DiscussionDetails")]
    public partial class DiscussionDetails : BaseUserControl
    {
        public Message Discussion { get; set; }

        public Project Project
        {
            get { return Page.Project; }
        }

        public UserInfo Author { get; set; }

        public bool CanReadFiles { get; set; }
        public bool CanEditFiles { get; set; }
        public bool CanEdit { get; set; }

        public int FilesCount { get; private set; }
        public int ProjectFolderId { get; set; }

        protected bool FilesAvailable { get; set; }
        protected bool CommentsAvailable { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Utility.RegisterTypeForAjax(typeof(DiscussionDetails), Page);

            _hintPopup.Options.IsPopup = true;

            BindDiscussionParticipants();

            CanEdit = ProjectSecurity.CanEdit(Discussion);
            CanReadFiles = ProjectSecurity.CanReadFiles(Discussion.Project);
            CanEditFiles = ProjectSecurity.IsInTeam(Project) && Discussion.Status == MessageStatus.Open;
            Author = CoreContext.UserManager.GetUsers(Discussion.CreateBy);
            FilesCount = FileEngine2.GetMessageFiles(Discussion).Count();
            FilesAvailable = CanReadFiles && !MobileDetector.IsMobile && (CanEditFiles || FilesCount > 0);
            CommentsAvailable = Discussion.Status == MessageStatus.Open || Discussion.CommentsCount > 0;

            if (FilesAvailable)
            {
                LoadDiscussionFilesControl();
            }

            if (Discussion.Status == MessageStatus.Archived)
            {
                Page.EssenceStatus = MessageResource.ArchiveDiscussionStatus;
            }

            if (CommentsAvailable)
            {
                LoadCommentsControl();
            }
            else
            {
                discussionComments.Visible = false;
            }
        }

        #region LoadControls

        private void BindDiscussionParticipants()
        {
            var participants = Global.EngineFactory.GetMessageEngine().GetSubscribers(Discussion)
                                     .Select(r => new ParticipiantWrapper(r.ID, Discussion))
                                     .OrderBy(r => r.FullUserName);

            var newList = new List<ParticipiantWrapper>();
            newList.AddRange(participants.Where(r => r.CanRead));
            newList.AddRange(participants.Where(r => !r.CanRead));
        }

        private void LoadDiscussionFilesControl()
        {
            ProjectFolderId = (int)FileEngine2.GetRoot(Project.ID);

            var discussionFilesControl = (Studio.UserControls.Common.Attachments.Attachments)LoadControl(Studio.UserControls.Common.Attachments.Attachments.Location);
            discussionFilesControl.EmptyScreenVisible = false;
            discussionFilesControl.EntityType = "message";
            discussionFilesControl.ModuleName = "projects";
            discussionFilesControl.CanAddFile = CanEditFiles;
            discussionFilesPlaceHolder.Controls.Add(discussionFilesControl);
        }

        #endregion

        #region Tabs

        protected string GetTabTitle(int count, string defaultTitle)
        {
            return count > 0 ? string.Format("{0} ({1})", defaultTitle, count) : defaultTitle;
        }

        #endregion

        #region ParticipiantWrapper

        protected class ParticipiantWrapper
        {
            public string ID { get; set; }
            public string Link { get; set; }
            public string FullUserName { get; set; }
            public string Title { get; set; }
            public string Department { get; set; }
            public bool CanRead { get; set; }

            public ParticipiantWrapper(string id, Message message)
            {
                ID = id;

                var participant = Global.EngineFactory.GetParticipantEngine().GetByID(new Guid(id));

                Link = participant.UserInfo.RenderProfileLink(ProductEntryPoint.ID);
                FullUserName = DisplayUserSettings.GetFullUserName(participant.UserInfo);
                Title = HttpUtility.HtmlEncode(participant.UserInfo.Title);
                Department = string.Join(", ", CoreContext.UserManager.GetUserGroups(participant.UserInfo.ID).Select(d => d.Name.HtmlEncode()));
                CanRead = ProjectSecurity.CanRead(message, new Guid(id));
            }
        }

        #endregion

        #region Comments

        private void LoadCommentsControl()
        {
            discussionComments.Items = BuilderCommentInfo();
            ConfigureComments(discussionComments, Discussion);
        }

        private static void ConfigureComments(CommentsList commentList, Message messageToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);

            var countMessageToUpdate = messageToUpdate != null ? Global.EngineFactory.GetCommentEngine().Count(messageToUpdate) : 0;

            commentList.IsShowAddCommentBtn = ((messageToUpdate != null && messageToUpdate.Status == MessageStatus.Open) || messageToUpdate == null) && ProjectSecurity.CanCreateComment();
            commentList.CommentsCountTitle = countMessageToUpdate != 0 ? countMessageToUpdate.ToString(CultureInfo.InvariantCulture) : "0";
            commentList.ObjectID = messageToUpdate != null
                                       ? messageToUpdate.ID.ToString(CultureInfo.InvariantCulture) : "";

            commentList.Simple = false;
            commentList.BehaviorID = "commentsObj";
            commentList.JavaScriptAddCommentFunctionName = "AjaxPro.DiscussionDetails.AddComment";
            commentList.JavaScriptLoadBBcodeCommentFunctionName = "AjaxPro.DiscussionDetails.LoadCommentBBCode";
            commentList.JavaScriptPreviewCommentFunctionName = "AjaxPro.DiscussionDetails.GetPreview";
            commentList.JavaScriptRemoveCommentFunctionName = "AjaxPro.DiscussionDetails.RemoveComment";
            commentList.JavaScriptUpdateCommentFunctionName = "AjaxPro.DiscussionDetails.UpdateComment";
            commentList.OnRemovedCommentJS = "ASC.Projects.DiscussionDetails.removeComment";
            commentList.FckDomainName = "projects_comments";
            commentList.TotalCount = countMessageToUpdate;
        }

        [AjaxMethod]
        public AjaxResponse AddComment(string parrentCommentID, string messageID, string text, string pid)
        {
            ProjectSecurity.DemandCreateComment();

            var resp = new AjaxResponse();

            var comment = new Comment
                {
                    Content = text,
                    TargetUniqID = ProjectEntity.BuildUniqId<Message>(Convert.ToInt32(messageID))
                };

            resp.rs1 = parrentCommentID;

            if (!string.IsNullOrEmpty(parrentCommentID))
            {
                comment.Parent = new Guid(parrentCommentID);
            }

            var messageEngine = Global.EngineFactory.GetMessageEngine();
            Discussion = messageEngine.GetByID(Convert.ToInt32(messageID));

            if (Discussion == null) return new AjaxResponse {status = "error", message = "Access denied."};

            ProjectSecurity.DemandCreateComment(Discussion);

            comment = messageEngine.SaveOrUpdateComment(Discussion, comment);
            MessageService.Send(HttpContext.Current.Request, MessageAction.DiscussionCommentCreated, Discussion.Project.Title, Discussion.Title);

            resp.rs2 = GetHTMLComment(comment);
            return resp;
        }

        private string GetHTMLComment(Comment comment)
        {
            var creator = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy);
            var commentInfo = new CommentInfo
                {
                    TimeStamp = comment.CreateOn,
                    TimeStampStr = comment.CreateOn.Ago(),
                    CommentBody = comment.Content,
                    CommentID = comment.ID.ToString(),
                    UserID = comment.CreateBy,
                    UserFullName = creator.UserInfo.DisplayUserName(),
                    Inactive = comment.Inactive,
                    IsEditPermissions = ProjectSecurity.CanEditComment(Discussion, comment),
                    IsResponsePermissions = ProjectSecurity.CanCreateComment(Discussion),
                    IsRead = true,
                    UserAvatar = Global.GetHTMLUserAvatar(creator.UserInfo),
                    UserPost = creator.UserInfo.Title
                };

            if (discussionComments == null)
            {
                discussionComments = new CommentsList();
                ConfigureComments(discussionComments, null);
            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                discussionComments,
                commentInfo,
                comment.Parent == Guid.Empty,
                false);
        }

        [AjaxMethod]
        public AjaxResponse UpdateComment(string commentID, string text, string pid)
        {
            var messageEngine = Global.EngineFactory.GetMessageEngine();
            var resp = new AjaxResponse {rs1 = commentID};

            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentID));

            comment.Content = text;

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var target = messageEngine.GetByID(targetID);

            if (target == null) return new AjaxResponse {status = "error", message = "Access denied."};

            Discussion = target;

            ProjectSecurity.DemandEditComment(Discussion, comment);

            messageEngine.SaveOrUpdateComment(target, comment);
            MessageService.Send(HttpContext.Current.Request, MessageAction.DiscussionCommentUpdated, Discussion.Project.Title, Discussion.Title);

            resp.rs2 = HtmlUtility.GetFull(text) + CodeHighlighter.GetJavaScriptLiveHighlight(true);
            return resp;
        }

        [AjaxMethod]
        public string RemoveComment(string commentID, string pid)
        {
            var commentEngine = Global.EngineFactory.GetCommentEngine();
            var comment = commentEngine.GetByID(new Guid(commentID));

            comment.Inactive = true;

            var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
            var target = Global.EngineFactory.GetMessageEngine().GetByID(targetID);

            ProjectSecurity.DemandEditComment(target, comment);
            ProjectSecurity.DemandRead(target);

            Discussion = target;

            commentEngine.SaveOrUpdate(comment);
            MessageService.Send(HttpContext.Current.Request, MessageAction.DiscussionCommentDeleted, Discussion.Project.Title, Discussion.Title);

            return commentID;
        }

        [AjaxMethod]
        public string GetPreview(string text, string commentID)
        {
            ProjectSecurity.DemandAuthentication();

            return GetHTMLComment(text, commentID);
        }

        [AjaxMethod]
        public string LoadCommentBBCode(string commentId)
        {
            ProjectSecurity.DemandAuthentication();

            var comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentId));
            return comment != null ? comment.Content : string.Empty;
        }


        private string GetHTMLComment(Comment comment, bool isPreview)
        {
            var creator = Global.EngineFactory.GetParticipantEngine().GetByID(comment.CreateBy);
            var commentInfo = new CommentInfo
                {
                    CommentID = comment.ID.ToString(),
                    UserID = comment.CreateBy,
                    TimeStamp = comment.CreateOn,
                    TimeStampStr = comment.CreateOn.Ago(),
                    UserPost = creator.UserInfo.Title,
                    IsRead = true,
                    Inactive = comment.Inactive,
                    CommentBody = comment.Content,
                    UserFullName = DisplayUserSettings.GetFullUserName(creator.UserInfo),
                    UserAvatar = Global.GetHTMLUserAvatar(creator.UserInfo)
                };

            var defComment = new CommentsList();
            ConfigureComments(defComment, null);

            if (!isPreview)
            {
                var targetID = Convert.ToInt32(comment.TargetUniqID.Split('_')[1]);
                var target = Global.EngineFactory.GetMessageEngine().GetByID(targetID);
                Discussion = target;

                commentInfo.IsEditPermissions = ProjectSecurity.CanEditComment(Discussion, comment);
                commentInfo.IsResponsePermissions = ProjectSecurity.CanCreateComment(Discussion);
                commentInfo.IsRead = true;
            }

            return CommentsHelper.GetOneCommentHtmlWithContainer(
                defComment,
                commentInfo,
                comment.Parent == Guid.Empty,
                false);
        }

        private string GetHTMLComment(string text, string commentId)
        {
            Comment comment;
            if (!string.IsNullOrEmpty(commentId))
            {
                comment = Global.EngineFactory.GetCommentEngine().GetByID(new Guid(commentId));
                comment.Content = text;
            }
            else
            {
                comment = new Comment
                    {
                        Content = text,
                        CreateOn = TenantUtil.DateTimeNow(),
                        CreateBy = SecurityContext.CurrentAccount.ID
                    };
            }
            return GetHTMLComment(comment, true);
        }

        private IList<CommentInfo> BuilderCommentInfo()
        {
            var comments = Global.EngineFactory.GetCommentEngine().GetComments(Discussion);
            comments.Sort((x, y) => DateTime.Compare(x.CreateOn, y.CreateOn));

            return (from comment in comments where comment.Parent == Guid.Empty select GetCommentInfo(comments, comment)).ToList();
        }

        private CommentInfo GetCommentInfo(IEnumerable<Comment> allComments, Comment parent)
        {
            var creator = Global.EngineFactory.GetParticipantEngine().GetByID(parent.CreateBy).UserInfo;
            var commentInfo = new CommentInfo
                {
                    TimeStampStr = parent.CreateOn.Ago(),
                    IsRead = true,
                    Inactive = parent.Inactive,
                    IsResponsePermissions = ProjectSecurity.CanCreateComment(Discussion),
                    IsEditPermissions = ProjectSecurity.CanEditComment(Discussion, parent),
                    CommentID = parent.ID.ToString(),
                    CommentBody = parent.Content,
                    UserID = parent.CreateBy,
                    UserFullName = creator.DisplayUserName(),
                    UserPost = creator.Title,
                    UserAvatar = Global.GetHTMLUserAvatar(creator),
                    CommentList = new List<CommentInfo>(),
                };

            if (allComments != null)
            {
                foreach (var comment in allComments.Where(comment => comment.Parent == parent.ID))
                {
                    commentInfo.CommentList.Add(GetCommentInfo(allComments, comment));
                }
            }
            return commentInfo;
        }

        #endregion
    }
}