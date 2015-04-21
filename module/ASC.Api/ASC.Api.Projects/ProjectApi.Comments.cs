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
using ASC.Api.Attributes;
using ASC.Api.Exceptions;
using ASC.Api.Projects.Wrappers;
using ASC.Api.Utils;
using ASC.Projects.Core.Domain;

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
            return new CommentWrapper(EngineFactory.GetCommentEngine().GetByID(commentid).NotFoundIfNull());
        }

        ///<summary>
        ///Updates the seleted comment using the comment text specified in the request
        ///</summary>
        ///<short>
        ///Update comment
        ///</short>
        /// <category>Comments</category>
        ///<param name="commentid">comment ID</param>
        ///<param name="content">comment text</param>
        ///<returns>Comment</returns>
        /// <exception cref="ItemNotFoundException"></exception>
        /// <example>
        /// <![CDATA[
        /// Sending data in application/json:
        /// 
        /// {
        ///     text:"My comment text",
        ///     
        /// }
        /// 
        /// Sending data in application/x-www-form-urlencoded
        /// content=My%20comment%20text
        /// ]]>
        /// </example>
        [Update(@"comment/{commentid}")]
        public CommentWrapper UpdateComments(Guid commentid, string content)
        {
            var comment = EngineFactory.GetCommentEngine().GetByID(commentid).NotFoundIfNull();
            comment.Content = Update.IfNotEquals(comment.Content, content);

            string type;
            comment = SaveComment(comment, out type);

            return new CommentWrapper(comment);
        }

        ///<summary>
        ///Delete the comment with the ID specified in the request from the portal
        ///</summary>
        ///<short>
        ///Delete comment
        ///</short>
        /// <category>Comments</category>
        ///<param name="commentid">comment ID</param>
        /// <exception cref="ItemNotFoundException"></exception>
        [Delete(@"comment/{commentid}")]
        public CommentWrapper DeleteComments(Guid commentid)
        {
            var comment = EngineFactory.GetCommentEngine().GetByID(commentid).NotFoundIfNull();
            comment.Inactive = true;

            string type;
            comment = SaveComment(comment, out type);

            return new CommentWrapper(comment);
        }

        private Comment SaveComment(Comment comment, out string type)
        {
            var targetUniqID = comment.TargetUniqID.Split('_');
            var entityType = targetUniqID[0];
            var entityId = targetUniqID[1];

            type = null;
            switch (entityType)
            {
                case "Task":
                    var taskEngine = EngineFactory.GetTaskEngine();
                    var task = taskEngine.GetByID(Convert.ToInt32(entityId)).NotFoundIfNull();
                    comment = taskEngine.SaveOrUpdateComment(task, comment);
                    type = "Task";
                    break;

                case "Message":
                    var messageEngine = EngineFactory.GetMessageEngine();
                    var message = messageEngine.GetByID(Convert.ToInt32(entityId)).NotFoundIfNull();
                    comment = messageEngine.SaveOrUpdateComment(message, comment);
                    type = "Message";
                    break;
            }

            return comment;
        }

        #endregion
    }
}