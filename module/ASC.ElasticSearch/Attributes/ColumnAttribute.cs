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

namespace ASC.ElasticSearch
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; private set; }

        public ColumnTypeEnum ColumnType { get; private set; }

        public int Order { get; private set; }

        public Analyzer Analyzer { get; set; }

        public Filter Filter { get; set; }

        public CharFilter CharFilter { get; set; }

        public ColumnAttribute(string columnName, int order, ColumnTypeEnum columnType = ColumnTypeEnum.Content, Filter filter = Filter.lowercase, CharFilter charFilter = CharFilter.io)
        {
            ColumnName = columnName;
            ColumnType = columnType;
            Order = order;
            Analyzer = Analyzer.whitespace;
            Filter = filter;
            CharFilter = charFilter;

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnIdAttribute : ColumnAttribute
    {
        public ColumnIdAttribute(string columnName)
            : base(columnName, -3, ColumnTypeEnum.Id)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnTenantIdAttribute : ColumnAttribute
    {
        public ColumnTenantIdAttribute(string columnName)
            : base(columnName, -2, ColumnTypeEnum.TenantId)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnLastModifiedAttribute : ColumnAttribute
    {
        public ColumnLastModifiedAttribute(string columnName)
            : base(columnName, -1, ColumnTypeEnum.LastModified)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnConditionAttribute : ColumnAttribute
    {
        internal object Value { get; private set; }
        public ColumnConditionAttribute(string columnName, int order, object value = null)
            : base(columnName, order, ColumnTypeEnum.Condition)
        {
            Value = value;
        }

        public ColumnConditionAttribute(string columnName, int order, params object[] value)
            : base(columnName, order, ColumnTypeEnum.Condition)
        {
            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ColumnMeta : ColumnAttribute
    {
        public ColumnMeta(string columnName, int order)
            : base(columnName, order, ColumnTypeEnum.Meta)
        {
        }
    }

    public enum ColumnTypeEnum
    {
        Id,
        TenantId,
        LastModified,
        Content,
        Condition,
        Meta
    }

    public enum Analyzer
    {
        standard,
        whitespace,
        uax_url_email
    }

    [Flags]
    public enum CharFilter
    {
        io,
        html
    }

    [Flags]
    public enum Filter
    {
        lowercase
    }
}
