namespace net.openstack.Providers.Rackspace.Objects.Dns
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a nameserver associated with a domain
    /// in the DNS service.
    /// </summary>
    /// <seealso cref="DnsDomain.Nameservers"/>
    /// <seealso href="http://docs.rackspace.com/cdns/api/v1.0/cdns-devguide/content/list_domain_details.html">List Domain Details (Rackspace Cloud DNS Developer Guide - API v1.0)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DnsNameserver : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsNameserver"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DnsNameserver()
        {
        }

        /// <summary>
        /// Gets the fully-qualified domain name of the nameserver.
        /// </summary>
        /// <value>
        /// The fully-qualified domain name of the nameserver, or <see langword="null"/> if the JSON
        /// response from the server did not include this property.
        /// </value>
        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
