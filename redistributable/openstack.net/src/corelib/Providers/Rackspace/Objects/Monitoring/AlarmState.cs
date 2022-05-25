namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an alarm state in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known alarm states,
    /// with added support for unknown states returned by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(AlarmState.Converter))]
    public sealed class AlarmState : ExtensibleEnum<AlarmState>
    {
        private static readonly ConcurrentDictionary<string, AlarmState> _types =
            new ConcurrentDictionary<string, AlarmState>(StringComparer.OrdinalIgnoreCase);
        private static readonly AlarmState _ok = FromName("OK");
        private static readonly AlarmState _warning = FromName("WARNING");
        private static readonly AlarmState _critical = FromName("CRITICAL");

        /// <summary>
        /// Initializes a new instance of the <see cref="AlarmState"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private AlarmState(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="AlarmState"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="AlarmState"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static AlarmState FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new AlarmState(i));
        }

        /// <summary>
        /// Gets a <see cref="AlarmState"/> representing the <c>OK</c> alarm state.
        /// </summary>
        public static AlarmState OK
        {
            get
            {
                return _ok;
            }
        }

        /// <summary>
        /// Gets a <see cref="AlarmState"/> representing the <c>WARNING</c> alarm state.
        /// </summary>
        public static AlarmState Warning
        {
            get
            {
                return _warning;
            }
        }

        /// <summary>
        /// Gets a <see cref="AlarmState"/> representing the <c>CRITICAL</c> alarm state.
        /// </summary>
        public static AlarmState Critical
        {
            get
            {
                return _critical;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="AlarmState"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override AlarmState FromName(string name)
            {
                return AlarmState.FromName(name);
            }
        }
    }
}
