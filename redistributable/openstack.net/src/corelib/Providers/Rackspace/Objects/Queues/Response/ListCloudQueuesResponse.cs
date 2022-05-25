namespace net.openstack.Providers.Rackspace.Objects.Queues.Response
{
    using net.openstack.Core.Domain;
    using net.openstack.Core.Domain.Queues;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListCloudQueuesResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        [JsonProperty("links")]
        private Link[] _links;

        [JsonProperty("queues")]
        private CloudQueue[] _queues;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCloudQueuesResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListCloudQueuesResponse()
        {
        }

        public Link[] Links
        {
            get
            {
                return _links;
            }
        }

        public CloudQueue[] Queues
        {
            get
            {
                return _queues;
            }
        }
    }
}
