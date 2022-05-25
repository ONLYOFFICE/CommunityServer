namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents a session persistence configuration in the load balancer service.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class SessionPersistence : ExtensibleJsonObject
    {
        /// <summary>
        /// This intermediate field is required for modeling the JSON representation of a
        /// session persistence configuration.
        /// </summary>
        [JsonProperty("sessionPersistence")]
        private SessionPersistenceBody _body;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionPersistence"/> class during
        /// JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SessionPersistence()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionPersistence"/> class using
        /// the specified persistence type.
        /// </summary>
        /// <param name="persistenceType">The session persistence mode to use.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="persistenceType"/> is <see langword="null"/>.</exception>
        public SessionPersistence(SessionPersistenceType persistenceType)
        {
            if (persistenceType == null)
                throw new ArgumentNullException("persistenceType");

            _body = new SessionPersistenceBody(persistenceType);
        }

        /// <summary>
        /// Gets the session persistence type.
        /// </summary>
        public SessionPersistenceType PersistenceType
        {
            get
            {
                if (_body == null)
                    return null;

                return _body.PersistenceType;
            }
        }

        /// <summary>
        /// This class models the JSON representation used for the body of a session persistence
        /// configuration.
        /// </summary>
        [JsonObject(MemberSerialization.OptIn)]
        protected class SessionPersistenceBody : ExtensibleJsonObject
        {
            /// <summary>
            /// This is the backing field for the <see cref="SessionPersistenceType"/> property.
            /// </summary>
            [JsonProperty("persistenceType")]
            private SessionPersistenceType _persistenceType;

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionPersistenceBody"/> class during
            /// JSON deserialization.
            /// </summary>
            [JsonConstructor]
            protected SessionPersistenceBody()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SessionPersistenceBody"/> class
            /// using the specified persistence type.
            /// </summary>
            /// <param name="persistenceType">The session persistence mode to use.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="persistenceType"/> is <see langword="null"/>.</exception>
            protected internal SessionPersistenceBody(SessionPersistenceType persistenceType)
            {
                if (persistenceType == null)
                    throw new ArgumentNullException("persistenceType");

                _persistenceType = persistenceType;
            }

            /// <summary>
            /// Gets the session persistence type.
            /// </summary>
            public SessionPersistenceType PersistenceType
            {
                get
                {
                    return _persistenceType;
                }
            }
        }
    }
}
