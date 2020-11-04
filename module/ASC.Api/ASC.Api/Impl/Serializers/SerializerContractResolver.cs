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


using ASC.Api.Utils;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ASC.Api.Impl.Serializers
{
    internal class SerializerContractResolver : CamelCasePropertyNamesContractResolver
    {
        private readonly Type _responseType;
        private readonly HashSet<string> _filterToFields;

        public SerializerContractResolver(object response, ApiContext context)
        {
            if (response != null)
            {
                //Try resolve context
                try
                {
                    if (context != null && context.Fields != null && context.Fields.Any())
                    {
                        //Get type
                        _filterToFields = new HashSet<string>(context.Fields, new StringIgnoreCaseComparer());
                        _responseType = response.GetType();
                        if (Binder.IsCollection(_responseType))
                        {
                            //Get collection type
                            _responseType = Binder.GetCollectionType(_responseType);
                        }
                    }
                }
                catch
                {

                }
            }
        }

        private struct FieldsResolverKey : IEquatable<FieldsResolverKey>
        {
            private readonly Type _contractType;
            private readonly string _fields;

            public FieldsResolverKey(Type contractType, IEnumerable<string> fields)
            {
                _contractType = contractType;
                _fields = string.Join(",",fields.Select(x=>x.Trim().ToLowerInvariant()).OrderBy(x=>x).ToArray());
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public bool Equals(FieldsResolverKey other)
            {
                return other._contractType == _contractType && Equals(other._fields, _fields);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((_contractType != null ? _contractType.GetHashCode() : 0)*397) ^ (_fields != null ? _fields.GetHashCode() : 0);
                }
            }

            public static bool operator ==(FieldsResolverKey left, FieldsResolverKey right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(FieldsResolverKey left, FieldsResolverKey right)
            {
                return !left.Equals(right);
            }
        }

        private static readonly IDictionary<FieldsResolverKey, JsonContract> FieldsContractCache = new ConcurrentDictionary<FieldsResolverKey, JsonContract>(); 

        public override JsonContract ResolveContract(Type type)
        {
            if (IsSourceType(type) && _filterToFields != null && _filterToFields.Any())
            {
                var key = new FieldsResolverKey(type, _filterToFields);
                JsonContract contract;
                if (!FieldsContractCache.TryGetValue(key,out contract))
                {
                    //Do a new resolve
                    contract = CreateContract(type);
                    //Cache it
                    FieldsContractCache[key]=contract;
                }
                return contract;
            }
            return base.ResolveContract(type);
        }

        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            if (_filterToFields != null && member.DeclaringType != null && IsSourceType(member.DeclaringType))
            {
                if (!_filterToFields.Contains(member.Name))
                    return null;
            }
            return property;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            if (_filterToFields != null && IsSourceType(type))
            {
                properties = properties.Where(x => _filterToFields.Contains(x.PropertyName)).ToList();
            }
            return properties;
        }

        protected override List<System.Reflection.MemberInfo> GetSerializableMembers(Type objectType)
        {
            var baseMembers = base.GetSerializableMembers(objectType);
            if (_filterToFields != null && IsSourceType(objectType))
            {
                //Filter!
                baseMembers = baseMembers.Where(x => _filterToFields.Contains(x.Name)).ToList();
            }
            return baseMembers;
        }

        private bool IsSourceType(Type type)
        {
            return _responseType != null && (type == _responseType || _responseType.IsAssignableFrom(type) || type.IsAssignableFrom(_responseType));
        }

    }
}