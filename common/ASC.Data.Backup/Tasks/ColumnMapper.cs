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

namespace ASC.Data.Backup.Tasks
{
    public class ColumnMapper
    {
        private readonly Dictionary<string, object> mappings = new Dictionary<string, object>();
        private readonly Dictionary<string, object> newMappings = new Dictionary<string, object>(); 
        private readonly DateTime now = DateTime.UtcNow;

        public int GetTenantMapping()
        {
            var mapping = GetMapping("tenants_tenants", "id");
            return mapping != null ? Convert.ToInt32(mapping) : -1;
        }

        private DateTime GetTenantCreationDate()
        {
            var mappingKey = GetMappingKey("tenants_tenants", "creationdatetime");
            if (HasMapping(mappingKey))
            {
                var mapping = (MappingWithCondition)GetMappingInternal(mappingKey);
                return mapping != null ? Convert.ToDateTime(mapping.OldValue) : DateTime.MinValue;
            }
            return DateTime.MinValue;
        }

        public string GetUserMapping(string oldValue)
        {
            var mapping = GetMapping("core_user", "id", oldValue);
            return mapping != null ? Convert.ToString(mapping) : null;
        }

        public void SetDateMapping(string tableName, KeyValuePair<string, bool> column, object oldValue)
        {
            if (!column.Value)
            {
                SetMapping(tableName, column.Key, oldValue, now);
                return;
            }

            var newValue = Convert.ToDateTime(oldValue);
            var tenantCreationDate = GetTenantCreationDate();
            if (tenantCreationDate != DateTime.MinValue && newValue > DateTime.MinValue.AddDays(1) && newValue < DateTime.MaxValue.AddDays(-1))
            {
                newValue = newValue.AddDays(now.Subtract(tenantCreationDate).Days);
            }

            SetMapping(tableName, column.Key, oldValue, newValue);
        }

        public void SetMapping(string tableName, string columnName, object oldValue, object newValue)
        {
            if (tableName == "tenants_tenants")
            {
                var mapping = new MappingWithCondition { NewValue = newValue, OldValue = oldValue};
                AddMappingInternal(GetMappingKey(tableName, columnName), mapping);

            }
            AddMappingInternal(GetMappingKey(tableName, columnName, oldValue), newValue);
        }

        public object GetMapping(string tableName, string columnName)
        {
            var mappingKey = GetMappingKey(tableName, columnName);
            return HasMapping(mappingKey) ? ((MappingWithCondition)GetMappingInternal(mappingKey)).NewValue : null;
        }

        public object GetMapping(string tableName, string columnName, object oldValue)
        {
            var mappingKey = GetMappingKey(tableName, columnName, oldValue);
            if (HasMapping(mappingKey))
            {
                return GetMappingInternal(mappingKey);
            }
            mappingKey = GetMappingKey(tableName, columnName);
            if (HasMapping(mappingKey))
            {
                var mapping = (MappingWithCondition)GetMappingInternal(mappingKey);
                return mapping.NewValue;
            }
            return null;
        }

        public void Commit()
        {
            foreach (var mapping in newMappings)
            {
                mappings[mapping.Key] = mapping.Value;
            }
            newMappings.Clear();
        }

        public void Rollback()
        {
            newMappings.Clear();
        }

        private void AddMappingInternal(string key, object value)
        {
            newMappings[key] = value;
        }

        private object GetMappingInternal(string key)
        {
            return newMappings.ContainsKey(key) ? newMappings[key] : mappings[key];
        }

        private bool HasMapping(string key)
        {
            return newMappings.ContainsKey(key) || mappings.ContainsKey(key);
        }

        private static string GetMappingKey(string tableName, string columnName)
        {
            return string.Format("t:{0};c:{1}", tableName, columnName).ToLowerInvariant();
        }

        private static string GetMappingKey(string tableName, string columnName, object oldValue)
        {
            return string.Format("{0};v:{1}", GetMappingKey(tableName, columnName), oldValue).ToLowerInvariant();
        }

        private class MappingWithCondition
        {
            public object NewValue { get; set; }
            public object OldValue { get; set; }
        }
    }
}
