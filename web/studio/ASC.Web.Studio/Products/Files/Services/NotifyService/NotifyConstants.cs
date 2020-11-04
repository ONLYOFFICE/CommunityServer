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


using ASC.Notify.Model;

namespace ASC.Web.Files.Services.NotifyService
{
    public static class NotifyConstants
    {
        #region Events

        public static readonly INotifyAction Event_DocuSignComplete = new NotifyAction("DocuSignComplete", "docusign complete");
        public static readonly INotifyAction Event_DocuSignStatus = new NotifyAction("DocuSignStatus", "docusign status");
        public static readonly INotifyAction Event_MailMergeEnd = new NotifyAction("MailMergeEnd", "mail merge end");
        public static readonly INotifyAction Event_ShareDocument = new NotifyAction("ShareDocument", "share document");
        public static readonly INotifyAction Event_ShareFolder = new NotifyAction("ShareFolder", "share folder");
        public static readonly INotifyAction Event_EditorMentions = new NotifyAction("EditorMentions", "editor mentions");

        #endregion

        #region  Tags

        public static readonly string Tag_DocumentTitle = "DocumentTitle";
        public static readonly string Tag_DocumentUrl = "DocumentURL";
        public static readonly string Tag_AccessRights = "AccessRights";
        public static readonly string Tag_Message = "Message";
        public static readonly string Tag_MailsCount = "MailsCount";

        #endregion
    }
}