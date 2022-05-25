namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers.Response
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListAllowedDomainsResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        [JsonProperty("allowedDomains")]
        private AllowedDomain[] _allowedDomains;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListAllowedDomainsResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListAllowedDomainsResponse()
        {
        }

        public IEnumerable<string> AllowedDomains
        {
            get
            {
                if (_allowedDomains == null)
                    return null;

                return _allowedDomains.Select(i => i.Name);
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        protected class AllowedDomain
        {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
            [JsonProperty("allowedDomain")]
            private AllowedDomainDescriptor _allowedDomain;
#pragma warning restore 649

            /// <summary>
            /// Initializes a new instance of the <see cref="AllowedDomain"/> class
            /// during JSON deserialization.
            /// </summary>
            [JsonConstructor]
            protected AllowedDomain()
            {
            }

            public string Name
            {
                get
                {
                    if (_allowedDomain == null)
                        return null;

                    return _allowedDomain.Name;
                }
            }

            [JsonObject(MemberSerialization.OptIn)]
            protected class AllowedDomainDescriptor
            {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
                [JsonProperty("name")]
                private string _name;
#pragma warning restore 649

                /// <summary>
                /// Initializes a new instance of the <see cref="AllowedDomainDescriptor"/> class
                /// during JSON deserialization.
                /// </summary>
                [JsonConstructor]
                protected AllowedDomainDescriptor()
                {
                }

                public string Name
                {
                    get
                    {
                        return _name;
                    }
                }
            }
        }
    }
}
