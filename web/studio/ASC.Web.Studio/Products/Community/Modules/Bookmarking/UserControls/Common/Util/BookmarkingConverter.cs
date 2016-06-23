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
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
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

            CommentInfo c = new CommentInfo
                {
                    CommentID = comment.ID.ToString(),
                    UserID = userID,
                    TimeStamp = comment.Datetime,
                    TimeStampStr = comment.Datetime.Ago(),
                    Inactive = comment.Inactive,
                    CommentBody = comment.Content,
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