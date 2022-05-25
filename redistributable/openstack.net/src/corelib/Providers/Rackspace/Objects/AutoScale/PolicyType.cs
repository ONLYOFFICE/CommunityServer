namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents an Auto Scale policy type.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known policy types,
    /// with added support for unknown types returned by a server extension.
    /// </remarks>
    /// <seealso href="http://docs.rackspace.com/cas/api/v1.0/autoscale-devguide/content/POST_createPolicies_v1.0__tenantId__groups__groupId__policies_autoscale-policies.html">Create policy (Rackspace Auto Scale Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(PolicyType.Converter))]
    public sealed class PolicyType : ExtensibleEnum<PolicyType>
    {
        private static readonly ConcurrentDictionary<string, PolicyType> _types =
            new ConcurrentDictionary<string, PolicyType>(StringComparer.OrdinalIgnoreCase);
        private static readonly PolicyType _webhook = FromName("webhook");
        private static readonly PolicyType _schedule = FromName("schedule");
        private static readonly PolicyType _cloudMonitoring = FromName("cloud_monitoring");

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private PolicyType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="PolicyType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="PolicyType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static PolicyType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new PolicyType(i));
        }

        /// <summary>
        /// Gets a <see cref="PolicyType"/> representing a webhook scaling policy.
        /// </summary>
        public static PolicyType Webhook
        {
            get
            {
                return _webhook;
            }
        }

        /// <summary>
        /// Gets a <see cref="PolicyType"/> representing a scheduled scaling policy.
        /// </summary>
        public static PolicyType Schedule
        {
            get
            {
                return _schedule;
            }
        }

        /// <summary>
        /// Gets a <see cref="PolicyType"/> representing a scaling policy triggered by the Cloud Monitoring service.
        /// </summary>
        public static PolicyType CloudMonitoring
        {
            get
            {
                return _cloudMonitoring;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="PolicyType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override PolicyType FromName(string name)
            {
                return PolicyType.FromName(name);
            }
        }
    }
}
