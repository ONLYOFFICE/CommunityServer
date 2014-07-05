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

        public static readonly INotifyAction Event_ImportCompleted = new NotifyAction("ImportCompleted", "import is completed");
      
        public static readonly INotifyAction Event_CreateNewContact = new NotifyAction("CreateNewContact", "create new contact");
        
        public static readonly string Tag_AdditionalData = "AdditionalData";

        public static readonly string Tag_EntityID = "EntityID";

        public static readonly string Tag_EntityTitle = "EntityTitle";

        public static readonly string Tag_EntityRelativeURL = "EntityRelativeURL";

        public static readonly string Tag_EntityListRelativeURL = "EntityListRelativeURL";

        public static readonly string Tag_EntityListTitle = "EntityListTitle";

    }
}