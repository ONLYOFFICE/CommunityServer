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
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Web.Community.News.Resources;
using Action = ASC.Common.Security.Authorizing.Action;

namespace ASC.Web.Community.News
{
    public static class NewsConst
    {
        public static Guid ModuleId = new Guid("3CFD481B-46F2-4a4a-B55C-B8C0C9DEF02C");


        public static readonly Action Action_Add = new Action(new Guid("{2C6552B3-B2E0-4a00-B8FD-13C161E337B1}"), NewsResource.Action_Add_Name);
        public static readonly Action Action_Edit = new Action(new Guid("{14BE970F-7AF5-4590-8E81-EA32B5F7866D}"), NewsResource.Action_Edit_Name);
        public static readonly Action Action_Comment = new Action(new Guid("{FCAC42B8-9386-48eb-A938-D19B3C576912}"), NewsResource.Action_Comment_Name);

        public static INotifyAction NewFeed = new NotifyAction("new feed", "news added");
        public static INotifyAction NewComment = new NotifyAction("new feed comment", "new feed comment");

        public static string TagCaption = "Caption";
        public static string TagText = "Text";
        public static string TagDate = "Date";
        public static string TagURL = "URL";
        public static string TagUserName = "UserName";
        public static string TagUserUrl = "UserURL";
        public static string TagAnswers = "Answers";

        public static string TagFEED_TYPE = "FEED_TYPE";

    }
}