namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a load balancer usage record in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="LoadBalancerUsage.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(LoadBalancerUsageId.Converter))]
    public sealed class LoadBalancerUsageId : ResourceIdentifier<LoadBalancerUsageId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerUsageId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The load balancer usage record identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public LoadBalancerUsageId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="LoadBalancerUsageId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override LoadBalancerUsageId FromValue(string id)
            {
                return new LoadBalancerUsageId(id);
            }
        }
    }
}
