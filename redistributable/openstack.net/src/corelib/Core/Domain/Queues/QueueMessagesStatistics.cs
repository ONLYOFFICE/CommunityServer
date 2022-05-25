namespace net.openstack.Core.Domain.Queues
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON object used to represent statistics for messages
    /// a particular message queue.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueueMessagesStatistics : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// The backing field for the <see cref="Free"/> property;
        /// </summary>
        [JsonProperty("free")]
        private long _free;

        /// <summary>
        /// The backing field for the <see cref="Claimed"/> property;
        /// </summary>
        [JsonProperty("claimed")]
        private long _claimed;

        /// <summary>
        /// The backing field for the <see cref="Total"/> property;
        /// </summary>
        [JsonProperty("total")]
        private long _total;

        /// <summary>
        /// The backing field for the <see cref="Oldest"/> property;
        /// </summary>
        [JsonProperty("oldest")]
        private MessageStatistics _oldest;

        /// <summary>
        /// The backing field for the <see cref="Newest"/> property;
        /// </summary>
        [JsonProperty("newest")]
        private MessageStatistics _newest;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueMessagesStatistics"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected QueueMessagesStatistics()
        {
        }

        /// <summary>
        /// Gets the number of unclaimed messages in the queue.
        /// </summary>
        public long Free
        {
            get
            {
                return _free;
            }
        }

        /// <summary>
        /// Gets the number of claimed messages in the queue.
        /// </summary>
        public long Claimed
        {
            get
            {
                return _claimed;
            }
        }

        /// <summary>
        /// Gets the total number of messages currently in the queue.
        /// </summary>
        public long Total
        {
            get
            {
                return _total;
            }
        }

        /// <summary>
        /// Gets additional statistics for the oldest message in the queue.
        /// </summary>
        /// <value>
        /// A <see cref="MessageStatistics"/> object containing statistics about the oldest message in the queue,
        /// or <see langword="null"/> if <see cref="Total"/> is 0.
        /// </value>
        public MessageStatistics Oldest
        {
            get
            {
                return _oldest;
            }
        }

        /// <summary>
        /// Gets additional statistics for the newest message in the queue.
        /// </summary>
        /// <value>
        /// A <see cref="MessageStatistics"/> object containing statistics about the newest message in the queue,
        /// or <see langword="null"/> if <see cref="Total"/> is 0.
        /// </value>
        public MessageStatistics Newest
        {
            get
            {
                return _newest;
            }
        }
    }
}
