namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an Auto Scale launch type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known launch types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/PUT_putLaunchConfig_v1.0__tenantId__groups__groupId__launch_Configurations.html">Update launch configuration (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(LaunchType.Converter))]
    public sealed class LaunchType : ExtensibleEnum<LaunchType>
    {
        private static readonly ConcurrentDictionary<string, LaunchType> _types =
            new ConcurrentDictionary<string, LaunchType>(StringComparer.OrdinalIgnoreCase);
        private static readonly LaunchType _launchServer = FromName("launch_server");

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private LaunchType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="LaunchType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="LaunchType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static LaunchType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new LaunchType(i));
        }

        /// <summary>
        /// Gets a <see cref="LaunchType"/> representing a launch configuration that launches new server resources.
        /// </summary>
        public static LaunchType LaunchServer
        {
            get
            {
                return _launchServer;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="LaunchType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override LaunchType FromName(string name)
            {
                return LaunchType.FromName(name);
            }
        }
    }
}
