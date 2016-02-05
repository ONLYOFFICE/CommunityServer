namespace RedisSessionProvider
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    using RedisSessionProvider.Config;
    using RedisSessionProvider.Redis;
    using RedisSessionProvider.Serialization;

    using SysTimer = System.Timers.Timer;

    /// <summary>
    /// Class that keeps a count of the number of currently executing web requests that need 
    ///     a particular Session. When the count reaches 0, clears it from the shared in-memory
    ///     storage so that the next request goes back to Redis.
    /// </summary>
    public class LocalSharedSessionDictionary
    {
        private static ConcurrentDictionary<string, SessionAndRefCount> localCache =
            new ConcurrentDictionary<string, SessionAndRefCount>();

        private static SysTimer cacheFreshnessTimer;

        private static int cacheItemExpirationAgeMillis = 120000;
        private static int checkCacheFreshnessInterval = 5000;

        static LocalSharedSessionDictionary()
        {
            cacheFreshnessTimer = new SysTimer(checkCacheFreshnessInterval);
            cacheFreshnessTimer.Elapsed += EnsureLocalCacheFreshness;
            cacheFreshnessTimer.Start();
        }

        static void EnsureLocalCacheFreshness(object sender, ElapsedEventArgs e)
        {
            try
            {
                List<string> expiredKeys = new List<string>();

                DateTime now = DateTime.Now;

                foreach (KeyValuePair<string, SessionAndRefCount> val in LocalSharedSessionDictionary.localCache)
                {
                    if (val.Value.LastAccess.AddMilliseconds(cacheItemExpirationAgeMillis) < now ||
                        val.Value.RequestReferences <= 0)
                    {
                        expiredKeys.Add(val.Key);
                    }
                }

                foreach (string expKey in expiredKeys)
                {
                    SessionAndRefCount removed;
                    LocalSharedSessionDictionary.localCache.TryRemove(expKey, out removed);
                }
            }
            catch (Exception sharedDictExc)
            {
                if (RedisSessionConfig.SessionExceptionLoggingDel != null)
                {
                    RedisSessionConfig.SessionExceptionLoggingDel(sharedDictExc);
                }
            }
        }
        
        /// <summary>
        /// Gets a session for a given redis ID, and increments the count of the number of requests
        ///     that have accessed this redis ID
        /// </summary>
        /// <param name="redisHashId">The id of the session in Redis</param>
        /// <param name="getDel">The delegate to run to fetch the session from Redis</param>
        /// <returns>A RedisSessionStateItemCollection for the session</returns>
        public RedisSessionStateItemCollection GetSessionForBeginRequest(
            string redisHashId, 
            Func<string, RedisSessionStateItemCollection> getDel)
        {
            SessionAndRefCount sessAndCount = LocalSharedSessionDictionary.localCache.AddOrUpdate(
                redisHashId,
                (redisKey) =>
                {
                    RedisSessionStateItemCollection itms = getDel(redisHashId);

                    return new SessionAndRefCount(itms);
                },
                (redisKey, existingItem) => {
                    Interlocked.Increment(ref existingItem.RequestReferences);
                    existingItem.LastAccess = DateTime.Now;

                    return existingItem;
                });

            return sessAndCount.Sess;
        }

        /// <summary>
        /// Gets a Session collection, but decrements the number of requests that need it. When
        ///     the count gets to 0, the object is cleared from the local in-memory cache of
        ///     all Sessions so that the next request that needs a session will go to Redis for
        ///     the data
        /// </summary>
        /// <param name="redisHashId">The Id of the session in Redis</param>
        /// <returns>A RedisSessionStateItemCollection for the session</returns>
        public RedisSessionStateItemCollection GetSessionForEndRequest(string redisHashId)
        {
            SessionAndRefCount sessAndCount;
            if (LocalSharedSessionDictionary.localCache.TryGetValue(redisHashId, out sessAndCount))
            {
                // atomically decrease ref count, and check to see if any requests outstanding
                Interlocked.Decrement(ref sessAndCount.RequestReferences);
                // the timer will clear it out within the next 5 seconds if the count goes to 0

                return sessAndCount.Sess;
            }

            return null;
        }

        /// <summary>
        /// Internal class for holding a session item collection and the count of requests referecing it
        /// </summary>
        class SessionAndRefCount
        {
            /// <summary>
            /// Initializes a new instance of the SessionAndRefCount class with a given item collection
            /// </summary>
            /// <param name="itms">The items in a session</param>
            public SessionAndRefCount(RedisSessionStateItemCollection itms)
                : this(itms, 1)
            {
            }

            /// <summary>
            /// Initializes a new instance of the SessionAndRefCount class with a given item collection
            ///     and count of requests using it
            /// </summary>
            /// <param name="itms">The items in a session</param>
            /// <param name="count">The number of requests accessing this session</param>
            public SessionAndRefCount(RedisSessionStateItemCollection itms, int count)
            {
                this.Sess = itms;
                this.RequestReferences = count;
                this.LastAccess = DateTime.Now;
            }

            /// <summary>
            /// Gets or sets the item collection
            /// </summary>
            public RedisSessionStateItemCollection Sess { get; set; }

            /// <summary>
            /// The number of requests that have a reference to this session
            /// </summary>
            public int RequestReferences;

            /// <summary>
            /// The last time this session was accessed.
            /// </summary>
            public DateTime LastAccess;
        }
    }
}
