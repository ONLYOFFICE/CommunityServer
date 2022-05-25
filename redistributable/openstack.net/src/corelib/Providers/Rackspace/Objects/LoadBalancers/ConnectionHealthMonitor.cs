namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON object used to represent a <see cref="HealthMonitor"/> for
    /// simple connection monitoring.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ConnectionHealthMonitor : HealthMonitor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHealthMonitor"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ConnectionHealthMonitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionHealthMonitor"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="attemptsBeforeDeactivation">The number of permissible monitor failures before removing a node from rotation.</param>
        /// <param name="timeout">The maximum number of seconds to wait for a connection to be established before timing out.</param>
        /// <param name="delay">The minimum time to wait before executing the health monitor.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="attemptsBeforeDeactivation"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="timeout"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="delay"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        public ConnectionHealthMonitor(int attemptsBeforeDeactivation, TimeSpan timeout, TimeSpan delay)
            : base(HealthMonitorType.Connect, attemptsBeforeDeactivation, timeout, delay)
        {
        }
    }
}
