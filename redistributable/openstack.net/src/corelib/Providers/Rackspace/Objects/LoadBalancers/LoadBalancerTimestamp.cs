namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a timestamp object
    /// in the load balancers service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerTimestamp : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Time"/> property.
        /// </summary>
        [JsonProperty("time")]
        private DateTimeOffset? _time;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerTimestamp"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerTimestamp()
        {
        }

        /// <summary>
        /// Gets the time represented by the object.
        /// </summary>
        public DateTimeOffset? Time
        {
            get
            {
                return _time;
            }
        }
    }
}
