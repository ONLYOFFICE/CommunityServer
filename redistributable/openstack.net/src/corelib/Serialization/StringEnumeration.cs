using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace OpenStack.Serialization
{
    /// <summary>
    /// Using classes for enumerations allows us to use inheritance and enable API versions or other providers to share and extend "enums"
    /// <seealso href="https://lostechies.com/jimmybogard/2008/08/12/enumeration-classes/"/>
    /// </summary>
    /// <exclude />
    [JsonConverter(typeof(StringEnumerationConverter))]
    public abstract class StringEnumeration : IComparable
    {
        /// <summary />
        protected StringEnumeration()
        { }

        /// <summary />
        protected StringEnumeration(string displayName)
        {
            DisplayName = displayName;
        }
        
        /// <summary />
        public string DisplayName { get; protected set; }

        /// <summary />
        public override string ToString()
        {
            return DisplayName;
        }

        /// <summary />
        public static IEnumerable<T> GetAll<T>()
            where T : StringEnumeration
        {
            return GetAll(typeof(T)).Cast<T>();
        }

        /// <summary />
        public static IEnumerable<StringEnumeration> GetAll(Type type)
        {
            IEnumerable<FieldInfo> fields = type
                .GetFields(BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static)
                .Where(field => field.FieldType == type);

            return fields.Select(info => info.GetValue(null)).Cast<StringEnumeration>();
        }

        /// <summary />
        public override bool Equals(object obj)
        {
            var otherValue = obj as StringEnumeration;

            if (otherValue == null)
            {
                return false;
            }

            var typeMatches = GetType().Equals(obj.GetType());
            var valueMatches = DisplayName.Equals(otherValue.DisplayName);

            return typeMatches && valueMatches;
        }

        /// <summary />
        public static bool operator ==(StringEnumeration left, StringEnumeration right)
        {
            return Equals(left, right);
        }

        /// <summary />
        public static bool operator !=(StringEnumeration left, StringEnumeration right)
        {
            return !Equals(left, right);
        }

        /// <summary />
        public override int GetHashCode()
        {
            return DisplayName.GetHashCode();
        }

        /// <summary />
        public static T FromDisplayName<T>(string displayName)
            where T : StringEnumeration
        {
            return (T)FromDisplayName(typeof(T), displayName);
        }

        /// <summary />
        public static StringEnumeration FromDisplayName(Type objectType, string displayName)
        {
            return GetAll(objectType).FirstOrDefault(item => item.DisplayName == displayName);
        }

        /// <summary />
        public int CompareTo(object other)
        {
            return string.Compare(DisplayName, ((StringEnumeration)other).DisplayName, StringComparison.Ordinal);
        }
    }
}
