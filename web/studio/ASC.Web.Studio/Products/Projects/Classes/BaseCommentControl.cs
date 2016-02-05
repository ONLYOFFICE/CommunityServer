using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ASC.Core.Users;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Projects.Classes
{
    public class BaseCommentControl<T> : BaseUserControl where T : ProjectEntity
    {
        public void InitCommentBlock(CommentsList commentList, ProjectEntity entity)
        {
            commentList.Items = BuilderCommentInfo(entity);
            ConfigureComments(commentList, entity);
        }

        private IList<CommentInfo> BuilderCommentInfo(ProjectEntity entity)
        {
            var comments = Page.EngineFactory.CommentEngine.GetComments(entity).ToList();
            comments.Sort((x, y) => DateTime.Compare(x.CreateOn, y.CreateOn));

            return comments.Where(r => r.Parent == Guid.Empty).Select(comment => GetCommentInfo(comments, comment, entity)).ToList();
        }

        private CommentInfo GetCommentInfo(IEnumerable<Comment> allComments, Comment parent, ProjectEntity entity)
        {
            var creator = Page.EngineFactory.ParticipantEngine.GetByID(parent.CreateBy).UserInfo;
            var commentInfo = new CommentInfo
            {
                TimeStampStr = parent.CreateOn.Ago(),
                Inactive = parent.Inactive,
                IsRead = true,
                IsResponsePermissions = ProjectSecurity.CanCreateComment(entity),
                IsEditPermissions = ProjectSecurity.CanEditComment(entity, parent),
                CommentID = parent.OldGuidId.ToString(),
                CommentBody = parent.Content,
                UserID = parent.CreateBy,
                UserFullName = creator.DisplayUserName(),
                UserProfileLink = creator.GetUserProfilePageURL(),
                UserPost = creator.Title,
                UserAvatarPath = creator.GetBigPhotoURL(),
                CommentList = new List<CommentInfo>(),
            };

            if (allComments != null)
                foreach (var comment in allComments.Where(comment => comment.Parent == parent.OldGuidId))
                {
                    commentInfo.CommentList.Add(GetCommentInfo(allComments, comment, entity));
                }

            return commentInfo;
        }

        private void ConfigureComments(CommentsList commentList, ProjectEntity taskToUpdate)
        {
            CommonControlsConfigurer.CommentsConfigure(commentList);
            var commentsCount = Page.EngineFactory.CommentEngine.Count(taskToUpdate);

            commentList.IsShowAddCommentBtn = ProjectSecurity.CanCreateComment(taskToUpdate);

            commentList.ObjectID = taskToUpdate != null ? taskToUpdate.ID.ToString(CultureInfo.InvariantCulture) : "";
            commentList.BehaviorID = "commentsObj";
            commentList.ModuleName = "projects_" + typeof(T).Name;
            commentList.FckDomainName = "projects_comments";
            commentList.OnRemovedCommentJS = "ASC.Projects.Common.removeComment";
            commentList.TotalCount = commentsCount;
        }
    }
}