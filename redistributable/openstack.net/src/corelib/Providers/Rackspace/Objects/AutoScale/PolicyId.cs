namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a scaling policy in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="Policy.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(PolicyId.Converter))]
    public sealed class PolicyId : ResourceIdentifier<PolicyId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The scaling group identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public PolicyId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="PolicyId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override PolicyId FromValue(string id)
            {
                return new PolicyId(id);
            }
        }
    }
}
