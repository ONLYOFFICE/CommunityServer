/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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
