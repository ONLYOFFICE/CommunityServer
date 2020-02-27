/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

        public ColumnAttribute(string columnName, int order, ColumnTypeEnum columnType = ColumnTypeEnum.Content, Filter filter = Filter.lowercase,  CharFilter charFilter = CharFilter.io)
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
