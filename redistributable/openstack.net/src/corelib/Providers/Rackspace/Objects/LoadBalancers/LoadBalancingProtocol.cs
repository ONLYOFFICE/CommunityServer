namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This models the JSON object representing a load balancing protocol.
    /// </summary>
    /// <seealso cref="ILoadBalancerService.ListProtocolsAsync"/>
    /// <seealso href="http://docs.rackspace.com/loadbalancers/api/v1.0/clb-devguide/content/List_Load_Balancing_Protocols-d1e4269.html">List Load Balancing Protocols (Rackspace Cloud Load Balancers Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancingProtocol : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Port"/> property.
        /// </summary>
        [JsonProperty("port")]
        private int? _port;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancingProtocol"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancingProtocol()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancingProtocol"/> class
        /// using the specified name and port number.
        /// </summary>
        /// <param name="name">The protocol name.</param>
        /// <param name="port">The default port number for the protocol, or 0 if no default port is defined for the protocol.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than 0 or greated than 65535.</exception>
        public LoadBalancingProtocol(string name, int port)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("port");

            _name = name;
            _port = port;
        }

        /// <summary>
        /// Gets the name of the load balancing protocol.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the port for the load balancing protocol.
        /// </summary>
        /// <value>
        /// The default port number used for the protocol, or <see langword="null"/> if the JSON response from
        /// the server did not include the underlying property. If the value is 0, no default port
        /// is defined for the protocol.
        /// </value>
        public int? Port
        {
            get
            {
                return _port;
            }
        }
    }
}
