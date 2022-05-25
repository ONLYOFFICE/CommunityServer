namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteMysqlBanner"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteMysqlBanner"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MysqlBannerCheckDetails : SecureConnectionCheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MysqlBannerCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MysqlBannerCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MysqlBannerCheckDetails"/> class
        /// with the specified port and SSL configuration.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port (3306) for the associated service should be used.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public MysqlBannerCheckDetails(int? port = null, bool? enableSsl = null)
            : base(port, enableSsl)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteMysqlBanner"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteMysqlBanner;
        }
    }
}
