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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ASC.Api.Utils
{
    public static class Binder
    {
        private static object CreateParamType(Type modelType)
        {
            var type = modelType;
            if (modelType.IsGenericType)
            {
                var genericTypeDefinition = modelType.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(IDictionary<,>))
                    type = typeof(Dictionary<,>).MakeGenericType(modelType.GetGenericArguments());
                else if (genericTypeDefinition == typeof(IEnumerable<>) || genericTypeDefinition == typeof(ICollection<>) || genericTypeDefinition == typeof(IList<>))
                    type = typeof(List<>).MakeGenericType(modelType.GetGenericArguments());
            }
            if (modelType.IsArray && modelType.HasElementType)
            {
                //Create generic list and we will convert it to aaray  later
                type = typeof(List<>).MakeGenericType(modelType.GetElementType());
            }
            return Activator.CreateInstance(type);
        }

        public static Type GetCollectionType(Type modelType)
        {
            Type type = null;
            if (modelType.IsGenericType)
            {
                var genericTypeDefinition = modelType.GetGenericTypeDefinition();
                if (typeof(IDictionary).IsAssignableFrom(genericTypeDefinition))
                    type = modelType.GetGenericArguments().Last();
                else if (typeof(IEnumerable).IsAssignableFrom(genericTypeDefinition) || typeof(ICollection).IsAssignableFrom(genericTypeDefinition) || typeof(IList).IsAssignableFrom(genericTypeDefinition))
                    type = modelType.GetGenericArguments().Last();
            }
            if (modelType.IsArray && modelType.HasElementType)
            {
                type = modelType.GetElementType();
            }
            return type;
        }

        public static object Bind(Type type, NameValueCollection collection)
        {
            return Bind(type, collection, string.Empty);
        }

        public static T Bind<T>(NameValueCollection collection)
        {
            return Bind<T>(collection, string.Empty);
        }

        public static T Bind<T>(NameValueCollection collection, string prefix)
        {
            return (T)Bind(typeof(T), collection, prefix);
        }

        public static object Bind(Type type, NameValueCollection collection, string prefix)
        {
            if (IsSimple(type))
            {
                //Just bind
                var value = GetValue(GetNameTransforms(prefix), collection, string.Empty);
                if (value != null)
                {
                    return ConvertUtils.GetConverted(value, type);
                }
            }
            var binded = CreateParamType(type);
            //Get all props
            if (IsCollection(type))
            {
                BindCollection(prefix, (IList)binded, collection);
            }
            else
            {
                binded = BindObject(type, binded, collection, prefix);
            }
            return ConvertToSourceType(binded, type);
        }

        private static object ConvertToSourceType(object binded, Type type)
        {
            if (binded != null)
            {
                if (type.IsArray && type.HasElementType && binded.GetType().IsGenericType && IsCollection(binded.GetType()))
                {
                    var genericType = binded.GetType().GetGenericArguments();
                    var arrayType = type.GetElementType();
                    if (genericType.Length == 1 && genericType[0] == arrayType)
                    {
                        IList collection = (IList)binded;
                        //Its list need to convert to array
                        Array array = Array.CreateInstance(arrayType, collection.Count);
                        collection.CopyTo(array, 0);
                        return array;
                    }
                }
            }
            return binded;
        }


        private static object BindObject(IReflect type, object binded, NameValueCollection values, string prefix)
        {
            //Get properties
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var isBinded = false;
            foreach (var propertyInfo in properties)
            {
                if (IsSimple(propertyInfo.PropertyType))
                {
                    //Set from collection
                    var value = GetValue(GetName(propertyInfo), values, prefix);
                    if (value != null)
                    {
                        propertyInfo.SetValue(binded, ConvertUtils.GetConverted(value,propertyInfo), null);
                        isBinded = true;
                    }
                }
                else
                {
                    var propNames = GetName(propertyInfo);
                    PropertyInfo info = propertyInfo;
                    foreach (string propName in propNames)
                    {
                        string bindPrefix = !string.IsNullOrEmpty(prefix) ? prefix + string.Format("[{0}]", propName) : propName;
                        //find prefix in collection
                        if (IsPrefixExists(bindPrefix, values))
                        {
                            var result = Bind(info.PropertyType, values, bindPrefix);
                            if (result != null)
                            {
                                propertyInfo.SetValue(binded, result, null);
                                break;
                            }
                        }
                    }
                }
            }
            return isBinded ? binded : null;
        }

        private static bool IsPrefixExists(string prefix, NameValueCollection values)
        {
            return values.AllKeys.Any(x => x != null && x.StartsWith(prefix));
        }

        private static IEnumerable<string> GetName(PropertyInfo property)
        {
            return GetNameTransforms(property.Name);
        }

        private static IEnumerable<string> GetNameTransforms(string name)
        {
            return new[]
                       {
                           name, 
                           StringUtils.ToCamelCase(name),
                           name.ToLower()
                       };
        }


        private static string GetValue(IEnumerable<string> names, NameValueCollection values, string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return names.Select(name => values[name]).FirstOrDefault(value => !string.IsNullOrEmpty(value));
            }

            //var regex = new Regex(@"^(\[){0,1}(" + string.Join("|", names.Select(name => Regex.Escape(name)).ToArray()) + @")(\]){0,1}");

            var keys = from key in values.AllKeys
                       where key != null && key.StartsWith(prefix)
                       let nameKey = key.Substring(prefix.Length).Trim('[', '.', ']')
                       where names.Contains(nameKey)
                       select key;

            return values[keys.SingleOrDefault() ?? string.Empty];
        }

        private static readonly ConcurrentDictionary<string, Regex> CollectionPrefixCache = new ConcurrentDictionary<string, Regex>();

        private static void BindCollection(string prefix, IList collection, NameValueCollection values)
        {
            Regex parse = GetParseRegex(prefix);

            //Parse values related to collection
            const string simple = "simple";
            var keys = from key in values.AllKeys.Where(key => key != null)
                       let match = parse.Match(key)
                       group key by string.IsNullOrEmpty(match.Groups["pos"].Value) ? (match.Groups["arrleft"].Success ? string.Empty : simple) : match.Groups["pos"].Value into key
                       select key;
            foreach (var key in keys)
            {
                int index;
                bool indexed = int.TryParse(key.Key, out index);

                Type genericType = null;
                var collectionType = collection.GetType();
                if (collectionType.IsGenericType)
                {
                    genericType = collectionType.GetGenericArguments().SingleOrDefault();
                }
                else if (collectionType.IsArray && collectionType.HasElementType)
                {
                    //Create array
                    genericType = collectionType.GetElementType();
                }

                if (genericType != null)
                {

                    var newprefix = simple.Equals(key.Key) ? prefix : prefix + "[" + (indexed ? index.ToString(CultureInfo.InvariantCulture) : "") + "]";
                    if (IsSimple(genericType))
                    {
                        //Collect all and insert
                        var collectionValues = values.GetValues(newprefix);
                        if (collectionValues != null)
                        {
                            foreach (var collectionValue in collectionValues)
                            {
                                collection.Add(ConvertUtils.GetConverted(collectionValue,genericType));
                            }
                        }

                    }
                    else
                    {
                        var constructed = Bind(genericType, values, newprefix);
                        if (constructed != null)
                            collection.Insert(index, constructed);
                    }
                }
            }

        }

        private static Regex GetParseRegex(string prefix)
        {
            Regex value = null;
            if (!CollectionPrefixCache.TryGetValue(prefix, out value))
            {
                value = new Regex("(?'prefix'^" + Regex.Escape(prefix) +
                    @"(?'arrleft'\[){0,1}(?'pos'[\d]+){0,1}(?'arrright'\]){0,1})(?'suffix'.+){0,1}",
                    RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                CollectionPrefixCache.TryAdd(prefix, value);
            }
            return value;
        }

        public static bool IsCollection(Type type)
        {
            return !IsSimple(type) && (type.IsArray || HasInterface(type, typeof(IEnumerable)));
        }

        public static bool HasInterface(Type type, Type interfaceType)
        {
            return type.GetInterfaces().Any(x => x == interfaceType);
        }

        private static readonly IDictionary<Type, bool> SimpleMap = new ConcurrentDictionary<Type, bool>();

        private static bool IsSimple(Type type)
        {
            bool simple;
            if (type.IsPrimitive) return true;

            if (!SimpleMap.TryGetValue(type, out simple))
            {
                var converter = TypeDescriptor.GetConverter(type);//TODO: optimize?
                simple = converter.CanConvertFrom(typeof(string));
                SimpleMap[type] = simple;
            }
            return simple;
        }

        public static long GetCollectionCount(object collection)
        {
            var collection1 = collection as ICollection;
            if (collection1 != null)
                return (collection1).Count;

            var responceType = collection.GetType();
            var lengthProperty = responceType.GetProperty("Count", BindingFlags.Public | BindingFlags.Instance) ?? responceType.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
            if (lengthProperty != null)
            {
                return Convert.ToInt64(lengthProperty.GetValue(collection, new object[0]));
            }
            return 1;
        }
    }
}