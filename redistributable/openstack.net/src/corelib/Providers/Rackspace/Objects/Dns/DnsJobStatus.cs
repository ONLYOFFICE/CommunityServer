namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the status of a DNS job.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known statuses,
    /// with added support for unknown statuses returned by a server extension.
    /// </remarks>
    /// <seealso cref="DnsJob.Status"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(DnsJobStatus.Converter))]
    public sealed class DnsJobStatus : ExtensibleEnum<DnsJobStatus>
    {
        private static readonly ConcurrentDictionary<string, DnsJobStatus> _types =
            new ConcurrentDictionary<string, DnsJobStatus>(StringComparer.OrdinalIgnoreCase);
        private static readonly DnsJobStatus _initialized = FromName("INITIALIZED");
        private static readonly DnsJobStatus _running = FromName("RUNNING");
        private static readonly DnsJobStatus _completed = FromName("COMPLETED");
        private static readonly DnsJobStatus _error = FromName("ERROR");

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsJobStatus"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private DnsJobStatus(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="DnsJobStatus"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="DnsJobStatus"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static DnsJobStatus FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new DnsJobStatus(i));
        }

        /// <summary>
        /// Gets a <see cref="DnsJobStatus"/> representing a job which has been accepted by the DNS service but has not yet started.
        /// </summary>
        public static DnsJobStatus Initialized
        {
            get
            {
                return _initialized;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsJobStatus"/> representing a job which is currently running.
        /// </summary>
        public static DnsJobStatus Running
        {
            get
            {
                return _running;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsJobStatus"/> representing a job which completed successfully.
        /// </summary>
        public static DnsJobStatus Completed
        {
            get
            {
                return _completed;
            }
        }

        /// <summary>
        /// Gets a <see cref="DnsJobStatus"/> representing a job which terminated with an error.
        /// </summary>
        public static DnsJobStatus Error
        {
            get
            {
                return _error;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="DnsJobStatus"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override DnsJobStatus FromName(string name)
            {
                return DnsJobStatus.FromName(name);
            }
        }
    }
}
