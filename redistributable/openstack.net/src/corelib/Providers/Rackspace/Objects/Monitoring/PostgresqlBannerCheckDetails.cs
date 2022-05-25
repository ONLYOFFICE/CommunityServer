namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemotePostgresqlBanner"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemotePostgresqlBanner"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class PostgresqlBannerCheckDetails : SecureConnectionCheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresqlBannerCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected PostgresqlBannerCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresqlBannerCheckDetails"/> class
        /// with the specified port and SSL configuration.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port (5432) for the associated service should be used.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public PostgresqlBannerCheckDetails(int? port = null, bool? enableSsl = null)
            : base(port, enableSsl)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemotePostgresqlBanner"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemotePostgresqlBanner;
        }
    }
}
