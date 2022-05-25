namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using System.Collections.ObjectModel;
    using net.openstack.Core.Domain;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a database instance within the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DatabaseInstance : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private DatabaseInstanceId _id;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private DatabaseInstanceStatus _status;

        /// <summary>
        /// This is the backing field for the <see cref="VolumeConfiguration"/> property.
        /// </summary>
        [JsonProperty("volume")]
        private DatabaseVolumeConfiguration _volumeStatistics;

        /// <summary>
        /// This is the backing field for the <see cref="HostName"/> property.
        /// </summary>
        [JsonProperty("hostname")]
        private string _hostName;

        /// <summary>
        /// This is the backing field for the <see cref="Flavor"/> property.
        /// </summary>
        [JsonProperty("flavor")]
        private DatabaseFlavor _flavor;

        /// <summary>
        /// This is the backing field for the <see cref="Created"/> property.
        /// </summary>
        [JsonProperty("created")]
        private DateTimeOffset? _created;

        /// <summary>
        /// This is the backing field for the <see cref="Updated"/> property.
        /// </summary>
        [JsonProperty("updated")]
        private DateTimeOffset? _updated;

        /// <summary>
        /// This is the backing field for the <see cref="Links"/> property.
        /// </summary>
        [JsonProperty("links")]
        private Link[] _links;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInstance"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DatabaseInstance()
        {
        }

        /// <summary>
        /// Gets the unique identifier for the database instance.
        /// </summary>
        public DatabaseInstanceId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the name of the database instance.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the current status of the database instance.
        /// </summary>
        public DatabaseInstanceStatus Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets the volume configuration for the database instance.
        /// </summary>
        public DatabaseVolumeConfiguration VolumeConfiguration
        {
            get
            {
                return _volumeStatistics;
            }
        }

        /// <summary>
        /// Gets the host name for the database instance.
        /// </summary>
        public string HostName
        {
            get
            {
                return _hostName;
            }
        }

        /// <summary>
        /// Gets the flavor for the database instance.
        /// </summary>
        public DatabaseFlavor Flavor
        {
            get
            {
                return _flavor;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when this database instance was created.
        /// </summary>
        public DateTimeOffset? Created
        {
            get
            {
                return _created;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when this database instance was last updated.
        /// </summary>
        public DateTimeOffset? Updated
        {
            get
            {
                return _updated;
            }
        }

        /// <summary>
        /// Gets a collection of links describing resources related to this database instance.
        /// </summary>
        public ReadOnlyCollection<Link> Links
        {
            get
            {
                if (_links == null)
                    return null;

                return new ReadOnlyCollection<Link>(_links);
            }
        }
    }
}
