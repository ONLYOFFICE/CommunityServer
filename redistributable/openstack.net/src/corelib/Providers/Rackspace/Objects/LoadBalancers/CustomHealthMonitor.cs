namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON object used to represent a custom <see cref="HealthMonitor"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CustomHealthMonitor : HealthMonitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CustomHealthMonitor"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CustomHealthMonitor()
        {
        }
    }
}
