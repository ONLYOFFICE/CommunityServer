namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class extends the <see cref="ConnectionCheckDetails"/> class
    /// for check types which may use an SSL secure connection.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class SecureConnectionCheckDetails : ConnectionCheckDetails
    {
        /// <summary>
        /// This is the backing field for the <see cref="EnableSsl"/> property.
        /// </summary>
        [JsonProperty("ssl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private bool? _enableSsl;

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureConnectionCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SecureConnectionCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecureConnectionCheckDetails"/> class
        /// with the specified port and SSL configuration.
        /// </summary>
        /// <param name="port">The port to use for connecting to the remote service. If this value is <see langword="null"/>, the default port for the associated service should be used.</param>
        /// <param name="enableSsl"><see langword="true"/> to enable SSL for connecting to the service; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, a provider-specific default value is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        protected SecureConnectionCheckDetails(int? port, bool? enableSsl)
            : base(port)
        {
            _enableSsl = enableSsl;
        }

        /// <summary>
        /// Gets a value indicating whether an SSL connection should be used to connect to the service.
        /// </summary>
        public bool? EnableSsl
        {
            get
            {
                return _enableSsl;
            }
        }
    }
}
