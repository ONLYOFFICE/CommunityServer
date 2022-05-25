namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents an entity in the <see cref="IMonitoringService"/>.
    /// </summary>
    /// <remarks>
    /// An entity is the target of what you are monitoring. For example, you can create an entity
    /// to monitor your website, a particular web service, or your Rackspace server or server
    /// instance. Note that an entity represents only one item in the monitoring system. For
    /// example, if you wanted to monitor each server in a cluster, you would create an entity
    /// for each of the servers. You would not create a single entity to represent the entire
    /// cluster.
    ///
    /// <para>
    /// An entity can have multiple checks associated with it. This allows you to check multiple
    /// services on the same host by creating multiple checks on the same entity, instead of
    /// multiple entities each with a single check.
    /// </para>
    ///
    /// <para>
    /// When you create a new entity in the monitoring system, you specify the following
    /// parameters:
    /// </para>
    ///
    /// <list type="bullet">
    /// <item>A meaningful name for the entity</item>
    /// <item>The IP address(es) for the entity (optional)</item>
    /// <item>The metadata that the monitoring system uses if an alarm is triggered (optional)</item>
    /// </list>
    /// </remarks>
    /// <see href="http://docs.rackspace.com/cm/api/v1.0/cm-devguide/content/service-entities.html">Entities (Rackspace Cloud Monitoring Developer Guide - API v1.0)</see>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Entity : EntityConfiguration
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private EntityId _id;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Entity"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Entity()
        {
        }

        /// <summary>
        /// Gets the unique identifier of this entity.
        /// </summary>
        /// <value>
        /// An <see cref="EntityId"/> object containing the unique identifier of the entity,
        /// or <see langword="null"/> if the JSON response from the server did not contain the underlying
        /// property.
        /// </value>
        public EntityId Id
        {
            get
            {
                return _id;
            }
        }
    }
}
