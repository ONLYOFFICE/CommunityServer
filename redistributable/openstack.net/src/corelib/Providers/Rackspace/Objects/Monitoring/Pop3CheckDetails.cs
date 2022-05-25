namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemotePop3Banner"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemotePop3Banner"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Pop3CheckDetails : SecureConnectionCheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Pop3CheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected Pop3CheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Pop3CheckDetails"/> class
        /// with the specified port and SSL configuration.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port (110) for the associated service should be used.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public Pop3CheckDetails(int? port = null, bool? enableSsl = null)
            : base(port, enableSsl)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemotePop3Banner"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemotePop3Banner;
        }
    }
}
