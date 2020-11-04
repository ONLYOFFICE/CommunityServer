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


#region Usings

using ASC.Bookmarking.Common;
using ASC.Bookmarking.Pojo;
using ASC.Web.Community.Product;

#endregion

namespace ASC.Bookmarking.Business.Permissions
{
    public static class BookmarkingPermissionsCheck
    {
        public static bool PermissionCheckCreateBookmark()
        {
            return CommunitySecurity.CheckPermissions(BookmarkingBusinessConstants.BookmarkCreateAction);
        }

        public static bool PermissionCheckAddToFavourite()
        {
            return CommunitySecurity.CheckPermissions(BookmarkingBusinessConstants.BookmarkAddToFavouriteAction);
        }

        public static bool PermissionCheckRemoveFromFavourite(UserBookmark b)
        {
            return CommunitySecurity.CheckPermissions(new BookmarkPermissionSecurityObject(b.UserID), BookmarkingBusinessConstants.BookmarkRemoveFromFavouriteAction);
        }

        public static bool PermissionCheckCreateComment()
        {
            return CommunitySecurity.CheckPermissions(BookmarkingBusinessConstants.BookmarkCreateCommentAction);
        }

        public static bool PermissionCheckEditComment()
        {
            return CommunitySecurity.CheckPermissions(BookmarkingBusinessConstants.BookmarkEditCommentAction);
        }

        public static bool PermissionCheckEditComment(Comment c)
        {
            return CommunitySecurity.CheckPermissions(new BookmarkPermissionSecurityObject(c.UserID, c.ID), BookmarkingBusinessConstants.BookmarkEditCommentAction);
        }
    }
}
