/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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