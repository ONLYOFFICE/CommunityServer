namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents the configuration for a new database in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="IDatabaseService.CreateDatabaseAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DatabaseConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private DatabaseName _name;

        /// <summary>
        /// This is the backing field for the <see cref="CharacterSet"/> property.
        /// </summary>
        [JsonProperty("character_set", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _characterSet;

        /// <summary>
        /// This is the backing field for the <see cref="Collate"/> property.
        /// </summary>
        [JsonProperty("collate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _collate;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DatabaseConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfiguration"/> class with
        /// the specified name and the default MySQL character set and collation configuration.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        public DatabaseConfiguration(DatabaseName name)
            : this(name, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfiguration"/> class with
        /// the specified name, character set, and collation configuration.
        /// </summary>
        /// <param name="name">The name of the database.</param>
        /// <param name="characterSet">The MySQL character set. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="collate">The MySQL collation configuration. If the value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        public DatabaseConfiguration(DatabaseName name, string characterSet, string collate)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            _name = name;
            _characterSet = characterSet;
            _collate = collate;
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public DatabaseName Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the MySQL character set used by the database.
        /// </summary>
        public string CharacterSet
        {
            get
            {
                return _characterSet;
            }
        }

        /// <summary>
        /// Gets the MySQL collation configuration used by the database.
        /// </summary>
        public string Collate
        {
            get
            {
                return _collate;
            }
        }
    }
}
