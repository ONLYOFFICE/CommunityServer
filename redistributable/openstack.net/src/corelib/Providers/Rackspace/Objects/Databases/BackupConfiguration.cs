namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of a database instance backup configuration in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="IDatabaseService.CreateBackupAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class BackupConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="InstanceId"/> property.
        /// </summary>
        [JsonProperty("instance")]
        private DatabaseInstanceId _instanceId;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Description"/> property.
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _description;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected BackupConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackupConfiguration"/> class with
        /// the specified values.
        /// </summary>
        /// <param name="instanceId">The database instance ID. This is obtained from <see cref="DatabaseInstance.Id">DatabaseInstance.Id</see>.</param>
        /// <param name="name">The name of the backup.</param>
        /// <param name="description">The optional description of the backup.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="instanceId"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="name"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public BackupConfiguration(DatabaseInstanceId instanceId, string name, string description)
        {
            if (instanceId == null)
                throw new ArgumentNullException("instanceId");
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            _instanceId = instanceId;
            _name = name;
            _description = description;
        }

        /// <summary>
        /// Gets the identifier of the database instance which should be backed up.
        /// </summary>
        public DatabaseInstanceId InstanceId
        {
            get
            {
                return _instanceId;
            }
        }

        /// <summary>
        /// Gets the name of this backup.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the description of this backup.
        /// </summary>
        public string Description
        {
            get
            {
                return _description;
            }
        }
    }
}
