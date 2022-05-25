namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a node service event in the load balancer service.
    /// </summary>
    /// <see cref="ILoadBalancerService.ListNodeServiceEventsAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class NodeServiceEvent : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private NodeServiceEventId _id;

        /// <summary>
        /// This is the backing field for the <see cref="LoadBalancerId"/> property.
        /// </summary>
        [JsonProperty("loadBalancerId")]
        private LoadBalancerId _loadBalancerId;

        /// <summary>
        /// This is the backing field for the <see cref="NodeId"/> property.
        /// </summary>
        [JsonProperty("nodeId")]
        private NodeId _nodeId;

        /// <summary>
        /// This is the backing field for the <see cref="DetailedMessage"/> property.
        /// </summary>
        [JsonProperty("detailedMessage")]
        private string _detailedMessage;

        /// <summary>
        /// This is the backing field for the <see cref="Type"/> property.
        /// </summary>
        [JsonProperty("type")]
        private NodeServiceEventType _type;

        /// <summary>
        /// This is the backing field for the <see cref="Category"/> property.
        /// </summary>
        [JsonProperty("category")]
        private NodeServiceEventCategory _category;

        /// <summary>
        /// This is the backing field for the <see cref="Severity"/> property.
        /// </summary>
        [JsonProperty("severity")]
        private NodeServiceEventSeverity _severity;

        /// <summary>
        /// This is the backing field for the <see cref="Description"/> property.
        /// </summary>
        [JsonProperty("description")]
        private string _description;

        /// <summary>
        /// This is the backing field for the <see cref="RelativeUri"/> property.
        /// </summary>
        [JsonProperty("relativeUri")]
        private string _relativeUri;

        /// <summary>
        /// This is the backing field for the <see cref="AccountId"/> property.
        /// </summary>
        [JsonProperty("accountId")]
        private ProjectId _accountId;

        /// <summary>
        /// This is the backing field for the <see cref="Title"/> property.
        /// </summary>
        [JsonProperty("title")]
        private string _title;

        /// <summary>
        /// This is the backing field for the <see cref="Author"/> property.
        /// </summary>
        [JsonProperty("author")]
        private string _author;

        /// <summary>
        /// This is the backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created")]
        private DateTimeOffset? _created;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeServiceEvent"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected NodeServiceEvent()
        {
        }

        /// <summary>
        /// Gets the unique ID for this node service event record.
        /// </summary>
        public NodeServiceEventId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the load balancer ID.
        /// </summary>
        /// <seealso cref="LoadBalancer.Id"/>
        public LoadBalancerId LoadBalancerId
        {
            get
            {
                return _loadBalancerId;
            }
        }

        /// <summary>
        /// Gets the load balancer node ID.
        /// </summary>
        /// <seealso cref="Node.Id"/>
        public NodeId NodeId
        {
            get
            {
                return _nodeId;
            }
        }

        /// <summary>
        /// Gets the detailed message describing the service event.
        /// </summary>
        public string DetailedMessage
        {
            get
            {
                return _detailedMessage;
            }
        }

        /// <summary>
        /// Gets the service event type.
        /// </summary>
        public NodeServiceEventType Type
        {
            get
            {
                return _type;
            }
        }

        /// <summary>
        /// Gets the service event category.
        /// </summary>
        public NodeServiceEventCategory Category
        {
            get
            {
                return _category;
            }
        }

        /// <summary>
        /// Gets the service event severity.
        /// </summary>
        public NodeServiceEventSeverity Severity
        {
            get
            {
                return _severity;
            }
        }

        /// <summary>
        /// Gets a description of the service event.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Gets <placeholder>what?</placeholder>
        /// </summary>
        public Uri RelativeUri
        {
            get
            {
                if (_relativeUri == null)
                    return null;

                return new Uri(_relativeUri);
            }
        }

        /// <summary>
        /// Gets the account ID associated with this service event. The account ID within
        /// the load balancer service is equivalent to the <see cref="Tenant.Id">Tenant.Id</see>
        /// referenced by other services.
        /// </summary>
        /// <value>
        /// The account ID for the node service event, or <see langword="null"/> if the JSON response from the server
        /// did not include this property.
        /// </value>
        public ProjectId AccountId
        {
            get
            {
                return _accountId;
            }
        }

        /// <summary>
        /// Gets the title of the service event.
        /// </summary>
        public string Title
        {
            get
            {
                return _title;
            }
        }

        /// <summary>
        /// Gets the author responsible for this service event.
        /// </summary>
        public string Author
        {
            get
            {
                return _author;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when this service event was created.
        /// </summary>
        public DateTimeOffset? Created
        {
            get
            {
                return _created;
            }
        }
    }
}
