namespace net.openstack.Core
{
    using System;

    using net.openstack.Core.Domain.Converters;

    /// <summary>
    /// Represents the base class for extensible enumeration types used
    /// for strongly-typed values in JSON object models.
    /// </summary>
    /// <typeparam name="T">The enumeration type.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    public abstract class ExtensibleEnum<T> : IEquatable<T>
        where T : ExtensibleEnum<T>
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensibleEnum{T}"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        protected ExtensibleEnum(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            _name = name;
        }

        /// <summary>
        /// Gets the canonical name of this member.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <inheritdoc/>
        public bool Equals(T other)
        {
            return this == other;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ExtensibleEnum{T}"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        protected abstract class ConverterBase : SimpleStringJsonConverter<T>
        {
            /// <remarks>
            /// This method uses <see cref="Name"/> for serialization.
            /// </remarks>
            /// <inheritdoc/>
            protected override string ConvertToString(T obj)
            {
                return obj.Name;
            }

            /// <remarks>
            /// If <paramref name="str"/> is an empty string, this method returns <see langword="null"/>.
            /// Otherwise, this method uses <see cref="FromName"/> for deserialization.
            /// </remarks>
            /// <inheritdoc/>
            protected override T ConvertToObject(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;

                return FromName(str);
            }

            /// <summary>
            /// Gets or creates the enumeration member with the given name.
            /// </summary>
            /// <param name="name">The name of the member. This value is never <see langword="null"/> or empty.</param>
            /// <returns>The instance of <typeparamref name="T"/> corresponding to the specified <paramref name="name"/>.</returns>
            protected abstract T FromName(string name);
        }
    }
}
