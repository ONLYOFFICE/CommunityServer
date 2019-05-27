/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ASC.ElasticSearch.Core;
using Nest;
using Newtonsoft.Json.Linq;

namespace ASC.ElasticSearch
{
    public abstract class Wrapper
    {
        protected internal abstract string Table { get; }

        protected internal virtual string IndexName
        {
            get { return Table; }
        }

        [ColumnId("id")]
        public virtual int Id { get; set; }

        [ColumnTenantId("tenant_id")]
        public virtual int TenantId { get; set; }

        [ColumnLastModified("last_modified_on"), Date]
        public virtual DateTime LastModifiedOn { get; set; }

        private List<PropertyInfo> orderedProperties;

        private List<PropertyInfo> OrderedProperties
        {
            get
            {
                return orderedProperties ?? (orderedProperties = GetType()
                    .GetProperties()
                    .Where(r =>
                    {
                        var column = r.GetCustomAttribute<ColumnAttribute>();
                        return column != null && !string.IsNullOrEmpty(column.ColumnName);
                    })
                    .OrderBy(r => r.GetCustomAttribute<ColumnAttribute>().Order)
                    .ToList());
            }
        }

        private List<ColumnAttribute> columns;

        private List<ColumnAttribute> Columns
        {
            get
            {
                return columns ?? (columns = OrderedProperties
                    .Select(r => r.GetCustomAttribute<ColumnAttribute>())
                    .ToList());
            }
        }

        internal Converter<object[], Wrapper> GetDataConverter(bool sub = false)
        {
            return data =>
            {
                var result = Activator.CreateInstance(GetType());

                var i = 0;
                var props = OrderedProperties;
                for (; i < props.Count; i++)
                {
                    if (data[i] == null) continue;
                    object newValue;
                    if (props[i].PropertyType == typeof(Guid))
                    {
                        newValue = Guid.Parse(data[i].ToString());
                    }
                    else if (props[i].PropertyType == typeof(bool))
                    {
                        try
                        {
                            newValue = Convert.ToBoolean(data[i]);
                        }
                        catch (Exception)
                        {
                            newValue = data[i] != null;
                        }
                    }
                    else
                    {
                        try
                        {
                            newValue = Convert.ChangeType(data[i], props[i].PropertyType);
                        }
                        catch (Exception)
                        {
                            newValue = Activator.CreateInstance(props[i].PropertyType);
                        }
                    }
                    props[i].SetValue(result, newValue);
                }

                var joins = GetType()
                    .GetProperties()
                    .Where(r => r.GetCustomAttribute<JoinAttribute>() != null)
                    .ToList();

                if (!joins.Any()) return (Wrapper) result;

                data = data.Skip(i).ToArray();

                foreach (var join in joins)
                {
                    Wrapper joinWrapper;

                    if (join.PropertyType.IsGenericType)
                    {
                        joinWrapper = Activator.CreateInstance(join.PropertyType.GenericTypeArguments[0]) as Wrapper;
                    }
                    else
                    {
                        joinWrapper = Activator.CreateInstance(join.PropertyType) as Wrapper;
                    }

                    if (joinWrapper == null) continue;

                    var joinAttr = join.GetCustomAttribute<JoinAttribute>();
                    var joinSub = sub || joinAttr.JoinType == JoinTypeEnum.Sub;
                    List<object[]> newArray;
                    if (joinSub && join.PropertyType.IsGenericType)
                    {
                        newArray = data[0] == null
                            ? new List<object[]>()
                            : JArray.Parse(data[0].ToString()).Select(r => ((JArray)r).Select(q => (object)q).ToArray()).ToList();
                    }
                    else
                    {
                        newArray = new List<object[]> {data};
                    }

                    var newArrayValue = newArray.ConvertAll(joinWrapper.GetDataConverter(joinSub));

                    object newValue;

                    if (joinSub && join.PropertyType.IsGenericType)
                    {
                        var list = (IList)Activator.CreateInstance(join.PropertyType);
                        foreach (var item in newArrayValue)
                        {
                            list.Add(item);
                        }

                        newValue = list;
                    }
                    else
                    {
                        newValue = Convert.ChangeType(newArrayValue.First(), join.PropertyType);
                    }

                    join.SetValue(result, newValue);

                    var skipCount = joinSub
                        ? 1
                        : joinWrapper.OrderedProperties.Count;

                    data = data.Skip(skipCount).ToArray();
                }

                return (Wrapper) result;
            };
        }

        internal string GetColumnName(ColumnTypeEnum columnType, string alias)
        {
            var column = Columns.FirstOrDefault(r => r.ColumnType == columnType);
            if (column == null || string.IsNullOrEmpty(column.ColumnName)) return null;
            return alias + "." + column.ColumnName;
        }

        internal bool TryGetColumnName(ColumnTypeEnum columnType, string alias, out string columnName)
        {
            columnName = GetColumnName(columnType, alias);
            return columnName != null;
        }

        internal bool TryGetColumnNames(List<ColumnTypeEnum> columnType, string alias, out List<string> columnName)
        {
            columnName = new List<string>();
            foreach (var cType in columnType)
            {
                var type = cType;
                var column = Columns.Where(r => r.ColumnType == type);
                columnName.AddRange(column.Select(r => alias + "." + r.ColumnName));
            }

            return columnName.Any();
        }

        internal string[] GetColumnNames(string alias)
        {
            return Columns
                .Select(r => alias + "." + r.ColumnName)
                .ToArray();
        }

        internal string[] GetContentProperties()
        {
            var result = OrderedProperties
                .Where(r => r.GetCustomAttribute<ColumnAttribute>().ColumnType == ColumnTypeEnum.Content)
                .Select(r => ToLowerCamelCase(r.Name))
                .ToList();

            if (this is WrapperWithDoc)
            {
                result.Add("document.attachment.content");
            }

            foreach (var join in GetJoins())
            {
                var joinType = join.Value.Name;
                var joinAttr = join.Value.GetCustomAttribute<JoinAttribute>().JoinType;
                result.AddRange(join.Key.GetContentProperties()
                    .Select(r => (joinAttr == JoinTypeEnum.Sub ? joinAttr + ":" : "") + ToLowerCamelCase(joinType) + "." + r));
            }

            return result.ToArray();
        }

        internal Dictionary<Wrapper, PropertyInfo> GetJoins()
        {
            var result = new Dictionary<Wrapper, PropertyInfo>();

            var joins = GetType()
                .GetProperties()
                .Where(r => r.GetCustomAttribute<JoinAttribute>() != null)
                .ToList();

            foreach (var join in joins)
            {
                Wrapper joinWrapper;
                if (join.PropertyType.IsGenericType)
                {
                    joinWrapper = Activator.CreateInstance(join.PropertyType.GenericTypeArguments[0]) as Wrapper;
                }
                else
                {
                    joinWrapper = Activator.CreateInstance(join.PropertyType) as Wrapper;
                }

                if (joinWrapper == null) continue;

                result.Add(joinWrapper, join);
            }

            return result;
        }

        internal Dictionary<string, object> GetConditions(string alias)
        {
            var result = new Dictionary<string, object>();

            var conditions = Columns.OfType<ColumnConditionAttribute>();

            foreach (var con in conditions.Where(r=> r.Value != null))
            {
                result.Add(alias + "." + con.ColumnName, con.Value);
            }

            return result;
        }

        internal string ToLowerCamelCase(string value)
        {
            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        internal Func<PropertiesDescriptor<T>, IPromise<IProperties>> GetProperties<T>() where T : Wrapper
        {
            var analyzers = GetAnalyzers();

            return p =>
            {
                foreach (var c in analyzers)
                {
                    var c1 = c;
                    string analyzer;

                    if (c.Value.CharFilter != CharFilter.io)
                    {
                        analyzer = c.Key;
                    }
                    else
                    {
                        analyzer = c1.Value.Analyzer + "custom";
                    }

                    p.Text(s => s.Name(c1.Key).Analyzer(analyzer));
                }
                if (this is WrapperWithDoc)
                {
                    p.Object<Document>(
                        r => r.Name("document").Properties(
                        q => q.Object<Attachment>(
                        a => a.Name("attachment").Properties(
                        w => w.Text(
                        t => t.Name("content").Analyzer("document"))))));
                }
                return p;
            };
        }

        internal Dictionary<string, ColumnAttribute> GetAnalyzers()
        {
            var result = new Dictionary<string, ColumnAttribute>();

            foreach (var prop in OrderedProperties)
            {
                var column = prop.GetCustomAttribute<ColumnAttribute>();

                if (column == null || column.ColumnType != ColumnTypeEnum.Content || column.Analyzer == Analyzer.standard) continue;

                result.Add(prop.Name.ToLowerInvariant()[0] + prop.Name.Substring(1), column);
            }

            return result;
        }

        internal Dictionary<Type, string> GetNested()
        {
            var result = new Dictionary<Type, string>();

            var joins = GetType().GetProperties().Where(r =>
            {
                var attr = r.GetCustomAttribute<JoinAttribute>();
                return attr != null;
            });

            foreach (var prop in joins)
            {
                result.Add(prop.PropertyType, prop.Name);
            }

            return result;
        }
    }
}