namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a job in the <see cref="IDnsService"/>.
    /// </summary>
    /// <seealso cref="DnsJob.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(JobId.Converter))]
    public sealed class JobId : ResourceIdentifier<JobId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The job identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public JobId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="JobId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override JobId FromValue(string id)
            {
                return new JobId(id);
            }
        }
    }
}
