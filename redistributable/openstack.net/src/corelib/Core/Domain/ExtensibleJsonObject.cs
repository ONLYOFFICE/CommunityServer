using System.Collections.ObjectModel;

namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using IEnumerable = System.Collections.IEnumerable;
    using IEnumerator = System.Collections.IEnumerator;

    /// <summary>
    /// This is the abstract base class for types modeling the JSON representation of a resource
    /// which may be extended by specific providers or updated in future releases of a core
    /// service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ExtensibleJsonObject
    {
        /// <summary>
        /// An empty, and thus immutable, value which is the default return value
        /// for <see cref="ExtensionData"/> when the backing field is <see langword="null"/>.
        /// </summary>
        protected static readonly ReadOnlyDictionary<string, JToken> EmptyExtensionData =
            new ReadOnlyDictionary<string, JToken>(new Dictionary<string, JToken>());

        /// <summary>
        /// This is the backing field for the <see cref="ExtensionData"/> property.
        /// </summary>
        private Dictionary<string, JToken> _extensionData;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleJsonObject"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ExtensibleJsonObject()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleJsonObject"/> class
        /// with the specified extension data.
        /// </summary>
        /// <param name="extensionData">The extension data.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensionData"/> is <see langword="null"/>.</exception>
        protected ExtensibleJsonObject(IDictionary<string, JToken> extensionData)
        {
            if (extensionData == null)
                throw new ArgumentNullException("extensionData");

            if (extensionData.Count > 0)
                _extensionData = new Dictionary<string, JToken>(extensionData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleJsonObject"/> class
        /// with the specified extension data.
        /// </summary>
        /// <param name="extensionData">The extension data.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensionData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="extensionData"/> contains any <see langword="null"/> values.</exception>
        protected ExtensibleJsonObject(IEnumerable<JProperty> extensionData)
            : this(extensionData.ToArray())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleJsonObject"/> class
        /// with the specified extension data.
        /// </summary>
        /// <param name="extensionData">The extension data.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="extensionData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="extensionData"/> contains any <see langword="null"/> values.</exception>
        protected ExtensibleJsonObject(params JProperty[] extensionData)
        {
            if (extensionData == null)
                throw new ArgumentNullException("extensionData");

            if (extensionData.Length > 0)
            {
                _extensionData = new Dictionary<string, JToken>();
                foreach (JProperty property in extensionData)
                {
                    if (property == null)
                        throw new ArgumentException("extensionData cannot contain any null values");

                    _extensionData[property.Name] = property.Value;
                }
            }
        }

        /// <summary>
        /// Gets a map of object properties which did not map to another field or property
        /// during JSON deserialization. The keys of the map represent the property names,
        /// and the values are <see cref="JToken"/> instances containing the parsed JSON
        /// values.
        /// </summary>
        /// <value>
        /// A collection of object properties which did not map to another field or property
        /// during JSON deserialization.
        /// <para>-or-</para>
        /// <para>An empty dictionary if the object did not contain any unmapped properties.</para>
        /// </value>
        public ReadOnlyDictionary<string, JToken> ExtensionData
        {
            get
            {
                if (_extensionData == null)
                    return EmptyExtensionData;

                return new ReadOnlyDictionary<string, JToken>(_extensionData);
            }
        }

        /// <summary>
        /// This property exposes the <see cref="_extensionData"/> field to Json.NET as a dictionary with
        /// <see cref="object"/> values instead of <see cref="JToken"/> values, which works around a known bug in the
        /// way Json.NET 5.x handled <see langword="null"/> values in the extension data.
        /// </summary>
        [JsonExtensionData]
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ExtensionDataDictionary ExtensionDataWrapper
        {
            get
            {
                // This can never return null or Json.NET will attempt to set the value.
                return new ExtensionDataDictionary(this);
            }

            set
            {
                // This setter must exist or Json.NET will not recognize the extension data. It cannot be used because
                // Json.NET will bypass the getter, resulting in a lost update.
                throw new NotSupportedException("Attempted to set the extension data wrapper. See issue openstacknetsdk/openstack.net#419.");
            }
        }

        /// <summary>
        /// Converts an object to a <see cref="JToken"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Unlike <see cref="JToken.FromObject(object)"/>, this method supports <see langword="null"/> values.
        /// </para>
        /// </remarks>
        /// <param name="obj">The object.</param>
        /// <returns>
        /// <para>The result of calling <see cref="JToken.FromObject(object)"/> on the input object.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if <paramref name="obj"/> is <see langword="null"/>.</para>
        /// </returns>
        private static JToken ToJToken(object obj)
        {
            if (obj == null)
                return null;

            return JToken.FromObject(obj);
        }

        /// <summary>
        /// This class works around a known bug in Json.NET's handling of JSON extension data.
        /// </summary>
        /// <remarks>
        /// <para>Adding values to the underlying dictionary requires converting the value to a <see cref="JToken"/> by
        /// calling <see cref="ToJToken(object)"/>. Reading values does not require the inverse because the serializer
        /// in Json.NET has no trouble handling <see cref="JToken"/> values as input.</para>
        /// </remarks>
        /// <seealso cref="ExtensionDataWrapper"/>
        private sealed class ExtensionDataDictionary : IDictionary<string, object>
        {
            private readonly ExtensibleJsonObject _underlying;

            [JsonConstructor]
            private ExtensionDataDictionary()
            {
                // This constructor must exist or Json.NET will not be able to set the extension data. It cannot be used
                // because Json.NET will not set the required _underlying field.
                throw new NotSupportedException("Attempted to create the extension data wrapper with its underlying object. See issue openstacknetsdk/openstack.net#419.");
            }

            public ExtensionDataDictionary(ExtensibleJsonObject extensibleJsonObject)
            {
                if (extensibleJsonObject == null)
                    throw new ArgumentNullException("extensibleJsonObject");

                _underlying = extensibleJsonObject;
            }

            public object this[string key]
            {
                get
                {
                    return _underlying.ExtensionData[key];
                }

                set
                {
                    GetOrCreateExtensionData()[key] = ToJToken(value);
                }
            }

            public int Count
            {
                get
                {
                    return _underlying.ExtensionData.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public ICollection<string> Keys
            {
                get
                {
                    return _underlying.ExtensionData.Keys;
                }
            }

            public ICollection<object> Values
            {
                get
                {
                    return new ExtensionDataValues(_underlying.ExtensionData.Values);
                }
            }

            public void Add(KeyValuePair<string, object> item)
            {
                IDictionary<string, JToken> extensionData = GetOrCreateExtensionData();
                extensionData.Add(new KeyValuePair<string, JToken>(item.Key, ToJToken(item.Value)));
            }

            public void Add(string key, object value)
            {
                GetOrCreateExtensionData().Add(key, ToJToken(value));
            }

            public void Clear()
            {
                GetOrCreateExtensionData().Clear();
            }

            public bool Contains(KeyValuePair<string, object> item)
            {
                return _underlying.ExtensionData.Contains(new KeyValuePair<string, JToken>(item.Key, ToJToken(item.Value)));
            }

            public bool ContainsKey(string key)
            {
                return _underlying.ExtensionData.ContainsKey(key);
            }

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                IDictionary<string, object> intermediate = new Dictionary<string, object>(_underlying.ExtensionData.ToDictionary(i => i.Key, i => (object)i.Value));
                intermediate.CopyTo(array, arrayIndex);
            }

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
            {
                return _underlying.ExtensionData.Select(i => new KeyValuePair<string, object>(i.Key, i.Value)).GetEnumerator();
            }

            public bool Remove(KeyValuePair<string, object> item)
            {
                IDictionary<string, JToken> extensionData = _underlying._extensionData;
                if (extensionData == null)
                    return false;

                return extensionData.Remove(new KeyValuePair<string, JToken>(item.Key, ToJToken(item.Value)));
            }

            public bool Remove(string key)
            {
                var extensionData = _underlying._extensionData;
                if (extensionData == null)
                    return false;

                return extensionData.Remove(key);
            }

            public bool TryGetValue(string key, out object value)
            {
                JToken intermediate;
                bool result = _underlying.ExtensionData.TryGetValue(key, out intermediate);
                value = intermediate;
                return result;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private Dictionary<string, JToken> GetOrCreateExtensionData()
            {
                var result = _underlying._extensionData;
                if (result == null)
                {
                    result = new Dictionary<string, JToken>();
                    _underlying._extensionData = result;
                }

                return result;
            }
        }

        /// <summary>
        /// This class works around a known bug in Json.NET's handling of JSON extension data.
        /// </summary>
        /// <seealso cref="ExtensionDataWrapper"/>
        private class ExtensionDataValues : ICollection<object>
        {
            private readonly ICollection<JToken> _values;

            public ExtensionDataValues(ICollection<JToken> values)
            {
                if (values == null)
                    throw new ArgumentNullException("values");

                _values = values;
            }

            public int Count
            {
                get
                {
                    return _values.Count;
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return _values.IsReadOnly;
                }
            }

            public void Add(object item)
            {
                _values.Add(ToJToken(item));
            }

            public void Clear()
            {
                _values.Clear();
            }

            public bool Contains(object item)
            {
                return _values.Contains(ToJToken(item));
            }

            public void CopyTo(object[] array, int arrayIndex)
            {
                ICollection<object> intermediate = _values.ToArray();
                intermediate.CopyTo(array, arrayIndex);
            }

            public IEnumerator<object> GetEnumerator()
            {
                return _values.Cast<object>().GetEnumerator();
            }

            public bool Remove(object item)
            {
                return _values.Remove(ToJToken(item));
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
