namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class describes the configuration for a new database user in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="IDatabaseService.CreateUserAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is one of the backing fields for the <see cref="UserName"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Password"/> property.
        /// </summary>
        [JsonProperty("password")]
        private string _password;

        /// <summary>
        /// This is one of the backing fields for the <see cref="UserName"/> property.
        /// </summary>
        [JsonProperty("host", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _host;

        /// <summary>
        /// This is one of the backing fields for the <see cref="Databases"/> property.
        /// </summary>
        [JsonProperty("database", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private DatabaseName _database;

        /// <summary>
        /// This is one of the backing fields for the <see cref="Databases"/> property.
        /// </summary>
        [JsonProperty("databases", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private WrappedDatabaseName[] _databases;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected UserConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfiguration"/> class
        /// with the specified user name, password, and databases to initially grant
        /// access to.
        /// </summary>
        /// <param name="name">A <see cref="UserName"/> object describing the name and host of the user to add.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="databases">A collection of <see cref="DatabaseName"/> objects identifying the databases to initially grant access to for the new user.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="password"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="databases"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        public UserConfiguration(UserName name, string password, IEnumerable<DatabaseName> databases)
            : this(name, password, databases != null ? databases.ToArray() : null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConfiguration"/> class
        /// with the specified user name, password, and databases to initially grant
        /// access to.
        /// </summary>
        /// <param name="name">A <see cref="UserName"/> object describing the name and host of the user to add.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="databases">A collection of <see cref="DatabaseName"/> objects identifying the databases to initially grant access to for the new user.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="name"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="password"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="password"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="databases"/> contains any <see langword="null"/> values.</para>
        /// </exception>
        public UserConfiguration(UserName name, string password, params DatabaseName[] databases)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("password cannot be empty");
            if (databases != null && databases.Contains(null))
                throw new ArgumentException("databases cannot contain any null values", "databases");

            _name = name.Name;
            _password = password;
            _host = name.Host;
            if (databases != null && databases.Length > 0)
            {
                if (databases.Length == 1)
                    _database = databases[0];
                else
                    _databases = Array.ConvertAll(databases, i => new WrappedDatabaseName(i));
            }
        }

        /// <summary>
        /// Gets a <see cref="UserName"/> object describing the username and host of the user.
        /// </summary>
        public UserName UserName
        {
            get
            {
                if (_host == null)
                    return new UserName(_name);

                return new UserName(_name, _host);
            }
        }

        /// <summary>
        /// Gets the password for the user.
        /// </summary>
        public string Password
        {
            get
            {
                return _password;
            }
        }

        /// <summary>
        /// Gets a collection of <see cref="DatabaseName"/> objects identifying the databases to
        /// initially grant access to.
        /// </summary>
        public ReadOnlyCollection<DatabaseName> Databases
        {
            get
            {
                if (_databases != null)
                {
                    return new ReadOnlyCollection<DatabaseName>(Array.ConvertAll(_databases, i => i.Name));
                }
                else if (_database != null)
                {
                    return new ReadOnlyCollection<DatabaseName>(new[] { _database });
                }

                return new ReadOnlyCollection<DatabaseName>(new DatabaseName[0]);
            }
        }

        /// <summary>
        /// This class models the JSON representation of a database name in the Create User API call.
        /// </summary>
        /// <seealso href="http://docs.rackspace.com/cdb/api/v1.0/cdb-devguide/content/POST_createUser__version___accountId__instances__instanceId__users_.html">Create User (Rackspace Cloud Databases Developer Guide - API v1.0)</seealso>
        [JsonObject(MemberSerialization.OptIn)]
        protected class WrappedDatabaseName : ExtensibleJsonObject
        {
            /// <summary>
            /// This is the backing field for the <see cref="Name"/> property.
            /// </summary>
            [JsonProperty("name")]
            private DatabaseName _name;

            /// <summary>
            /// Initializes a new instance of the <see cref="WrappedDatabaseName"/> class
            /// with the specified name.
            /// </summary>
            /// <param name="name">The database name.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
            public WrappedDatabaseName(DatabaseName name)
            {
                if (name == null)
                    throw new ArgumentNullException("name");

                _name = name;
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
        }
    }
}
