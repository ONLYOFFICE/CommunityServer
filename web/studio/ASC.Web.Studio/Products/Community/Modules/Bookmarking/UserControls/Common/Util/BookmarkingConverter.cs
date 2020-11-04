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
using System.Collections.Generic;
using System.Linq;
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.Studio.Utility.HtmlUtility;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;
using ASC.Web.Studio.Utility;

namespace ASC.Web.UserControls.Bookmarking.Common.Util
{
    public static class BookmarkingConverter
    {
        public static IList<CommentInfo> ConvertCommentList(IList<Comment> commentList)
        {
            var result = new List<CommentInfo>();
            foreach (var comment in commentList)
            {
                var parentID = Guid.Empty;
                try
                {
                    parentID = new Guid(comment.Parent);
                }
                catch
                {
                }
                if (Guid.Empty.Equals(parentID))
                {
                    var c = ConvertComment(comment, commentList);
                    result.Add(c);
                }
            }
            return result;
        }

        public static CommentInfo ConvertComment(Comment comment, IList<Comment> commentList)
        {
            var userID = comment.UserID;

            var c = new CommentInfo
                {
                    CommentID = comment.ID.ToString(),
                    UserID = userID,
                    TimeStamp = comment.Datetime,
                    TimeStampStr = comment.Datetime.Ago(),
                    Inactive = comment.Inactive,
                    CommentBody = HtmlUtility.GetFull(comment.Content),
                    UserFullName = DisplayUserSettings.GetFullUserName(userID),
                    UserProfileLink = CommonLinkUtility.GetUserProfile(userID),
                    UserAvatarPath = UserPhotoManager.GetBigPhotoURL(userID),
                    IsEditPermissions = BookmarkingPermissionsCheck.PermissionCheckEditComment(comment),
                    IsResponsePermissions = BookmarkingPermissionsCheck.PermissionCheckCreateComment(),
                    UserPost = BookmarkingServiceHelper.GetUserInfo(userID).Title
                };

            var commentsList = new List<CommentInfo>();

            var childComments = GetChildComments(comment, commentList);
            if (childComments != null)
            {
                foreach (var item in childComments)
                {
                    commentsList.Add(ConvertComment(item, commentList));
                }
            }
            c.CommentList = commentsList;
            return c;
        }

        private static IList<Comment> GetChildComments(Comment c, IList<Comment> comments)
        {
            var commentID = c.ID.ToString();
            return comments.Where(comment => commentID.Equals(comment.Parent)).ToList();
        }

        public static string GetDateAsString(DateTime date)
        {
            try
            {
                return date.ToShortTimeString() + "&nbsp;&nbsp;&nbsp;" + date.ToShortDateString();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}