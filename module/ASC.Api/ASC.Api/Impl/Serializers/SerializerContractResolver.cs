/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.Linq;
using ASC.Api.Utils;
using Newtonsoft.Json.Serialization;

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

        private static readonly IDictionary<FieldsResolverKey,JsonContract> FieldsContractCache = new ASC.Collections.SynchronizedDictionary<FieldsResolverKey,JsonContract>(); 

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