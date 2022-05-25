using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;

namespace net.openstack.Core.Domain
{
    /// <summary>
    /// Represents the way a user has authenticated.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known authentication types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AuthenticationType.Converter))]
    public class AuthenticationType : ExtensibleEnum<AuthenticationType>
    {
        private static readonly ConcurrentDictionary<string, AuthenticationType> _types =
            new ConcurrentDictionary<string, AuthenticationType>(StringComparer.OrdinalIgnoreCase);

        private static readonly AuthenticationType _password = FromName("PASSWORD");
        private static readonly AuthenticationType _rsa = FromName("RSAKEY");

        private AuthenticationType(string name) : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="AuthenticationType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="AuthenticationType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static AuthenticationType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new AuthenticationType(i));
        }

        /// <summary>
        /// Gets an <see cref="AuthenticationType"/> representing that a user authenticated using a password.
        /// </summary>
        public static AuthenticationType Password
        {
            get
            {
                return _password;
            }
        }

        /// <summary>
        /// Gets an <see cref="AuthenticationType"/> representing that a user authenticated using an RSA key.
        /// </summary>
        public static AuthenticationType RSA
        {
            get
            {
                return _rsa;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AuthenticationType"/> objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AuthenticationType FromName(string name)
            {
                return AuthenticationType.FromName(name);
            }
        }
    }
}
