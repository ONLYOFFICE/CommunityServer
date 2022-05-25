namespace net.openstack.Providers.Rackspace.Objects.Queues.Response
{
    using System.Collections.ObjectModel;
    using net.openstack.Core.Domain;
    using net.openstack.Core.Domain.Queues;
    using Newtonsoft.Json;

    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    internal class ListCloudQueueMessagesResponse
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        [JsonProperty("links")]
        private Link[] _links;

        [JsonProperty("messages")]
        private QueuedMessage[] _messages;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="ListCloudQueueMessagesResponse"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ListCloudQueueMessagesResponse()
        {
        }

        public ReadOnlyCollection<Link> Links
        {
            get
            {
                if (_links == null)
                    return null;

                return new ReadOnlyCollection<Link>(_links);
            }
        }

        public ReadOnlyCollection<QueuedMessage> Messages
        {
            get
            {
                if (_messages == null)
                    return null;

                return new ReadOnlyCollection<QueuedMessage>(_messages);
            }
        }
    }
}
