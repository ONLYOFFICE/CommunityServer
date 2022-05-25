namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteTelnetBanner"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteTelnetBanner"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class TelnetBannerCheckDetails : SecureConnectionCheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="BannerMatch"/> property.
        /// </summary>
        [JsonProperty("banner_match", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _bannerMatch;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetBannerCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected TelnetBannerCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TelnetBannerCheckDetails"/> class
        /// with the values.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <param name="bannerMatch">A regular expression to match against the telnet banner returned by the service. If this value is <see langword="null"/>, the behavior of the service is unspecified.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public TelnetBannerCheckDetails(int port, bool? enableSsl = null, string bannerMatch = null)
            : base(port, enableSsl)
        {
            _bannerMatch = bannerMatch;
        }

        /// <summary>
        /// Gets the regular expression to match against the telnet banner returned by the remote service.
        /// </summary>
        public string BannerMatch
        {
            get
            {
                return _bannerMatch;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteTelnetBanner"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteTelnetBanner;
        }
    }
}
