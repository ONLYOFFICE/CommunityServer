namespace net.openstack.Core.Domain.Queues
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the response returned when posting messages to a queue in the <see cref="IQueueingService"/>.
    /// </summary>
    /// <seealso cref="O:net.openstack.Core.Providers.IQueueingService.PostMessagesAsync"/>
    /// <seealso href="https://wiki.openstack.org/w/index.php?title=Marconi/specs/api/v1#Post_Message.28s.29">Post Message(s) (OpenStack Marconi API v1 Blueprint)</seealso>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MessagesEnqueued : ExtensibleJsonObject
    {
        /// <summary>
        /// A default instance of the <see cref="MessagesEnqueued"/> class representing a successful
        /// operation to post an empty collection of messages.
        /// </summary>
        /// <see cref="O:net.openstack.Core.Domain.Queues.IQueueingService.PostMessagesAsync"/>
        public static MessagesEnqueued Empty = new MessagesEnqueued(false, new Uri[0]);

#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Partial"/> property.
        /// </summary>
        [JsonProperty("partial")]
        private bool? _partial;

        /// <summary>
        /// Contains a collection of message resource URIs.
        /// </summary>
        [JsonProperty("resources")]
        private Uri[] _resources;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesEnqueued"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MessagesEnqueued()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesEnqueued"/> class
        /// with the specified properties.
        /// </summary>
        /// <param name="partial">The value of the <see cref="Partial"/> property.</param>
        /// <param name="resources">A collection </param>
        /// <exception cref="ArgumentException">If <paramref name="resources"/> contains any <see langword="null"/> values.</exception>
        private MessagesEnqueued(bool? partial, IEnumerable<Uri> resources)
        {
            _partial = partial;

            if (resources != null)
            {
                _resources = resources.ToArray();
                if (_resources.Contains(null))
                    throw new ArgumentException("resources cannot contain any null values", "resources");
            }
        }

        /// <summary>
        /// Gets a value indicating whether the post operation was only partially successful.
        /// </summary>
        /// <value>
        /// <see langword="false"/> if the Post Messages operation was fully successful.
        /// <para>-or-</para>
        /// <para><see langword="true"/> if the Post Messages operation was partially successful.</para>
        /// <para>-or-</para>
        /// <para><see langword="null"/> if the JSON response from the server did not include the underlying property.</para>
        /// </value>
        public bool? Partial
        {
            get
            {
                return _partial;
            }
        }

        /// <summary>
        /// Gets the posted message IDs.
        /// </summary>
        public IEnumerable<MessageId> Ids
        {
            get
            {
                if (_resources == null || _resources.Length == 0) 
                    yield break;

                foreach (var resource in _resources)
                {
                    yield return QueuedMessage.ParseMessageId(resource);
                }
            }
        }
    }
}