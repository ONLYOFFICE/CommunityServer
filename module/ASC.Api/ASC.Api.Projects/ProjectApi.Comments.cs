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

        ///<summary>
        ///Returns the information about the comment with the ID specified in the request
        ///</summary>
        ///<short>
        ///Get comment
        ///</short>
        ///<category>Comments</category>
        ///<param name="commentid">Comment ID</param>
        ///<returns>Comment</returns>        
        /// <exception cref="ItemNotFoundException"></exception>
        [Read(@"comment/{commentid}")]
        public CommentWrapper GetComment(Guid commentid)
        {
            var comment = EngineFactory.CommentEngine.GetByID(commentid).NotFoundIfNull();
            var entity = EngineFactory.CommentEngine.GetEntityByTargetUniqId(comment).NotFoundIfNull();

            return new CommentWrapper(comment, entity);
        }

        /////<summary>
        /////Updates the seleted comment using the comment text specified in the request
        /////</summary>
        /////<short>
        /////Update comment
        /////</short>
        ///// <category>Comments</category>
        /////<param name="commentid">comment ID</param>
        /////<param name="content">comment text</param>
        /////<returns>Comment</returns>
        ///// <exception cref="ItemNotFoundException"></exception>
        ///// <example>
        ///// <![CDATA[
        ///// Sending data in application/json:
        ///// 
        ///// {
        /////     text:"My comment text",
        /////     
        ///// }
        ///// 
        ///// Sending data in application/x-www-form-urlencoded
        ///// content=My%20comment%20text
        ///// ]]>
        ///// </example>
        //[Update(@"comment/{commentid}")]
        //public CommentWrapper UpdateComments(Guid commentid, string content)
        //{
        //    var comment = EngineFactory.CommentEngine.GetByID(commentid).NotFoundIfNull();
        //    comment.Content = Update.IfNotEquals(comment.Content, content);

        //    string type;
        //    comment = SaveComment(comment, out type);

        //    return new CommentWrapper(comment);
        //}

        ///<summary>
        ///Get preview
        ///</summary>
        ///<short>
        ///Get preview
        ///</short>
        /// <category>Comments</category>
        ///<param name="htmltext">html to create preview</param>
        ///<param name="commentid">guid of editing comment or empty string if comment is new</param>
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
        ///Remove comment with the id specified in the request
        /// </summary>
        /// <short>Remove comment</short>
        /// <section>Comments</section>
        /// <param name="commentid">Comment ID</param>
        /// <returns>Comment id</returns>
        /// <category>Comments</category>
        [Delete("comment/{commentid}")]
        public string RemoveProjectComment(string commentid)
        {
            var commentEngine = EngineFactory.CommentEngine;

            var comment = commentEngine.GetByID(new Guid(commentid));
            comment.Inactive = true;

            var entity = commentEngine.GetEntityByTargetUniqId(comment);
            if (entity == null) return "";

            ProjectSecurity.DemandEditComment(entity.Project, comment);

            commentEngine.SaveOrUpdate(comment);
            MessageService.Send(Request, MessageAction.TaskCommentDeleted, entity.Project.Title, entity.Title);

            return commentid;
        }

        /// <category>Comments</category>
        [Create("comment")]
        public CommentInfo AddProjectComment(string parentcommentid, int entityid, string content, string type)
        {
            if (string.IsNullOrEmpty(type) || !(new List<string> { "message", "task" }).Contains(type.ToLower()))
                throw new ArgumentException();

            var comment = type.ToLower().Equals("message")
                ? new Comment {Content = content, TargetUniqID = ProjectEntity.BuildUniqId<Message>(entityid)}
                : new Comment {Content = content, TargetUniqID = ProjectEntity.BuildUniqId<Task>(entityid)};
            

            if (!string.IsNullOrEmpty(parentcommentid))
                comment.Parent = new Guid(parentcommentid);

            var commentEngine = EngineFactory.CommentEngine;
            var entity = commentEngine.GetEntityByTargetUniqId(comment);
            if (entity == null) throw new Exception("Access denied.");
            ProjectSecurity.DemandCreateComment(entity);

            comment = commentEngine.SaveOrUpdateComment(entity, comment);
            MessageService.Send(Request, MessageAction.TaskCommentCreated, entity.Project.Title, entity.Title);

            return GetCommentInfo(null, comment, entity);
        }

        /// <category>Comments</category>
        [Update("comment/{commentid}")]
        public string UpdateComment(string commentid, string content)
        {
            var commentEngine = EngineFactory.CommentEngine;
            var comment = commentEngine.GetByID(new Guid(commentid));
            comment.Content = content;

            var entity = commentEngine.GetEntityByTargetUniqId(comment);
            if (entity == null) throw new Exception("Access denied.");

            ProjectSecurity.DemandEditComment(entity.Project, comment);

            commentEngine.SaveOrUpdateComment(entity, comment);
            MessageService.Send(Request, MessageAction.TaskCommentUpdated, entity.Project.Title, entity.Title);

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

        private Comment SaveComment(Comment comment, out string type)
        {
            var entity = EngineFactory.CommentEngine.GetEntityByTargetUniqId(comment).NotFoundIfNull();
            type = comment.TargetType;
            return EngineFactory.CommentEngine.SaveOrUpdateComment(entity, comment);
        }

        #endregion
    }
}