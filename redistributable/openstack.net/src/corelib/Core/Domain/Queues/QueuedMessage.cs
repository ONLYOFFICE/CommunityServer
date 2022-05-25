namespace net.openstack.Core.Domain.Queues
{
    using System;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using net.openstack.Core.Providers;

    /// <summary>
    /// Represents a message which is queued in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class QueuedMessage : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// The backing field for the <see cref="Href"/> property.
        /// </summary>
        [JsonProperty("href")]
        private Uri _href;

        /// <summary>
        /// The backing field for the <see cref="TimeToLive"/> property.
        /// The value is stored in seconds.
        /// </summary>
        [JsonProperty("ttl")]
        private long _ttl;

        /// <summary>
        /// The backing field for the <see cref="Age"/> property.
        /// The value is stored in seconds.
        /// </summary>
        [JsonProperty("age")]
        private long _age;

        /// <summary>
        /// The backing field for the <see cref="Body"/> property.
        /// </summary>
        [JsonProperty("body")]
        private JObject _body;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="QueuedMessage"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected QueuedMessage()
        {
        }

        /// <summary>
        /// Gets the ID of the message.
        /// </summary>
        public MessageId Id
        {
            get
            {
                if (Href == null)
                    return null;

                return ParseMessageId(Href);
            }
        }

        /// <summary>
        /// Gets the URI of the message resource.
        /// </summary>
        public Uri Href
        {
            get
            {
                return _href;
            }
        }

        /// <summary>
        /// Gets the time-to-live of the message.
        /// </summary>
        public TimeSpan TimeToLive
        {
            get
            {
                return TimeSpan.FromSeconds(_ttl);
            }
        }

        /// <summary>
        /// Gets the age of the message.
        /// </summary>
        public TimeSpan Age
        {
            get
            {
                return TimeSpan.FromSeconds(_age);
            }
        }

        /// <summary>
        /// Gets the JSON body of the message as a dynamic <see cref="JObject"/>.
        /// </summary>
        public JObject Body
        {
            get
            {
                return _body;
            }
        }

        /// <summary>
        /// Parses a URI to extract a <see cref="MessageId"/>.
        /// </summary>
        /// <param name="href">The resource URI.</param>
        /// <returns>The ID of the message.</returns>
        public static MessageId ParseMessageId(Uri href)
        {
            // make sure we have an absolute URI, or Segments will throw an InvalidOperationException
            if (!href.IsAbsoluteUri)
                href = new Uri(new Uri("http://example.com"), href);

            if (href.Segments.Length == 0)
                return null;

            return new MessageId(href.Segments.Last());
        }
    }
}
