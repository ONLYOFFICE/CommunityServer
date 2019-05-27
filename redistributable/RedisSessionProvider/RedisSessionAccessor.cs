namespace RedisSessionProvider
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.SessionState;

    using Config;

    public class RedisSessionAccessor : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the RedisSessionAccessor class, which provides access to a
        ///     local Redis items collection outside of the standard ASP.NET pipeline Session hooks
        /// </summary>
        /// <param name="context">The context of the current request</param>
        public RedisSessionAccessor(HttpContextBase context)
        {
            try
            {
                this.RequestContext = context;
                this.SharedSessions = new LocalSharedSessionDictionary();

                // if we have the session ID
                if (this.RequestContext.Request.Cookies[RedisSessionConfig.SessionHttpCookieName] != null)
                {
                    this.SessionRedisHashKey = RedisSessionStateStoreProvider.RedisHashIdFromSessionId(
                        this.RequestContext,
                        this.RequestContext.Request.Cookies[RedisSessionConfig.SessionHttpCookieName].Value);
                }

                if (!string.IsNullOrEmpty(this.SessionRedisHashKey))
                {
                    RedisSessionStateItemCollection items =
                        this.SharedSessions.GetSessionForBeginRequest(
                            this.SessionRedisHashKey,
                            (string redisKey) =>
                            {
                                return RedisSessionStateStoreProvider.GetItemFromRedis(
                                    redisKey,
                                    this.RequestContext,
                                    RedisSessionConfig.SessionTimeout);
                            });

                    this.Session = new FakeHttpSessionState(
                        items, 
                        this.RequestContext.Request.Cookies[RedisSessionConfig.SessionHttpCookieName].Value);
                }
            }
            catch(Exception exc)
            {
                if(RedisSessionConfig.RedisSessionAccessorExceptionLoggingDel != null)
                {
                    string errMsg = string.Format(
                        "RedisSessionAccessor unable to get Redis session for id: {0}", 
                        this.SessionRedisHashKey);

                    RedisSessionConfig.RedisSessionAccessorExceptionLoggingDel(
                        context,
                        errMsg,
                        exc);
                }
            }
        }

        /// <summary>
        /// Gets a Session item collection outside of the normal ASP.NET pipeline, but will serialize back to
        ///     Redis on Dispose of RedisSessionAccessor object
        /// </summary>
        public FakeHttpSessionState Session { get; protected set; }

        /// <summary>
        /// Gets or sets the context of the current web request
        /// </summary>
        protected HttpContextBase RequestContext { get; set; }

        /// <summary>
        /// Gets or sets a collection that handles all RedisSessionStateItemCollection objecst used by
        ///     RedisSessionProvider
        /// </summary>
        protected LocalSharedSessionDictionary SharedSessions { get; set; }

        /// <summary>
        /// Gets or sets the string key in Redis holding the Session data
        /// </summary>
        protected string SessionRedisHashKey { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            // record with local shared session storage that we are done with the session so it gets
            //      cleared out sooner
            RedisSessionStateItemCollection items =
                this.SharedSessions.GetSessionForEndRequest(this.SessionRedisHashKey);

            if (items != null)
            {
                RedisSessionStateStoreProvider.SerializeToRedis(
                    this.RequestContext,
                    items,
                    this.SessionRedisHashKey,
                    RedisSessionConfig.SessionTimeout);
            }
        }

        #endregion

        public class FakeHttpSessionState : HttpSessionStateBase
        {
            private string sessionID;

            public FakeHttpSessionState(ISessionStateItemCollection items, string sessID)
            {
                this.Items = items;
                this.sessionID = sessID;
            }

            protected ISessionStateItemCollection Items { get; set; }

            public override void Add(string name, object value)
            {
                this.Items[name] = value;
            }

            public override void Remove(string name)
            {
                this.Items.Remove(name);
            }

            public override object this[string name]
            {
                get
                {
                    return this.Items[name];
                }
                set
                {
                    this.Items[name] = value;
                }
            }

            public override System.Collections.Specialized.NameObjectCollectionBase.KeysCollection Keys
            {
                get
                {
                    return this.Items.Keys;
                }
            }

            public override int Count
            {
                get
                {
                    return this.Items.Count;
                }
            }

            /// <summary>
            /// Gets the SessionID associated with the current Session object. Note that this is
            ///     the raw Session ID value, not the Redis Key. In order to get that, run the
            ///     result through RedisSessionConfig.RedisKeyFromSessionIdDel
            /// </summary>
            public override string SessionID
            {
                get
                {
                    return this.sessionID;   
                }
            }
        }
    }
}
