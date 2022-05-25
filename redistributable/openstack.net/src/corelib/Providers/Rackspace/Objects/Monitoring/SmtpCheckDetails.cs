namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteSmtp"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteSmtp"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class SmtpCheckDetails : ConnectionCheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="Ehlo"/> property.
        /// </summary>
        [JsonProperty("ehlo", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _ehlo;

        /// <summary>
        /// This is the backing field for the <see cref="From"/> property.
        /// </summary>
        [JsonProperty("from", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _from;

        /// <summary>
        /// This is the backing field for the <see cref="To"/> property.
        /// </summary>
        [JsonProperty("to", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _to;

        /// <summary>
        /// This is the backing field for the <see cref="Payload"/> property.
        /// </summary>
        [JsonProperty("payload", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _payload;

        /// <summary>
        /// This is the backing field for the <see cref="StartTls"/> property.
        /// </summary>
        [JsonProperty("starttls", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private bool? _startTls;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SmtpCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpCheckDetails"/> class
        /// with the specified parameters.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port (25) for the associated service should be used.</param>
        /// <param name="ehlo">The <strong>EHLO</strong> parameter of the SMTP message to send.</param>
        /// <param name="from">The <strong>From</strong> parameter of the SMTP message to send.</param>
        /// <param name="to">The <strong>To</strong> parameter of the SMTP message to send. If the value is <see langword="null"/>, a <strong>quit</strong> is issued before sending a <strong>To</strong> line, and the connection is terminated.</param>
        /// <param name="payload">The body of the message to send.</param>
        /// <param name="startTls"><see langword="true"/> to use a TLS/SSL connection when connecting to the service; otherwise, <see langword="false"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public SmtpCheckDetails(int? port, string ehlo, string from, string to, string payload, bool? startTls)
            : base(port)
        {
            _ehlo = ehlo;
            _from = from;
            _to = to;
            _payload = payload;
            _startTls = startTls;
        }

        /// <summary>
        /// Gets the <strong>EHLO</strong> parameter of the SMTP message to send.
        /// </summary>
        public string Ehlo
        {
            get
            {
                return _ehlo;
            }
        }

        /// <summary>
        /// Gets the <strong>From</strong> parameter of the SMTP message to send.
        /// </summary>
        public string From
        {
            get
            {
                return _from;
            }
        }

        /// <summary>
        /// Gets the <strong>To</strong> parameter of the SMTP message to send.
        /// </summary>
        public string To
        {
            get
            {
                return _to;
            }
        }

        /// <summary>
        /// Gets the body of the message to send.
        /// </summary>
        public string Payload
        {
            get
            {
                return _payload;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a TLS/SSL connection should be used to connect to the service.
        /// </summary>
        public bool? StartTls
        {
            get
            {
                return _startTls;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteSmtp"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteSmtp;
        }
    }
}
