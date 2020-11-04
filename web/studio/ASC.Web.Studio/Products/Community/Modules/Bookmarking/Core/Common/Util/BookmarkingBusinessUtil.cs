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
using ASC.Bookmarking.Pojo;
using System.Web;
using ASC.Core;
using ASC.Core.Users;

namespace ASC.Bookmarking.Common.Util
{
    public static class BookmarkingBusinessUtil
    {
        public static string GenerateBookmarkInfoUrl(Bookmark b)
        {
            return Business.BookmarkingService.ModifyBookmarkUrl(b);
        }

        public static string GenerateBookmarksUrl(Bookmark b)
        {
            return VirtualPathUtility.ToAbsolute(BookmarkingBusinessConstants.BookmarkingBasePath + "/Default.aspx");
        }

        public static string RenderProfileLink(Guid userID)
        {
            return CoreContext.UserManager.GetUsers(userID).RenderCustomProfileLink("describe-text", "link gray");
        }
    }
}