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


#region Import

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Notify;
using ASC.Notify.Model;
using ASC.Notify.Patterns;
using ASC.Notify.Recipients;
using ASC.Core.Tenants;

#endregion

namespace ASC.Web.CRM.Services.NotifyService
{
    public static class NotifyConstants
    {

        public static readonly INotifyAction Event_SetAccess = new NotifyAction("SetAccess", "set access for users");

        public static readonly INotifyAction Event_ResponsibleForTask = new NotifyAction("ResponsibleForTask", "responsible for task");

        public static readonly INotifyAction Event_TaskReminder = new NotifyAction("TaskReminder", "auto reminder about task");

        public static readonly INotifyAction Event_ResponsibleForOpportunity = new NotifyAction("ResponsibleForOpportunity", "responsible for opportunity");
       
        public static readonly INotifyAction Event_AddRelationshipEvent = new NotifyAction("AddRelationshipEvent", "add relationship event");

        public static readonly INotifyAction Event_ExportCompleted = new NotifyAction("ExportCompleted", "export is completed");

        public static readonly INotifyAction Event_ExportCompletedCustomMode = new NotifyAction("ExportCompletedCustomMode", "export is completed");

        public static readonly INotifyAction Event_ImportCompleted = new NotifyAction("ImportCompleted", "import is completed");

        public static readonly INotifyAction Event_ImportCompletedCustomMode = new NotifyAction("ImportCompletedCustomMode", "import is completed");

        public static readonly INotifyAction Event_CreateNewContact = new NotifyAction("CreateNewContact", "create new contact");

        public static readonly string Tag_AdditionalData = "AdditionalData";

        public static readonly string Tag_EntityID = "EntityID";

        public static readonly string Tag_EntityTitle = "EntityTitle";

        public static readonly string Tag_EntityRelativeURL = "EntityRelativeURL";

        public static readonly string Tag_EntityListRelativeURL = "EntityListRelativeURL";

        public static readonly string Tag_EntityListTitle = "EntityListTitle";

    }
}