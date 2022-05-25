namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteFtpBanner"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteFtpBanner"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class FtpBannerCheckDetails : ConnectionCheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpBannerCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected FtpBannerCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpBannerCheckDetails"/> class
        /// with the specified port.
        /// </summary>
        /// <param name="port">The port number of the FTP service. If this value is <see langword="null"/>, the default value (21) for the service is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public FtpBannerCheckDetails(int? port)
            : base(port)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteFtpBanner"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteFtpBanner;
        }
    }
}
