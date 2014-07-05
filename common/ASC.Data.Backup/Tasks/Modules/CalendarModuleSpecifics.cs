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
using ASC.Data.Backup.Tasks.Data;

namespace ASC.Data.Backup.Tasks.Modules
{
    internal class CalendarModuleSpecifics : ModuleSpecificsBase
    {
        private readonly TableInfo[] _tables = new[]
            {
                new TableInfo("calendar_calendars", "tenant", "id") {UserIDColumns = new[] {"owner_id"}},
                new TableInfo("calendar_calendar_item"),
                new TableInfo("calendar_calendar_user") {UserIDColumns = new[] {"user_id"}},
                new TableInfo("calendar_events", "tenant", "id")
                    {
                        UserIDColumns = new[] {"owner_id"},
                        DateColumns = new Dictionary<string, bool> {{"start_date", true}, {"end_date", true}}
                    },
                new TableInfo("calendar_event_item"),
                new TableInfo("calendar_event_user") {UserIDColumns = new[] {"user_id"}},
                new TableInfo("calendar_notifications", "tenant")
                    {
                        UserIDColumns = new[] {"user_id"},
                        DateColumns = new Dictionary<string, bool> {{"notify_date", true}}
                    }
            };

        private readonly RelationInfo[] _tableRelations = new[]
            {
                new RelationInfo("calendar_calendars", "id", "calendar_calendar_item", "calendar_id"),
                new RelationInfo("calendar_calendars", "id", "calendar_calendar_user", "calendar_id"),
                new RelationInfo("calendar_calendars", "id", "calendar_events", "calendar_id"),
                new RelationInfo("calendar_events", "id", "calendar_event_item", "event_id"),
                new RelationInfo("calendar_events", "id", "calendar_event_user", "event_id"),
                new RelationInfo("calendar_events", "id", "calendar_notifications", "event_id"),
                new RelationInfo("core_user", "id", "calendar_calendar_item", "item_id", typeof(TenantsModuleSpecifics), 
                    x => Convert.ToInt32(x["is_group"]) == 0),
                new RelationInfo("core_group", "id", "calendar_calendar_item", "item_id", typeof(TenantsModuleSpecifics), 
                    x => Convert.ToInt32(x["is_group"]) == 1 && !Helpers.IsEmptyOrSystemGroup(Convert.ToString(x["item_id"]))),
                new RelationInfo("core_user", "id", "calendar_event_item", "item_id", typeof(TenantsModuleSpecifics), 
                    x => Convert.ToInt32(x["is_group"]) == 0),
                new RelationInfo("core_group", "id", "calendar_event_item", "item_id", typeof(TenantsModuleSpecifics), 
                    x => Convert.ToInt32(x["is_group"]) == 1 && !Helpers.IsEmptyOrSystemGroup(Convert.ToString(x["item_id"])))
            };

        public override ModuleName ModuleName
        {
            get { return ModuleName.Calendar; }
        }

        public override IEnumerable<TableInfo> Tables
        {
            get { return _tables; }
        }

        public override IEnumerable<RelationInfo> TableRelations
        {
            get { return _tableRelations; }
        }

        protected override string GetSelectCommandConditionText(int tenantId, TableInfo table)
        {
            if (table.Name == "calendar_calendar_item" || table.Name == "calendar_calendar_user")
                return "inner join calendar_calendars as t1 on t1.id = t.calendar_id where t1.tenant = " + tenantId;

            if (table.Name == "calendar_event_item" || table.Name == "calendar_event_user")
                return "inner join calendar_events as t1 on t1.id = t.event_id where t1.tenant = " + tenantId;

            return base.GetSelectCommandConditionText(tenantId, table);
        }
    }
}
