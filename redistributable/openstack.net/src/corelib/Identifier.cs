using System;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack
{
    /// <summary>
    /// Represents a uniqe identifier.
    /// <para>
    /// This is a Guid which has implicit and explicit conversions to/from Guid and String.
    /// </para>
    /// </summary>
    [JsonConverter(typeof(IdentifierConverter))]
    public class Identifier
    {
        private readonly string _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Identifier(string id)
        {
            _id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Identifier"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public Identifier(Guid id)
        {
            _id = id.ToString("D");
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _id;
        }

        #region Conversions
        /// <summary>
        /// Performs an implicit conversion from <see cref="Guid"/> to <see cref="Identifier"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Identifier(Guid id)
        {
            return new Identifier(id);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Identifier"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(Identifier id)
        {
            return id?.ToString();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="Identifier"/>.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Identifier(string id)
        {
            return new Identifier(id);
        }
        #endregion

        #region Equality

        private bool Equals(Identifier other)
        {
            return _id.Equals(other._id);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var stringOther = obj as string;
            if (stringOther != null)
            {
                Guid idGuid;
                return Guid.TryParse(stringOther, out idGuid) && Equals(new Identifier(idGuid));
            }

            var guidOther = obj as Guid?;
            if (guidOther != null)
                return Equals(new Identifier(guidOther.Value));

            var otherId = obj as Identifier;
            return otherId != null && Equals(otherId);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        /// <inheritdoc />
        public static bool operator ==(Identifier left, Identifier right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc />
        public static bool operator !=(Identifier left, Identifier right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}