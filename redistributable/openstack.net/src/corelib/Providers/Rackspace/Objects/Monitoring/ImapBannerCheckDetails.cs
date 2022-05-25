namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteImapBanner"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteImapBanner"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ImapBannerCheckDetails : SecureConnectionCheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImapBannerCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected ImapBannerCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImapBannerCheckDetails"/> class
        /// with the specified port and SSL configuration.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port (143) for the associated service should be used.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public ImapBannerCheckDetails(int? port = null, bool? enableSsl = null)
            : base(port, enableSsl)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteImapBanner"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteImapBanner;
        }
    }
}
