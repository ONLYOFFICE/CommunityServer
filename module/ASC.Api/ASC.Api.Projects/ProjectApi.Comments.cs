/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.Collections.Generic;
using System.Linq;

using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.Core;
using ASC.Core.Tenants;
using ASC.Core.Users;
using ASC.MessagingSystem;
using ASC.Projects.Core.Domain;
using ASC.Projects.Engine;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility.HtmlUtility;

namespace ASC.Api.Projects
{
    public partial class ProjectApi
    {
        #region comments

        /// <summary>
        /// Returns the information about a comment with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a comment
        /// </short>
        /// <category>Comments</category>
        /// <param type="System.Guid, System" method="url" name="commentid">Comment ID</param>
        /// <returns type="ASC.Api.Projects.Wrappers.CommentWrapper, ASC.Api.Projects">Comment</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <path>api/2.0/project/comment/{commentid}</path>
        /// <httpMethod>GET</httpMethod>
        [Read(@"comment/{commentid}")]
        public CommentWrapper GetComment(Guid commentid)
        {
            var comment = EngineFactory.CommentEngine.GetByID(commentid).NotFoundIfNull();
            var entity = EngineFactory.CommentEngine.GetEntityByTargetUniqId(comment).NotFoundIfNull();

            return new CommentWrapper(this, comment, entity);
        }

        /////<summary>
        /////Updates the seleted comment using the comment text specified in the request.
        /////</summary>
        /////<short>
        /////Update a comment
        /////</short>
        /////<category>Comments</category>
        /////<param type="System.Guid, System" name="commentid">Comment ID</param>
        /////<param type="System.String, System" name="content">Comment text</param>
        /////<returns>Updated comment</returns>
        /////<exception cref="ItemNotFoundException"></exception>
        /////<example>
        /////<![CDATA[
        /////Sending data in application/json:
        /////
        /////{
        /////    text:"My comment text",
        /////    
        /////}
        /////
        /////Sending data in application/x-www-form-urlencoded
        /////content=My%20comment%20text
        /////]]>
        /////</example>
        //[Update(@"comment/{commentid}")]
        //public CommentWrapper UpdateComments(Guid commentid, string content)
        //{
        //    var comment = EngineFactory.CommentEngine.GetByID(commentid).NotFoundIfNull();
        //    comment.Content = Update.IfNotEquals(comment.Content, content);

        //    string type;
        //    comment = SaveComment(comment, out type);

        //    return new CommentWrapper(comment);
        //}

        /// <summary>
        /// Returns a preview of a project comment with the ID specified in the request.
        /// </summary>
        /// <short>
        /// Get a comment preview
        /// </short>
        /// <category>Comments</category>
        /// <param type="System.String, System" name="htmltext">Comment text in the HTML format</param>
        /// <param type="System.String, System" name="commentid">Comment ID, or empty string if a comment is new</param>
        /// <returns type="ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio">Comment information</returns>
        /// <path>api/2.0/project/comment/preview</path>
        /// <httpMethod>POST</httpMethod>
        [Create(@"comment/preview")]
        public CommentInfo GetProjectCommentPreview(string htmltext, string commentid)
        {
            ProjectSecurity.DemandAuthentication();

            var commentEngine = EngineFactory.CommentEngine;

            Comment comment;
            if (!string.IsNullOrEmpty(commentid))
            {
                comment = commentEngine.GetByID(new Guid(commentid));
                comment.Content = htmltext;
            }
            else
            {
                comment = new Comment
                {
                    Content = htmltext,
                    CreateOn = TenantUtil.DateTimeNow(),
                    CreateBy = SecurityContext.CurrentAccount.ID
                };
            }

            var creator = EngineFactory.ParticipantEngine.GetByID(comment.CreateBy).UserInfo;
            var info = new CommentInfo
            {
                CommentID = comment.OldGuidId.ToString(),
                UserID = comment.CreateBy,
                TimeStamp = comment.CreateOn,
                TimeStampStr = comment.CreateOn.Ago(),
                UserPost = creator.Title,
                Inactive = comment.Inactive,
                CommentBody = HtmlUtility.GetFull(comment.Content),
                UserFullName = DisplayUserSettings.GetFullUserName(creator),
                UserProfileLink = creator.GetUserProfilePageURL(),
                UserAvatarPath = creator.GetBigPhotoURL()
            };

            return info;
        }

        /// <summary>
        /// Removes a comment with the ID specified in the request.
        /// </summary>
        /// <short>Remove a comment</short>
        /// <param type="System.String, System" method="url" name="commentid">Comment ID</param>
        /// <returns>Comment ID</returns>
        /// <category>Comments</category>
        /// <path>api/2.0/project/comment/{commentid}</path>
        /// <httpMethod>DELETE</httpMethod>
        [Delete("comment/{commentid}")]
        public string RemoveProjectComment(string commentid)
        {
            var commentEngine = EngineFactory.CommentEngine;

            var comment = commentEngine.GetByID(new Guid(commentid)).NotFoundIfNull();
            comment.Inactive = true;

            var entity = commentEngine.GetEntityByTargetUniqId(comment);
            if (entity == null) return "";

            ProjectSecurity.DemandEditComment(entity.Project, comment);

            commentEngine.SaveOrUpdate(comment);
            MessageService.Send(Request, MessageAction.TaskCommentDeleted, MessageTarget.Create(comment.ID), entity.Project.Title, entity.Title);

            return commentid;
        }

        /// <summary>
        /// Adds a project comment with the parameters specified in the request. The parent comment ID can also be selected.
        /// </summary>
        /// <short>Add a project comment</short>
        /// <param type="System.String, System" name="parentcommentid">Parent comment ID</param>
        /// <param type="System.Int32, System" name="entityid">Entity ID</param>
        /// <param type="System.String, System" name="content">Comment text</param>
        /// <param type="System.String, System" name="type">Comment type (message or task)</param>
        /// <category>Projects</category>
        /// <returns type="ASC.Web.Studio.UserControls.Common.Comments.CommentInfo, ASC.Web.Studio">Comment information</returns>
        /// <path>api/2.0/project/comment</path>
        /// <httpMethod>POST</httpMethod>
        [Create("comment")]
        public CommentInfo AddProjectComment(string parentcommentid, int entityid, string content, string type)
        {
            if (string.IsNullOrEmpty(type) || !(new List<string> { "message", "task" }).Contains(type.ToLower()))
                throw new ArgumentException();

            var isMessageComment = type.ToLower().Equals("message");
            var comment = isMessageComment
                ? new Comment { Content = content, TargetUniqID = ProjectEntity.BuildUniqId<Message>(entityid) }
                : new Comment { Content = content, TargetUniqID = ProjectEntity.BuildUniqId<Task>(entityid) };


            if (!string.IsNullOrEmpty(parentcommentid))
                comment.Parent = new Guid(parentcommentid);

            var commentEngine = EngineFactory.CommentEngine;
            var entity = commentEngine.GetEntityByTargetUniqId(comment).NotFoundIfNull();

            comment = commentEngine.SaveOrUpdateComment(entity, comment);

            MessageService.Send(Request, isMessageComment ? MessageAction.DiscussionCommentCreated : MessageAction.TaskCommentCreated, MessageTarget.Create(comment.ID), entity.Project.Title, entity.Title);

            return GetCommentInfo(null, comment, entity);
        }

        /// <summary>
        /// Updates the seleted comment using the comment text specified in the request.
        /// </summary>
        /// <short>Update a comment</short>
        /// <param type="System.String, System" method="url" name="commentid">Comment ID</param>
        /// <param type="System.String, System" name="content">New comment text</param>
        /// <category>Comments</category>
        /// <returns>Updated comment</returns>
        /// <path>api/2.0/project/comment/{commentid}</path>
        /// <httpMethod>PUT</httpMethod>
        [Update("comment/{commentid}")]
        public string UpdateComment(string commentid, string content)
        {
            var commentEngine = EngineFactory.CommentEngine;
            var comment = commentEngine.GetByID(new Guid(commentid));
            comment.Content = content;

            var entity = commentEngine.GetEntityByTargetUniqId(comment);
            if (entity == null) throw new Exception("Access denied.");

            commentEngine.SaveOrUpdateComment(entity, comment);

            MessageService.Send(Request, MessageAction.TaskCommentUpdated, MessageTarget.Create(comment.ID), entity.Project.Title, entity.Title);

            return HtmlUtility.GetFull(content);
        }

        internal CommentInfo GetCommentInfo(IEnumerable<Comment> allComments, Comment comment, ProjectEntity entity)
        {
            var creator = EngineFactory.ParticipantEngine.GetByID(comment.CreateBy).UserInfo;
            var oCommentInfo = new CommentInfo
            {
                TimeStamp = comment.CreateOn,
                TimeStampStr = comment.CreateOn.Ago(),
                CommentBody = HtmlUtility.GetFull(comment.Content),
                CommentID = comment.OldGuidId.ToString(),
                UserID = comment.CreateBy,
                UserFullName = creator.DisplayUserName(),
                UserProfileLink = creator.GetUserProfilePageURL(),
                Inactive = comment.Inactive,
                IsEditPermissions = ProjectSecurity.CanEditComment(entity, comment),
                IsResponsePermissions = ProjectSecurity.CanCreateComment(entity),
                IsRead = true,
                UserAvatarPath = creator.GetBigPhotoURL(),
                UserPost = creator.Title,
                CommentList = new List<CommentInfo>()
            };

            if (allComments != null)
                foreach (var com in allComments.Where(com => com.Parent == comment.OldGuidId))
                {
                    oCommentInfo.CommentList.Add(GetCommentInfo(allComments, com, entity));
                }

            return oCommentInfo;
        }

        #endregion
    }
}