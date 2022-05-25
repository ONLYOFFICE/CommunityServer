namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON object used to describe a new database instance in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="IDatabaseService.CreateDatabaseInstanceAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DatabaseInstanceConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="FlavorRef"/> property.
        /// </summary>
        [JsonProperty("flavorRef", DefaultValueHandling = DefaultValueHandling.Include)]
        private FlavorRef _flavorRef;

        /// <summary>
        /// This is the backing field for the <see cref="Volume"/> property.
        /// </summary>
        [JsonProperty("volume", DefaultValueHandling = DefaultValueHandling.Include)]
        private DatabaseVolumeConfiguration _volume;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="RestorePoint"/> property.
        /// </summary>
        [JsonProperty("restorePoint", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private RestorePoint _restorePoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInstanceConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DatabaseInstanceConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInstanceConfiguration"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="flavorRef">A <see cref="FlavorRef"/> object describing the flavor of the database instance.</param>
        /// <param name="volumeConfiguration">A <see cref="DatabaseVolumeConfiguration"/> object containing additional information about
        /// the database instance storage volume.</param>
        /// <param name="name">The name of the database instance, or <see langword="null"/> if the database instance is not named.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="flavorRef"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeConfiguration"/> is <see langword="null"/>.</para>
        /// </exception>
        public DatabaseInstanceConfiguration(FlavorRef flavorRef, DatabaseVolumeConfiguration volumeConfiguration, string name)
            : this(flavorRef, volumeConfiguration, name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseInstanceConfiguration"/> class
        /// with a restore point for restoring a database instance from a backup.
        /// </summary>
        /// <param name="flavorRef">A <see cref="FlavorRef"/> object describing the flavor of the database instance.</param>
        /// <param name="volumeConfiguration">A <see cref="DatabaseVolumeConfiguration"/> object containing additional information about
        /// the database instance storage volume.</param>
        /// <param name="name">The name of the database instance, or <see langword="null"/> if the database instance is not named.</param>
        /// <param name="restorePoint">A <see cref="RestorePoint"/> object describing the backup from which this database instance was restored, or null if the restore point is not available.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="flavorRef"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="volumeConfiguration"/> is <see langword="null"/>.</para>
        /// </exception>
        public DatabaseInstanceConfiguration(FlavorRef flavorRef, DatabaseVolumeConfiguration volumeConfiguration, string name, RestorePoint restorePoint)
        {
            if (flavorRef == null)
                throw new ArgumentNullException("flavorRef");
            if (volumeConfiguration == null)
                throw new ArgumentNullException("volumeConfiguration");

            _flavorRef = flavorRef;
            _volume = volumeConfiguration;
            _name = name;
            _restorePoint = restorePoint;
        }

        /// <summary>
        /// Gets a <see cref="FlavorRef"/> object describing the flavor of the database instance.
        /// </summary>
        public FlavorRef FlavorRef
        {
            get
            {
                return _flavorRef;
            }
        }

        /// <summary>
        /// Gets a <see cref="DatabaseVolumeConfiguration"/> object containing additional information about
        /// the database instance storage volume.
        /// </summary>
        public DatabaseVolumeConfiguration Volume
        {
            get
            {
                return _volume;
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
        /// Gets a <see cref="RestorePoint"/> object describing the backup from which this database instance was restored.
        /// </summary>
        public RestorePoint RestorePoint
        {
            get
            {
                return _restorePoint;
            }
        }
    }
}
