namespace net.openstack.Core.Domain.Queues
{
    using Newtonsoft.Json;

    /// <summary>
    /// This models the JSON object used to represent statistics returned by the
    /// Get Queue Statistics API call.
    /// </summary>
    /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Get_Queue_Stats">Get Queue Statistics (Marconi API v1 Blueprint)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueueStatistics : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// The backing field for the <see cref="MessageStatistics"/> property.
        /// </summary>
        [JsonProperty("messages")]
        private QueueMessagesStatistics _messageStatistics;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueStatistics"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected QueueStatistics()
        {
        }

        /// <summary>
        /// Gets statistics about messages contained in the queue.
        /// </summary>
        public QueueMessagesStatistics MessageStatistics
        {
            get
            {
                return _messageStatistics;
            }
        }
    }
}
