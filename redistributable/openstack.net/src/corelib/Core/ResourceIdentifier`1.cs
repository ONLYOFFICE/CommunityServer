namespace net.openstack.Core
{
    using System;
    using net.openstack.Core.Domain.Converters;

    /// <summary>
    /// Represents a unique identifier within the context of a cloud services provider.
    /// </summary>
    /// <typeparam name="T">The resource identifier type.</typeparam>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    public abstract class ResourceIdentifier<T> : IEquatable<T>
        where T : ResourceIdentifier<T>
    {
        /// <summary>
        /// This is the backing field for the <see cref="Value"/> property.
        /// </summary>
        private readonly string _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceIdentifier{T}"/> class
        /// with the specified identifier.
        /// </summary>
        /// <param name="id">The resource identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        protected ResourceIdentifier(string id)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id cannot be empty");

            _id = id;
        }

        /// <summary>
        /// Determines whether two specified resource identifiers have the same value.
        /// </summary>
        /// <param name="left">The first resource identifier to compare, or <see langword="null"/>.</param>
        /// <param name="right">The second resource identifier to compare, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the value of <paramref name="left"/> is the same as the value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator ==(ResourceIdentifier<T> left, ResourceIdentifier<T> right)
        {
            if (object.ReferenceEquals(left, null))
                return object.ReferenceEquals(right, null);
            else if (object.ReferenceEquals(right, null))
                return false;

            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two specified resource identifiers have different values.
        /// </summary>
        /// <param name="left">The first resource identifier to compare, or <see langword="null"/>.</param>
        /// <param name="right">The second resource identifier to compare, or <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the value of <paramref name="left"/> is different from the value of <paramref name="right"/>; otherwise, <see langword="false"/>.</returns>
        public static bool operator !=(ResourceIdentifier<T> left, ResourceIdentifier<T> right)
        {
            return !(left == right);
        }

        /// <summary>
        /// Gets the value of this resource identifier.
        /// </summary>
        public string Value
        {
            get
            {
                return _id;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The default implementation uses <see cref="StringComparer.Ordinal"/> to compare
        /// the <see cref="Value"/> property of two identifiers.
        ///
        /// <note type="implement">
        /// This method may be overridden to change the way unique identifiers are compared.
        /// </note>
        /// </remarks>
        public virtual bool Equals(T other)
        {
            if (object.ReferenceEquals(other, null))
                return false;

            return StringComparer.Ordinal.Equals(_id, other._id);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as T);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// The default implementation uses <see cref="StringComparer.Ordinal"/> to calculate
        /// and return a hash code from the <see cref="Value"/> property.
        ///
        /// <note type="implement">
        /// This method may be overridden to change the way unique identifiers are compared.
        /// </note>
        /// </remarks>
        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(_id);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _id;
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ResourceIdentifier{T}"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        protected abstract class ConverterBase : SimpleStringJsonConverter<T>
        {
            /// <remarks>
            /// This method uses <see cref="Value"/> for serialization.
            /// </remarks>
            /// <inheritdoc/>
            protected override string ConvertToString(T obj)
            {
                return obj.Value;
            }

            /// <remarks>
            /// If <paramref name="str"/> is <see langword="null"/> or an empty string, this method returns <see langword="null"/>.
            /// Otherwise, this method uses <see cref="FromValue"/> for deserialization.
            /// </remarks>
            /// <inheritdoc/>
            protected override T ConvertToObject(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return null;

                return FromValue(str);
            }

            /// <summary>
            /// Creates a resource identifier with the given value.
            /// </summary>
            /// <param name="id">The resource identifier value. This value is never <see langword="null"/> or empty.</param>
            /// <returns>An instance of <typeparamref name="T"/> corresponding representing the specified <paramref name="id"/>.</returns>
            protected abstract T FromValue(string id);
        }
    }
}
