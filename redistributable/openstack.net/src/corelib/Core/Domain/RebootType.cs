namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the type of a reboot operation.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known reboot types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverter(typeof(RebootType.Converter))]
    public sealed class RebootType : ExtensibleEnum<RebootType>
    {
        private static readonly ConcurrentDictionary<string, RebootType> _types =
            new ConcurrentDictionary<string, RebootType>(StringComparer.OrdinalIgnoreCase);
        private static readonly RebootType _hard = FromName("HARD");
        private static readonly RebootType _soft = FromName("SOFT");

        /// <summary>
        /// Initializes a new instance of the <see cref="RebootType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private RebootType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="RebootType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="RebootType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static RebootType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new RebootType(i));
        }

        /// <summary>
        /// Gets a <see cref="RebootType"/> representing the equivalent of cycling power to the server.
        /// </summary>
        public static RebootType Hard
        {
            get
            {
                return _hard;
            }
        }

        /// <summary>
        /// Gets a <see cref="RebootType"/> representing a reboot performed by signaling the server's
        /// operating system to restart, allowing for graceful shutdown of currently executing processes.
        /// </summary>
        public static RebootType Soft
        {
            get
            {
                return _soft;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="RebootType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override RebootType FromName(string name)
            {
                return RebootType.FromName(name);
            }
        }
    }
}
