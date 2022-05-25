namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a database instance backup in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Backup : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Id"/> property.
        /// </summary>
        [JsonProperty("id")]
        private BackupId _id;

        /// <summary>
        /// This is the backing field for the <see cref="InstanceId"/> property.
        /// </summary>
        [JsonProperty("instance_id")]
        private DatabaseInstanceId _instanceId;

        /// <summary>
        /// This is the backing field for the <see cref="LocationRef"/> property.
        /// </summary>
        [JsonProperty("locationRef")]
        private string _locationRef;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Description"/> property.
        /// </summary>
        [JsonProperty("description")]
        private string _description;

        /// <summary>
        /// This is the backing field for the <see cref="Status"/> property.
        /// </summary>
        [JsonProperty("status")]
        private BackupStatus _status;

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
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="Backup"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Backup()
        {
        }

        /// <summary>
        /// Gets the unique identifier for this database backup.
        /// </summary>
        public BackupId Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the unique identifier of the database instance associated with this backup.
        /// </summary>
        public DatabaseInstanceId InstanceId
        {
            get
            {
                return _instanceId;
            }
        }

        /// <summary>
        /// Gets the name of this database backup.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the description associated with this database backup.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        /// <summary>
        /// Gets the location of this database backup.
        /// </summary>
        public Uri LocationRef
        {
            get
            {
                if (_locationRef == null)
                    return null;

                return new Uri(_locationRef);
            }
        }

        /// <summary>
        /// Gets the status of this database backup.
        /// </summary>
        public BackupStatus Status
        {
            get
            {
                return _status;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when this backup was first created.
        /// </summary>
        public DateTimeOffset? Created
        {
            get
            {
                return _created;
            }
        }

        /// <summary>
        /// Gets a timestamp indicating when this backup was last modified.
        /// </summary>
        public DateTimeOffset? Updated
        {
            get
            {
                return _updated;
            }
        }
    }
}
