namespace net.openstack.Core.Domain.Queues
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON object used to represent statistics for a particular
    /// message in a message queue.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MessageStatistics : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// The backing field for the <see cref="Href"/> property.
        /// </summary>
        [JsonProperty("href")]
        private string _href;

        /// <summary>
        /// The backing field for the <see cref="Age"/> property.
        /// </summary>
        [JsonProperty("age")]
        private long _age;

        /// <summary>
        /// The backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created")]
        private string _created;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageStatistics"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MessageStatistics()
        {
        }

        /// <summary>
        /// Gets the absolute path of the URI to this message.
        /// </summary>
        public string Href
        {
            get
            {
                return _href;
            }
        }

        /// <summary>
        /// Gets the age of the message in the queue.
        /// </summary>
        public TimeSpan Age
        {
            get
            {
                return TimeSpan.FromSeconds(_age);
            }
        }

        /// <summary>
        /// Gets the timestamp when this message was first added to the queue.
        /// </summary>
        public DateTimeOffset Created
        {
            get
            {
                return JsonConvert.DeserializeObject<DateTimeOffset>(_created);
            }
        }
    }
}
