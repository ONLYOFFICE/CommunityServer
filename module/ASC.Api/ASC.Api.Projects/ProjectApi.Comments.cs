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