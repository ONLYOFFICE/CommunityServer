namespace net.openstack.Core.Domain.Queues
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Represents a queue message with a dynamic <see cref="JObject"/> body.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Message : Message<JObject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Message()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class using
        /// the specified time-to-live and message body.
        /// </summary>
        /// <param name="timeToLive">The time-to-live for the message.</param>
        /// <param name="body">The message data.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="timeToLive"/> is negative or <see cref="TimeSpan.Zero"/>.</exception>
        public Message(TimeSpan timeToLive, JObject body)
            : base(timeToLive, body)
        {
        }
    }
}
