namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class provides a base class for configuring checks that connect to a
    /// service on a configurable port.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class ConnectionCheckDetails : CheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Port"/> property.
        /// </summary>
        [JsonProperty("port", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ConnectionCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionCheckDetails"/> class
        /// with the specified port.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port for the associated service should be used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        protected ConnectionCheckDetails(int? port)
        {
            if (port <= 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");

            _port = port;
        }

        /// <summary>
        /// Gets the port to use for remote monitoring connections.
        /// </summary>
        public int? Port
        {
            get
            {
                return _port;
            }
        }
    }
}
