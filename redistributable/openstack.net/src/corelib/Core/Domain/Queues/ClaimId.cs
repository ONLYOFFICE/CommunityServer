namespace net.openstack.Core.Domain.Queues
{
    using System;
    using Newtonsoft.Json;
    using net.openstack.Core.Providers;

    /// <summary>
    /// Represents the unique identifier of a claim in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <seealso cref="Claim.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(ClaimId.Converter))]
    public sealed class ClaimId : ResourceIdentifier<ClaimId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The claim identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public ClaimId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ClaimId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ClaimId FromValue(string id)
            {
                return new ClaimId(id);
            }
        }
    }
}
