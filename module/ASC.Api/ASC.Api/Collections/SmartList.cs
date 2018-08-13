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


#region usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using ASC.Api.Interfaces;

#endregion

namespace ASC.Api.Collections
{
    public interface ISmartList : IList
    {
        long StartIndex { get; set; }
        long TakeCount { get; set; }
        string SortBy { get; set; }
        bool IsDescending { get; set; }
        string FilterBy { get; set; }
        string FilterOp { get; set; }
        string FilterType { get; set; }
        string[] FilterValue { get; set; }
        DateTime UpdatedSince { get; set; }
        long TotalCount { get; }
        ICollection Transform(bool getTotalCount);

        void AddRange(IEnumerable items);
    }

    [CollectionDataContract(Name = "list", Namespace = "", ItemName = "entry")]
    public class SmartList<T> : IList<T>, ISmartList, ICloneable
    {
        private readonly Dictionary<string, IEnumerable<PropertyInfo>> _props = new Dictionary<string, IEnumerable<PropertyInfo>>();
        private ItemList<T> _innerList = new ItemList<T>();
        protected IEnumerable<T> Operations;

        public SmartList()
        {
            //TODO: Do all filtering through generated proxies
        }

        public SmartList(IEnumerable items)
        {
            _innerList.AddRange(items.Cast<T>());
        }

        public SmartList(IEnumerable<T> items)
        {
            _innerList.AddRange(items);
        }

        public IEnumerable<string> SortFields
        {
            get { return !string.IsNullOrEmpty(SortBy) ? SortBy.Split(',').Select(x => x.ToLowerInvariant()) : null; }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new SmartList<T>(_innerList)
                       {
                           FilterBy = FilterBy,
                           FilterOp = FilterOp,
                           FilterValue = FilterValue,
                           IsDescending = IsDescending,
                           SortBy = SortBy,
                           StartIndex = StartIndex,
                           TakeCount = TakeCount,
                           FilterType = FilterType,
                           UpdatedSince = UpdatedSince
                       };
        }

        #endregion

        #region IList<T> Members

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            _innerList.Add(item);
        }

        public int Add(object value)
        {
            Add((T)value);
            return 0;
        }

        public void AddRange(IEnumerable<T> items)
        {
            _innerList.AddRange(items);
        }

        public void AddRange(IEnumerable items)
        {
            AddRange(items.Cast<T>());
        }


        public bool Contains(object value)
        {
            return Contains((T)value);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public int IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        public void Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        public void Remove(object value)
        {
            Remove((T)value);
        }

        public bool Contains(T item)
        {
            return _innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _innerList.Remove(item);
        }

        public void CopyTo(Array array, int index)
        {
            _innerList.CopyTo(array.OfType<T>().ToArray(), index);
        }

        public virtual int Count
        {
            get { return _innerList.Count; }
        }

        private readonly object _synchRoot = new object();

        public object SyncRoot
        {
            get { return _synchRoot; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public int IndexOf(T item)
        {
            return _innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _innerList.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _innerList.RemoveAt(index);
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { _innerList[index] = (T)value; }
        }

        public T this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }

        #endregion

        #region ISmartList Members

        public bool IsDescending { get; set; }
        public string FilterBy { get; set; }
        public string FilterOp { get; set; }

        public string FilterType
        { get; set; }

        public string[] FilterValue { get; set; }

        public DateTime UpdatedSince
        {
            get;
            set;
        }

        public long TotalCount { get; protected set; }

        public virtual ICollection Transform(bool getTotalCount)
        {
            var result = (SmartList<T>)Clone();
            result.OperationSet();
            result.FilterByUpdated();
            result.Filter();
            result.FilterByType();
            result.SortByField();

            if (getTotalCount)
            {
                //TODO: Performance impact
                Operations = Operations.ToList(); //Materialized filter
                TotalCount = Operations.Count();
            }

            result.SetStartIndex();
            result.SettakeCount();
            result.OperationFinish();
            return result;
        }


        public long StartIndex { get; set; }
        public long TakeCount { get; set; }
        public string SortBy { get; set; }

        #endregion

        private void OperationFinish()
        {
            _innerList = new ItemList<T>(Operations);
        }

        private void OperationSet()
        {
            Operations = _innerList;
        }

        private void SettakeCount()
        {
            if (TakeCount > 0)
            {
                Operations = Operations.Take((int)TakeCount);
            }
        }

        private void SetStartIndex()
        {
            if (StartIndex > 0)
            {
                Operations = Operations.Skip((int) Math.Min(StartIndex,TotalCount));
            }
        }

        protected void FilterByType()
        {
            if (string.IsNullOrEmpty(FilterType)) return;
            Operations = Operations.Where(x => SatisfyType(x, FilterType));
        }

        private readonly Dictionary<Type, string> _dataContractsMap = new Dictionary<Type, string>();

        protected bool SatisfyType(T obj, string filterType)
        {
            if (Equals(obj, default(T))) return false;

            //Try get dataContract type name
            var type = obj.GetType();
            if (!_dataContractsMap.ContainsKey(type))
            {
                var attrs = type.GetCustomAttributes(typeof(DataContractAttribute), true);
                if (attrs.Length > 0)
                {
                    _dataContractsMap.Add(type, ((DataContractAttribute)attrs[0]).Name);
                }
                else
                {
                    _dataContractsMap.Add(type, type.Name.ToLowerInvariant());
                }
            }
            return filterType == _dataContractsMap[type];
        }

        protected void FilterByUpdated()
        {
            if (UpdatedSince != DateTime.MinValue)
            {
                Operations = Operations.Where(x => SatisfyCreated(x, UpdatedSince));
            }
        }

        private bool SatisfyCreated(T arg, DateTime updatedSince)
        {
            var prop = (GetPropertyInfo("updated") ?? GetPropertyInfo("created")).FirstOrDefault();
            if (prop != null)
            {
                if (prop.PropertyType == typeof(DateTime))
                    return updatedSince < (DateTime)prop.GetValue(arg, null);
                if (prop.PropertyType.GetInterface("ASC.Api.Interfaces.IApiDateTime", false) != null)
                {
                    var apiDate = (prop.GetValue(arg, null) as IApiDateTime);
                    if (apiDate != null)
                    {
                        return updatedSince < apiDate.UtcTime;
                    }
                }
            }
            return false;
        }

        protected void Filter()
        {
            if (!ShouldFilter()) return;

            var prop = GetPropertyInfo(FilterBy.ToLowerInvariant());
            if (prop == null) return;
            var op = GetFilterOperation();
            Operations = Operations.Where(x => SatisfyFilter(x, prop, FilterValue, op));
        }

        protected bool ShouldFilter()
        {
            return !string.IsNullOrEmpty(FilterBy) && FilterValue != null && FilterValue.All(x=>!string.IsNullOrEmpty(x));
        }

        private static bool SatisfyFilter(T obj, IEnumerable<PropertyInfo> propertyChain, IEnumerable<string> filterValues, FilterOperation op)
        {
            var result = false;
            if (propertyChain != null)
            {
                //Find last value
                object objvalue = obj;
                foreach (var propertyInfo in propertyChain)
                {
                    if (propertyInfo != null && !Equals(objvalue, default(T)) && propertyInfo.DeclaringType != null && propertyInfo.DeclaringType.IsInstanceOfType(objvalue))
                        objvalue = propertyInfo.GetValue(objvalue, null);
                }

                if (objvalue != null && !ReferenceEquals(objvalue, obj))
                {
                    string value = Convert.ToString(objvalue);
                    foreach (var filterValue in filterValues)
                    {
                        switch (op)
                        {
                            case FilterOperation.Contains:
                                result = value.IndexOf(filterValue, StringComparison.OrdinalIgnoreCase) != -1;
                                break;
                            case FilterOperation.Equals:
                                result = value.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                                break;
                            case FilterOperation.StartsWith:
                                result = value.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase);
                                break;
                            case FilterOperation.EndsWith:
                                result = value.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase);
                                break;
                            case FilterOperation.Present:
                                result = value.Contains(filterValue);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("op");
                        }
                        if (result)
                            break;//Break loop
                    }
                }
            }
            return result;
        }

        protected FilterOperation GetFilterOperation()
        {
            FilterOperation operation = FilterOperation.Equals;
            if (!string.IsNullOrEmpty(FilterOp))
            {
                try
                {
                    operation = (FilterOperation)Enum.Parse(typeof(FilterOperation), FilterOp, true);
                }
                catch (Exception)
                {
                    
                }
            }
            return operation;
        }

        protected void SortByField()
        {
            if (SortFields != null && SortFields.Any())
            {
                Operations = OrderByFields(SortFields);
            }
        }

        private IEnumerable<T> OrderByFields(IEnumerable<string> sortFields)
        {
            IOrderedEnumerable<T> result = IsDescending
                                               ? Operations.OrderByDescending(x => GetSortField(x, sortFields.First()))
                                               : Operations.OrderBy(x => GetSortField(x, sortFields.First()));
            return sortFields
                .Skip(1)
                .Aggregate(result,
                           (current, sortField) =>
                           IsDescending
                               ? current.ThenByDescending(x => GetSortField(x, sortField))
                               : current.ThenBy(x => GetSortField(x, sortField)));
        }

        private object GetSortField(T instance, string sortProp)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(sortProp).FirstOrDefault();
            return propertyInfo != null ? propertyInfo.GetValue(instance, null) : null;
        }

        protected IEnumerable<PropertyInfo> GetPropertyInfo(string sortProp)
        {
            IEnumerable<PropertyInfo> propertyInfo;
            if (!_props.TryGetValue(sortProp, out propertyInfo))
            {
                var props = new List<PropertyInfo>();
                Type currentType = typeof(T);
                foreach (var prop in sortProp.Split('.'))
                {
                    var propInfo = currentType.GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(x => x.Name.Equals(prop, StringComparison.OrdinalIgnoreCase));
                    if (propInfo != null)
                    {
                        currentType = propInfo.GetGetMethod().ReturnType;
                        props.Add(propInfo);
                    }
                    else
                    {
                        break;
                    }
                }
                _props.Add(sortProp, props);
                propertyInfo = props;
            }
            return propertyInfo;
        }

    }

    public enum FilterOperation
    {
        Contains,
        Equals,
        EndsWith,
        StartsWith,
        Present
    }
}