namespace RedisSessionProvider.Config
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using StackExchange.Redis;

    /// <summary>
    /// Class that handles options for connecting to a Redis instance for RedisSessionProvider
    /// </summary>
    public class RedisConnectionParameters
    {
        /// <summary>
        /// Initializes a new instance of the RedisConnectionParameters class with a null
        ///     host address, default port 6379, null password, server version 2.6.14 and
        ///     no proxy behavior.
        /// </summary>
        public RedisConnectionParameters()
        {
            this.ServerAddress = null;
            this.ServerPort = 6379;
            this.Password = null;
            this.ServerVersion = "2.6.14";
            this.UseProxy = Proxy.None;
            this.KeepAlive = 5;
            this.DatabaseIndex = 0;
        }

        /// <summary>
        /// Gets or sets the ip address or hostname of the redis server
        /// </summary>
        public string ServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the port the redis server is listening on, defaults to 6379
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Gets or sets the password to use when connecting, default is null which indicates no password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the redis server version, defaults to 2.6.14
        /// </summary>
        public string ServerVersion { get; set; }

        /// <summary>
        /// Gets or sets the proxy behavior to use, currently, StackExchange.Redis has an option for
        ///     TwemProxy-compatible behavior
        /// </summary>
        public Proxy UseProxy { get; set; }

        /// <summary>
        /// Gets or sets the interval in seconds to make sure the connection is still going
        /// </summary>
        public int KeepAlive { get; set; }

        /// <summary>
        /// Gets or sets the index of database
        /// </summary>
        public int DatabaseIndex { get; set; }

        /// <summary>
        /// Makes a StackExchange.Redis.ConfigurationOptions class from the properties of the
        ///     RedisConnectionParameters.
        /// </summary>
        /// <returns>
        /// A ConfigurationOptions instance that can be used by StackExchange.Redis
        ///     ConnectionMultiplexer to connect to a Redis instance
        /// </returns>
        public ConfigurationOptions TranslateToConfigOpts()
        {
            ConfigurationOptions connectOpts = ConfigurationOptions.Parse(
                this.ServerAddress + ":" + this.ServerPort);

            connectOpts.KeepAlive = this.KeepAlive;

            if (!string.IsNullOrEmpty(this.Password))
            {
                connectOpts.Password = this.Password;
            }
            if (!string.IsNullOrEmpty(this.ServerVersion))
            {
                connectOpts.DefaultVersion = new Version(this.ServerVersion);
            }
            if (this.UseProxy != Proxy.None)
            {
                // thanks marc gravell
                connectOpts.Proxy = this.UseProxy;
            }

            return connectOpts;
        }
    }
}
