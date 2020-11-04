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
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CSharp;


namespace ASC.Api.Collections
{


    public abstract class ListFilter<T>
    {
        public virtual IEnumerable<T> FilterList(IEnumerable<T> items, bool sortDescending, FilterOperation operation, string[] filterValues)
        {
            return items;
        }

        protected bool Satisfy(FilterOperation operation, string[] filterValues, string fieldValue)
        {
            var result = false;
            if (fieldValue != null)
            {
                for (int index = 0; index < filterValues.Length; index++)
                {
                    var filterValue = filterValues[index];
                    if (operation == FilterOperation.Contains)
                    {
                        result = fieldValue.IndexOf(filterValue, StringComparison.OrdinalIgnoreCase) != -1;
                    }
                    else if (operation == FilterOperation.Equals)
                    {
                        result = fieldValue.Equals(filterValue, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (operation == FilterOperation.StartsWith)
                    {
                        result = fieldValue.StartsWith(filterValue, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (operation == FilterOperation.EndsWith)
                    {
                        result = fieldValue.EndsWith(filterValue, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (operation == FilterOperation.Present)
                    {
                        result = fieldValue.Contains(filterValue);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("operation");
                    }
                    if (result)
                        break; //Break loop
                }
            }
            return result;
        }
    }



    public class CompiledSmartList<T> : SmartList<T>
    {
        private IEnumerable<T> _items;

        public override int Count
        {
            get
            {
                return _items.Count();
            }
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public CompiledSmartList(IEnumerable<T> items)
        {
            _items = items;
        }

        public CompiledSmartList(IEnumerable items)
        {
            _items = items.Cast<T>();
        }

        public ListFilter<T> ForceFilter { get; set; }

        public override ICollection Transform(bool getTotalCount)
        {
            if (ShouldSort() || ShouldFilter())
            {
                //Create wrapper class
                _items = _items.Where(item => !Equals(item, default(T)));
                var filter = ForceFilter ?? Compile();
                if (filter != null)
                    _items = filter.FilterList(_items, IsDescending, GetFilterOperation(), FilterValue);
                else
                {
                    //Do program transform
                    Operations = _items;
                    FilterByUpdated();
                    Filter();
                    FilterByType();
                    SortByField();
                    _items = Operations;
                }
            }

            //Do sorting filtering
            if (!string.IsNullOrEmpty(FilterType))
            {
                _items = _items.Where(x => SatisfyType(x, FilterType));
            }

            if (getTotalCount)
            {
                //TODO: Performance impact
                //we need to know what we have filtered
                _items = _items.ToList(); //Materialized filter
                TotalCount = _items.Count();
            }


            if (StartIndex > 0)
            {
                _items = _items.Skip((int)StartIndex);
            }
            if (TakeCount > 0)
            {
                _items = _items.Take((int)TakeCount);
            }
            return new ItemList<T>(_items);//Materialize
        }

        private bool ShouldSort()
        {
            return (SortFields != null && SortFields.Any());
        }

        private string GetUniqueCode()
        {
            //Return code specific for this list for assembly cache
            return string.Format("f[{0}]s[{1}]",
                ShouldFilter() ? FilterBy : string.Empty,
                ShouldSort() ? string.Join(",", SortFields.ToArray()) : string.Empty
                ).ToLowerInvariant();
        }

        private static readonly Dictionary<string, ListFilter<T>> FiltersCache = new Dictionary<string, ListFilter<T>>();

        private ListFilter<T> Compile()
        {
            var uniqueCode = GetUniqueCode();
            ListFilter<T> resultFilter;

            if (!FiltersCache.TryGetValue(uniqueCode, out resultFilter))
            {
                var includeAssemblies = new HashSet<string>();

                var provider = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v3.5" } });

                var name = "Filter";
                if (FilterBy!=null)
                    name+="_"+Regex.Replace(FilterBy, @"\W", "");
                if (SortFields!=null)
                    name+="_Sort_" + Regex.Replace(SortFields.Aggregate((y, x) => y), @"\W", "");
                var typeName = provider.CreateValidIdentifier(name);


                var targetTypeName = typeof(T).FullName.Replace('+', '.');
                var code = new StringBuilder();
                code.AppendLine("using System;");
                code.AppendLine("using System.Collections;");
                code.AppendLine("using System.Collections.Generic;");
                code.AppendLine("using System.Linq;");
                code.AppendLine("using ASC.Api.Collections;");
                code.AppendLine("using System.Globalization;");

                code.AppendFormat("public class {1} : ListFilter<{0}> {{\r\n", targetTypeName, typeName);
                code.AppendFormat(
                    "    public override IEnumerable<{0}> FilterList(IEnumerable<{0}> items, bool sortDescending, FilterOperation operation, string[] filterValues){{\r\n",
                    targetTypeName);
                //Do a needed operations

                //TODO: Do a null checks!
                if (ShouldFilter())
                {
                    try
                    {
                        var filters = FilterBy.Split(',');
                        var filterChecks = new List<string>();
                        foreach (var filter in filters)
                        {
                            var propInfo = GetPropertyInfo(filter).Where(x => x != null).ToList();
                            foreach (var propertyInfo in propInfo)
                            {
                                includeAssemblies.Add(propertyInfo.PropertyType.Assembly.Location);
                            }

                            var byProperty = GetPropertyPath(propInfo);

                            var nullCheck = GetNullCheck("item", propInfo);


                            filterChecks.Add(string.Format("({1}Satisfy(operation, filterValues, item.{0}{2}))",
                                byProperty,
                                string.IsNullOrEmpty(nullCheck) ? "" : (nullCheck + " && "),
                                GetPropertyToString(propInfo.Last())));

                        }
                        code.AppendFormat(
                            "items = items.Where(item =>{0});\r\n", string.Join(" || ", filterChecks.ToArray()));
                    }
                    catch (Exception)
                    {

                    }
                }
                if (ShouldSort())
                {
                    var propInfo = SortFields.Select(x => GetPropertyInfo(x)).Where(x => x != null).ToList();
                    //Add where
                    if (propInfo.Any())
                    {
                        foreach (var info in propInfo)
                        {
                            foreach (var propertyInfo in info)
                            {
                                includeAssemblies.Add(propertyInfo.PropertyType.Assembly.Location);
                            }

                            var nullCheck = GetNullCheck("item", info);
                            if (!string.IsNullOrEmpty(nullCheck))
                            {
                                code.AppendFormat("items=items.Where(item=>{0});", nullCheck);
                                code.AppendLine();
                            }
                        }

                        var byProperties = propInfo.Select(x => GetPropertyPath(x)).ToList();
                        code.AppendLine("items = sortDescending");
                        code.AppendFormat("?items.OrderByDescending(item => item.{0})", byProperties.First());
                        foreach (var byProperty in byProperties.Skip(1))
                        {
                            code.AppendFormat(".ThenByDescending(item => item.{0})", byProperty);
                        }

                        code.AppendFormat(": items.OrderBy(item => item.{0})", byProperties.First());
                        foreach (var byProperty in byProperties.Skip(1))
                        {
                            code.AppendFormat(".ThenBy(item => item.{0})", byProperty);
                        }
                        code.AppendLine(";");
                    }

                }

                code.AppendFormat("return items;\r\n");
                code.AppendLine("} }");

                var assemblyName = "filter" + Guid.NewGuid().ToString("N");
                var cp = new CompilerParameters
                             {
                                 GenerateExecutable = false,
                                 OutputAssembly = assemblyName,
                                 GenerateInMemory = true,
                                 TreatWarningsAsErrors = false,
                                 CompilerOptions = "/optimize /t:library",
                                 IncludeDebugInformation = false,
                             };

                cp.ReferencedAssemblies.Add("mscorlib.dll");
                cp.ReferencedAssemblies.Add("system.dll");
                cp.ReferencedAssemblies.Add("System.Core.dll");
                cp.ReferencedAssemblies.Add(GetType().Assembly.Location);
                cp.ReferencedAssemblies.Add(typeof(T).Assembly.Location);
                foreach (var includeAssembly in includeAssemblies)
                {
                    cp.ReferencedAssemblies.Add(includeAssembly);
                }

                var cr = provider.CompileAssemblyFromSource(cp, code.ToString());
                if (!cr.Errors.HasErrors)
                {
                    var assembly = cr.CompiledAssembly;
                    var evaluatorType = assembly.GetType(typeName);
                    var evaluator = Activator.CreateInstance(evaluatorType);
                    resultFilter = evaluator as ListFilter<T>;

                }
                //Add anyway!!!
                FiltersCache.Add(uniqueCode, resultFilter);
            }
            return resultFilter;
        }

        private string GetPropertyToString(PropertyInfo propertyInfo)
        {
            if (typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return string.Empty;
            }
            if (typeof(IConvertible).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return ".ToString(CultureInfo.InvariantCulture)";
            }
            if (typeof(IFormattable).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return ".ToString(null, CultureInfo.InvariantCulture)";
            }
            return ".ToString()";
        }

        private HashSet<string> _alreadyNullChecked;

        private string GetNullCheck(string item, IEnumerable<PropertyInfo> byProperty)
        {
            if (_alreadyNullChecked == null)
            {
                _alreadyNullChecked = new HashSet<string>();
            }
            var stringBuilder = new StringBuilder();
            foreach (var propertyInfo in byProperty)
            {
                item = item + "." + propertyInfo.Name;
                if (!propertyInfo.PropertyType.IsValueType && !_alreadyNullChecked.Contains(item))
                {
                    stringBuilder.AppendFormat("&& {0}!=null ", item);
                    _alreadyNullChecked.Add(item);
                }
            }
            return stringBuilder.ToString().TrimStart('&');
        }

        private string GetPropertyPath(IEnumerable<PropertyInfo> filter)
        {
            return string.Join(".", filter.Select(x => x.Name).ToArray());
        }
    }
}