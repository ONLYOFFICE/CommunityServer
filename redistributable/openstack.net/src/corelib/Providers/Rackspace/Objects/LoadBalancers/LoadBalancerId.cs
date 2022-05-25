namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique identifier of a load balancer in the <see cref="ILoadBalancerService"/>.
    /// </summary>
    /// <seealso cref="LoadBalancer.Id"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(LoadBalancerId.Converter))]
    public sealed class LoadBalancerId : ResourceIdentifier<LoadBalancerId>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerId"/> class
        /// with the specified identifier value.
        /// </summary>
        /// <param name="id">The load balancer identifier value.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public LoadBalancerId(string id)
            : base(id)
        {
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="LoadBalancerId"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override LoadBalancerId FromValue(string id)
            {
                return new LoadBalancerId(id);
            }
        }
    }
}
