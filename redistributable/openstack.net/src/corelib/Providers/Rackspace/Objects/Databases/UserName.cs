namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using System.Net;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the unique name of a user in the <see cref="IDatabaseService"/>.
    /// </summary>
    /// <seealso cref="UserConfiguration.UserName"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(UserName.Converter))]
    public sealed class UserName : ResourceIdentifier<UserName>
    {
        private readonly string _name;

        private readonly string _host;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserName"/> class
        /// with the specified name.
        /// </summary>
        /// <param name="id">The database name.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public UserName(string id)
            : base(id)
        {
            int lastAt = id.LastIndexOf('@');
            if (lastAt >= 0)
            {
                _name = id.Substring(0, lastAt);
                _host = id.Substring(lastAt + 1);
            }
            else
            {
                _name = id;
                _host = "%";
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserName"/> class
        /// with the specified name and host address.
        /// </summary>
        /// <param name="id">The database name.</param>
        /// <param name="hostAddress">The host address which the user must connect from, or <see langword="null"/> to allow this user to connect from any host.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="id"/> is empty.</exception>
        public UserName(string id, IPAddress hostAddress)
            : base(string.Format("{0}@{1}", id, hostAddress != null ? hostAddress.ToString() : "%"))
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id cannot be empty");

            _name = id;
            _host = hostAddress != null ? hostAddress.ToString() : "%";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserName"/> class
        /// with the specified name and host.
        /// </summary>
        /// <param name="id">The database user name.</param>
        /// <param name="host">The name of the host from which this user can connect, or <see langword="null"/> to allow this user to connect from any host.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="id"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="host"/> is empty.</para>
        /// </exception>
        public UserName(string id, string host)
            : base(string.Format("{0}@{1}", id, host ?? "%"))
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("id cannot be empty");
            if (host == string.Empty)
                throw new ArgumentException("host cannot be empty", "host");

            _name = id;
            _host = host ?? "%";
        }

        /// <summary>
        /// Gets the username portion of this MySQL user name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the host portion of this MySQL user name.
        /// </summary>
        public string Host
        {
            get
            {
                return _host;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="UserName"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override UserName FromValue(string id)
            {
                return new UserName(id);
            }
        }
    }
}
