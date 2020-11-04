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
                new TableInfo("calendar_event_history", "tenant"),
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
                new RelationInfo("calendar_calendars", "id", "calendar_event_history", "calendar_id"),
                new RelationInfo("calendar_events", "id", "calendar_event_history", "event_id"),
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

            if (table.Name == "calendar_event_history")
                return string.Format(
                    "inner join calendar_calendars as t1 on t1.id = t.calendar_id inner join calendar_events as t2 on t2.id = t.event_id where t1.tenant = {0} and t2.tenant = {0}",
                    tenantId);

            return base.GetSelectCommandConditionText(tenantId, table);
        }
    }
}
