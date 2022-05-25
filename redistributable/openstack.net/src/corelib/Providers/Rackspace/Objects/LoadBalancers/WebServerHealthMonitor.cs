namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class models the JSON object used to represent a <see cref="HealthMonitor"/> for
    /// HTTP or HTTPS connections.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class WebServerHealthMonitor : HealthMonitor
    {
        /// <summary>
        /// This is the backing field for the <see cref="BodyRegex"/> property.
        /// </summary>
        [JsonProperty("bodyRegex")]
        private string _bodyRegex;

        /// <summary>
        /// This is the backing field for the <see cref="Path"/> property.
        /// </summary>
        [JsonProperty("path")]
        private string _path;

        /// <summary>
        /// This is the backing field for the <see cref="StatusRegex"/> property.
        /// </summary>
        [JsonProperty("statusRegex")]
        private string _statusRegex;

        /// <summary>
        /// This is the backing field for the <see cref="HostHeader"/> property.
        /// </summary>
        [JsonProperty("hostHeader", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _hostHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerHealthMonitor"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected WebServerHealthMonitor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerHealthMonitor"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="https"><see langword="true"/> to monitor HTTPS connections; otherwise, <see langword="false"/> to monitor HTTP connections.</param>
        /// <param name="attemptsBeforeDeactivation">The number of permissible monitor failures before removing a node from rotation.</param>
        /// <param name="timeout">The maximum number of seconds to wait for a connection to be established before timing out.</param>
        /// <param name="delay">The minimum time to wait before executing the health monitor.</param>
        /// <param name="bodyRegex">A regular expression that will be used to evaluate the contents of the body of the response.</param>
        /// <param name="path">The HTTP path that will be used in the sample request.</param>
        /// <param name="statusRegex">A regular expression that will be used to evaluate the HTTP status code returned in the response.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="bodyRegex"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="path"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="statusRegex"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="bodyRegex"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="path"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="statusRegex"/> is empty.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="attemptsBeforeDeactivation"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="timeout"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="delay"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        public WebServerHealthMonitor(bool https, int attemptsBeforeDeactivation, TimeSpan timeout, TimeSpan delay, string bodyRegex, string path, string statusRegex)
            : this(https, attemptsBeforeDeactivation, timeout, delay, bodyRegex, path, statusRegex, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerHealthMonitor"/> class
        /// with the specified values.
        /// </summary>
        /// <param name="https"><see langword="true"/> to monitor HTTPS connections; otherwise, <see langword="false"/> to monitor HTTP connections.</param>
        /// <param name="attemptsBeforeDeactivation">The number of permissible monitor failures before removing a node from rotation.</param>
        /// <param name="timeout">The maximum number of seconds to wait for a connection to be established before timing out.</param>
        /// <param name="delay">The minimum time to wait before executing the health monitor.</param>
        /// <param name="bodyRegex">A regular expression that will be used to evaluate the contents of the body of the response.</param>
        /// <param name="path">The HTTP path that will be used in the sample request.</param>
        /// <param name="statusRegex">A regular expression that will be used to evaluate the HTTP status code returned in the response.</param>
        /// <param name="hostHeader">The name of a host for which the health monitors will check, or <see langword="null"/> <placeholder>when?</placeholder>.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="bodyRegex"/> is <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="path"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="statusRegex"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="bodyRegex"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="path"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="statusRegex"/> is empty.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="attemptsBeforeDeactivation"/> is less than or equal to 0.
        /// <para>-or-</para>
        /// <para>If <paramref name="timeout"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="delay"/> is negative or <see cref="TimeSpan.Zero"/>.</para>
        /// </exception>
        public WebServerHealthMonitor(bool https, int attemptsBeforeDeactivation, TimeSpan timeout, TimeSpan delay, string bodyRegex, string path, string statusRegex, string hostHeader)
            : base(https ? HealthMonitorType.Https : HealthMonitorType.Http, attemptsBeforeDeactivation, timeout, delay)
        {
            if (bodyRegex == null)
                throw new ArgumentNullException("bodyRegex");
            if (path == null)
                throw new ArgumentNullException("path");
            if (statusRegex == null)
                throw new ArgumentNullException("statusRegex");
            if (string.IsNullOrEmpty(bodyRegex))
                throw new ArgumentException("bodyRegex cannot be empty");
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("path cannot be empty");
            if (string.IsNullOrEmpty(statusRegex))
                throw new ArgumentException("statusRegex cannot be empty");

            _bodyRegex = bodyRegex;
            _path = path;
            _statusRegex = statusRegex;
            _hostHeader = hostHeader;
        }

        /// <summary>
        /// Gets a regular expression that will be used to evaluate the contents of the body of the response.
        /// </summary>
        public string BodyRegex
        {
            get
            {
                return _bodyRegex;
            }
        }

        /// <summary>
        /// Gets the HTTP path that will be used in the sample request.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        /// <summary>
        /// Gets a regular expression that will be used to evaluate the HTTP status code returned in the response.
        /// </summary>
        public string StatusRegex
        {
            get
            {
                return _statusRegex;
            }
        }

        /// <summary>
        /// Gets the optional name of a host for which the health monitors will check.
        /// </summary>
        public string HostHeader
        {
            get
            {
                return _hostHeader;
            }
        }
    }
}
