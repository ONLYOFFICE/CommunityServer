namespace net.openstack.Core.Domain
{
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a map of network labels to collections of IP addresses.
    /// </summary>
    /// <remarks>
    /// The keys of this collection are network labels (see <see cref="CloudNetwork.Label"/>),
    /// and the values are collections of IP addresses.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    [JsonDictionary]
    public class ServerAddresses : Dictionary<string, IPAddressList>
    {
        /// <summary>
        /// Gets the server IP addresses associated with the <c>private</c> network.
        /// </summary>
        [JsonIgnore]
        public IList<IPAddress> Private
        {
            get
            {
                IPAddressList result;
                if (!TryGetValue("private", out result))
                    return null;

                return result;
            }
        }

        /// <summary>
        /// Gets the server IP addresses associated with the <c>public</c> network.
        /// </summary>
        [JsonIgnore]
        public IList<IPAddress> Public
        {
            get
            {
                IPAddressList result;
                if (!TryGetValue("public", out result))
                    return null;

                return result;
            }
        }
    }
}
