namespace net.openstack.Providers.Rackspace.Objects.AutoScale
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// Represents the launch arguments for a <see cref="ServerLaunchConfiguration"/> in
    /// the <see cref="IAutoScaleService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ServerLaunchArguments : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="LoadBalancers"/> property.
        /// </summary>
        [JsonProperty("loadBalancers", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private LoadBalancerArgument[] _loadBalancers;

        /// <summary>
        /// This is the backing field for the <see cref="Server"/> property.
        /// </summary>
        [JsonProperty("server", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private ServerArgument _server;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerLaunchArguments"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ServerLaunchArguments()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerLaunchArguments"/> class
        /// with the specified server argument.
        /// </summary>
        /// <param name="server">A <see cref="ServerArgument"/> object containing the detailed arguments for launching a server.</param>
        public ServerLaunchArguments(ServerArgument server)
            : this(server, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerLaunchArguments"/> class
        /// with the specified server argument and collection of load balancers to initially
        /// add created servers to.
        /// </summary>
        /// <param name="server">A <see cref="ServerArgument"/> object containing the detailed arguments for launching a server.</param>
        /// <param name="loadBalancers">A collection of <see cref="LoadBalancerArgument"/> objects describing the load balancers to initially add created servers to.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="server"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="loadBalancers"/> contains any <see langword="null"/> values.</exception>
        public ServerLaunchArguments(ServerArgument server, IEnumerable<LoadBalancerArgument> loadBalancers)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            _server = server;
            if (loadBalancers != null)
            {
                _loadBalancers = loadBalancers.ToArray();
                if (_loadBalancers.Contains(null))
                    throw new ArgumentException("loadBalancers cannot contain any null values", "loadBalancers");
            }
        }

        /// <summary>
        /// Gets a <see cref="ServerArgument"/> object describing the launch arguments for the server.
        /// </summary>
        public ServerArgument Server
        {
            get
            {
                return _server;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="LoadBalancerArgument"/> objects describing the load balancers to
        /// initially add the created server to.
        /// </summary>
        /// <remarks>
        /// All servers are added to these load balancers with the IP addresses of their ServiceNet network.
        /// All servers are enabled and equally weighted. Any new servers that are not connected to the
        /// ServiceNet network are not added to any load balancers.
        /// </remarks>
        public ReadOnlyCollection<LoadBalancerArgument> LoadBalancers
        {
            get
            {
                if (_loadBalancers == null)
                    return null;

                return new ReadOnlyCollection<LoadBalancerArgument>(_loadBalancers);
            }
        }
    }
}
