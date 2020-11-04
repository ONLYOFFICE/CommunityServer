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
using System.Web;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.UserControls.Wiki.Resources;

namespace ASC.Web.UserControls.Wiki
{
    public class Constants
    {
        public const string WikiCategoryKeyCaption = "Category";	
        public const string WikiInternalCategoriesKey = "Categories";
        public const string WikiInternalFilesKey = "Files";
        public const string WikiInternalHelpKey = "Help";
        public const string WikiInternalHomeKey = "Home";
        public const string WikiInternalIndexKey = "Index";
        public const string WikiInternalKeyCaption = "Internal";
        public const string WikiInternalNewPagesKey = "NewPages";
        public const string WikiInternalRecentlyKey = "Recently";

        public static INotifyAction NewPage = new NotifyAction("new wiki page", WikiResource.NotifyAction_NewPage);
        public static INotifyAction EditPage = new NotifyAction("edit wiki page", WikiResource.NotifyAction_ChangePage);
        public static INotifyAction AddPageToCat = new NotifyAction("add page to cat", WikiResource.NotifyAction_AddPageToCat);

        public static string TagPageName = "PageName";
        public static string TagURL = "URL";

        public static string TagUserName = "UserName";
        public static string TagUserURL = "UserURL";
        public static string TagDate = "Date";

        public static string TagPostPreview = "PagePreview";
        public static string TagCommentBody = "CommentBody";

        public static string TagChangePageType = "ChangeType";
        public static string TagCatName = "CategoryName";
    }
}