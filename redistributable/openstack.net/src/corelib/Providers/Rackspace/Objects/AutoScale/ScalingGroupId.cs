namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a scaling group in the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <seealso cref="ScalingGroup.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(ScalingGroupId.Converter))]
    public sealed class ScalingGroupId : ResourceIdentifier<ScalingGroupId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalingGroupId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The scaling group identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public ScalingGroupId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="ScalingGroupId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override ScalingGroupId FromValue(string id)
            {
                return new ScalingGroupId(id);
            }
        }
    }
}
