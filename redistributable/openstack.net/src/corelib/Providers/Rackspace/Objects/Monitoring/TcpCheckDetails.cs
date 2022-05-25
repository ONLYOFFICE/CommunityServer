namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteTcp"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteTcp"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class TcpCheckDetails : SecureConnectionCheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="BannerMatch"/> property.
        /// </summary>
        [JsonProperty("banner_match", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _bannerMatch;

        /// <summary>
        /// This is the backing field for the <see cref="BodyMatch"/> property.
        /// </summary>
        [JsonProperty("body_match", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _bodyMatch;

        /// <summary>
        /// This is the backing field for the <see cref="SendBody"/> property.
        /// </summary>
        [JsonProperty("send_body", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _sendBody;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected TcpCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpCheckDetails"/> class
        /// with the values.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="bannerMatch">A regular expression to match against the TCP banner returned by the service.</param>
        /// <param name="bodyMatch">A regular expression to match against the TCP body returned by the service.</param>
        /// <param name="sendBody">The body to send to the service after the banner is verified.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public TcpCheckDetails(int port, bool? enableSsl = null, string bannerMatch = null, string bodyMatch = null, string sendBody = null)
            : base(port, enableSsl)
        {
            _bannerMatch = bannerMatch;
            _bodyMatch = bodyMatch;
            _sendBody = sendBody;
        }

        /// <summary>
        /// Gets the regular expression to match against the TCP banner returned by the service.
        /// </summary>
        public string BannerMatch
        {
            get
            {
                return _bannerMatch;
            }
        }

        /// <summary>
        /// Gets the regular expression to match against the TCP body returned by the service.
        /// </summary>
        public string BodyMatch
        {
            get
            {
                return _bodyMatch;
            }
        }

        /// <summary>
        /// Gets the body to send to the service after the banner is verified.
        /// </summary>
        public string SendBody
        {
            get
            {
                return _sendBody;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteTcp"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteTcp;
        }
    }
}
