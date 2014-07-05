/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Bookmarking.Business.Permissions;
using ASC.Bookmarking.Pojo;
using ASC.Web.Core.Users;
using ASC.Web.Studio.UserControls.Common.Comments;
using ASC.Web.UserControls.Bookmarking.Common.Presentation;

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
                    UserAvatar = BookmarkingServiceHelper.GetHTMLUserAvatar(userID),
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