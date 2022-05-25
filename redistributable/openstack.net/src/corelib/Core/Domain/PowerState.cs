namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Concurrent;

    using Newtonsoft.Json;

    /// <summary>
    /// Represents the power state of a server.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known power states,
    /// with added support for unknown states returned by a server extension.
    ///
    /// <note>
    /// This property is defined by the Rackspace-specific Extended Status Extension
    /// to the OpenStack Compute API. The API does not regulate the status values,
    /// so it is possible that values can be added, removed, or renamed.
    /// </note>
    /// </remarks>
    /// <seealso cref="Server.PowerState"/>
    /// <seealso href="http://docs.rackspace.com/servers/api/v2/cs-devguide/content/ch_extensions.html#power_state">OS-EXT-STS:power_state (Rackspace Next Generation Cloud Servers Developer Guide - API v2)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonConverter(typeof(PowerState.Converter))]
    public sealed class PowerState : ExtensibleEnum<PowerState>
    {
        private static readonly ConcurrentDictionary<string, PowerState> _states =
            new ConcurrentDictionary<string, PowerState>(StringComparer.OrdinalIgnoreCase);
        private static readonly PowerState _poweredDown = FromName("0");
        private static readonly PowerState _poweredUp = FromName("1");
        private static readonly PowerState _shutOff = FromName("4");

        /// <summary>
        /// Initializes a new instance of the <see cref="PowerState"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private PowerState(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="PowerState"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="PowerState"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static PowerState FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _states.GetOrAdd(name, i => new PowerState(i));
        }

        /// <summary>
        /// Gets a <see cref="PowerState"/> instance representing <placeholder>description</placeholder>.
        /// </summary>
        public static PowerState PoweredDown
        {
            get
            {
                return _poweredDown;
            }
        }

        /// <summary>
        /// Gets a <see cref="PowerState"/> instance representing <placeholder>description</placeholder>.
        /// </summary>
        public static PowerState PoweredUp
        {
            get
            {
                return _poweredUp;
            }
        }

        /// <summary>
        /// Gets a <see cref="PowerState"/> instance representing <placeholder>description</placeholder>.
        /// </summary>
        public static PowerState ShutOff
        {
            get
            {
                return _shutOff;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="PowerState"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override PowerState FromName(string name)
            {
                return PowerState.FromName(name);
            }
        }
    }
}
