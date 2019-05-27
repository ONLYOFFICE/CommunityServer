namespace RedisSessionProvider.Redis
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using System.Web;

    using StackExchange.Redis;
    using RedisSessionProvider.Config;

    public sealed class RedisConnectionWrapper
    {
        private static Dictionary<string, ConnectionMultiplexer> RedisConnections =
            new Dictionary<string, ConnectionMultiplexer>();
        private static Dictionary<string, long> RedisStats =
            new Dictionary<string, long>();
        
        private static System.Timers.Timer connMessagesSentTimer;
        
        private static object RedisCreateLock = new object();

        static RedisConnectionWrapper()
        {
            connMessagesSentTimer = new System.Timers.Timer(30000);
            connMessagesSentTimer.Elapsed += RedisConnectionWrapper.GetConnectionsMessagesSent;
            connMessagesSentTimer.Start();
        }

        /// <summary>
        /// Gets or sets the parameters to use when connecting to a redis server
        /// </summary>
        private ConfigurationOptions connData;

        /// <summary>
        /// A string identifier for the connection, which will be used as the connection's key in the
        ///     this.RedisConnections dictionary.
        /// </summary>
        public string ConnectionID { get; set; }

        /// <summary>
        /// The index of Database to store session.
        /// </summary>
        public int DatabaseIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the RedisConnectionWrapper class, which contains methods for accessing
        ///     a static concurrentdictionary of already created and open RedisConnection instances
        /// </summary>
        /// <param name="serverAddress">The ip address of the redis instance</param>
        /// <param name="serverPort">The port number of the redis instance</param>
        public RedisConnectionWrapper(string srvAddr, int srvPort)
        {
            this.connData = ConfigurationOptions.Parse(srvAddr + ":" + srvPort);

            this.ConnectionID = string.Format(
                    "{0}_%_{1}",
                    srvAddr,
                    srvPort);
        }
        
        /// <summary>
        /// Initializes a new instance of the RedisConnectionWrapper class, which contains methods for accessing
        ///     a static concurrentdictionary of already created and open redisconnection instances
        /// </summary>
        /// <param name="redisParams">A configuration class containing the redis server hostname and port number</param>
        public RedisConnectionWrapper(RedisConnectionParameters redisParams)
        {
            if (redisParams == null)
            {
                throw new ConfigurationErrorsException(
                    "RedisConnectionWrapper cannot be initialized with null RedisConnectionParameters property");
            }

            this.connData = redisParams.TranslateToConfigOpts();

            this.ConnectionID = string.Format(
                    "{0}_%_{1}",
                    redisParams.ServerAddress,
                    redisParams.ServerPort);
            this.DatabaseIndex = redisParams.DatabaseIndex;
        }

        /// <summary>
        /// Initializes a new instance of the RedisConnectionWrapper class, which contains methods for accessing
        ///     a static concurrentdictionary of already created and open redisconnection instances
        /// </summary>
        /// <param name="connIdentifier">Because it is possible to have connections to multiple redis instances, we store
        /// a dictionary of them to reuse. This parameter is used as the key to that dictionary.</param>
        /// <param name="connOpts">A StackExchange.Redis configuration class containing the redis connection info</param>
        public RedisConnectionWrapper(string connIdentifier, ConfigurationOptions connOpts)
            : this(connIdentifier, 0, connOpts)
        {
        }

        /// <summary>
        /// Initializes a new instance of the RedisConnectionWrapper class, which contains methods for accessing
        ///     a static concurrentdictionary of already created and open redisconnection instances
        /// </summary>
        /// <param name="connIdentifier">Because it is possible to have connections to multiple redis instances, we store
        /// a dictionary of them to reuse. This parameter is used as the key to that dictionary.</param>
        /// <param name="dbIndex">The index of the redis database with session information</param>
        /// <param name="connOpts">A StackExchange.Redis configuration class containing the redis connection info</param>
        public RedisConnectionWrapper(string connIdentifier, int dbIndex, ConfigurationOptions connOpts)
        {
            if (connOpts == null)
            {
                throw new ConfigurationErrorsException(
                    "RedisConnectionWrapper cannot be initialized with null ConfigurationOptions property");
            }

            this.connData = connOpts;
            this.DatabaseIndex = dbIndex;
            this.ConnectionID = connIdentifier;
        }

        /// <summary>
        /// Method that returns a StackExchange.Redis.IDatabase object with ip and port number matching
        ///     what was passed into the constructor for this instance of RedisConnectionWrapper
        /// </summary>
        /// <returns>An open and callable RedisConnection object, shared with other threads in this
        /// application domain that also called for a connection to the specified ip and port</returns>
        public IDatabase GetConnection()
        {
            if (!RedisConnectionWrapper.RedisConnections.ContainsKey(this.ConnectionID))
            {
                lock(RedisConnectionWrapper.RedisCreateLock)
                {
                    if (!RedisConnectionWrapper.RedisConnections.ContainsKey(this.ConnectionID))
                    {
                        RedisConnectionWrapper.RedisConnections.Add(
                            this.ConnectionID,
                            ConnectionMultiplexer.Connect(
                                this.connData));
                    }
                }
            }

            return RedisConnectionWrapper.RedisConnections[this.ConnectionID].GetDatabase(
                this.DatabaseIndex);
        }
        
        /// <summary>
        /// Gets the number of redis commands sent and received, and sets the count to 0 so the next time
        ///     we will not see double counts
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void GetConnectionsMessagesSent(object sender, ElapsedEventArgs e)
        {
            bool logCount = RedisConnectionConfig.LogConnectionActionsCountDel != null;

            if (logCount)
            {
                foreach (string connName in RedisConnectionWrapper.RedisConnections.Keys.ToList())
                {
                    try
                    {
                        ConnectionMultiplexer conn;
                        if (RedisConnectionWrapper.RedisConnections.TryGetValue(connName, out conn))
                        {
                            long priorPeriodCount = 0;
                            if (RedisConnectionWrapper.RedisStats.ContainsKey(connName))
                            {
                                priorPeriodCount = RedisConnectionWrapper.RedisStats[connName];
                            }

                            ServerCounters counts = conn.GetCounters();
                            long curCount = counts.Interactive.OperationCount;

                            // log the sent commands
                            RedisConnectionConfig.LogConnectionActionsCountDel(
                                connName, 
                                curCount - priorPeriodCount);

                            RedisConnectionWrapper.RedisStats[connName] = curCount;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }
}