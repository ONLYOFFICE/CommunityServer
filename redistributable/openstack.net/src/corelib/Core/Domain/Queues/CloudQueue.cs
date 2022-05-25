namespace net.openstack.Core.Domain.Queues
{
    using System;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a named queue in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <seealso cref="IQueueingService"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CloudQueue : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// The backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private QueueName _name;

        /// <summary>
        /// The backing field for the <see cref="Href"/> property.
        /// </summary>
        [JsonProperty("href")]
        private Uri _href;

        /// <summary>
        /// The backing field for the <see cref="Metadata"/> property.
        /// </summary>
        [JsonProperty("metadata")]
        private JObject _metadata;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudQueue"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CloudQueue()
        {
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        public QueueName Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the URI of the queue resource.
        /// </summary>
        public Uri Href
        {
            get
            {
                return _href;
            }
        }

        /// <summary>
        /// Gets a dynamic object containing the metadata associated with the queue.
        /// </summary>
        public JObject Metadata
        {
            get
            {
                return _metadata;
            }
        }
    }
}
