namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// This class represents the detailed configuration parameters for a
    /// <see cref="CheckTypeId.RemoteSsh"/> check.
    /// </summary>
    /// <seealso cref="CheckTypeId.RemoteSsh"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class SshCheckDetails : ConnectionCheckDetails
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SshCheckDetails"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected SshCheckDetails()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SshCheckDetails"/> class
        /// with the specified port.
        /// </summary>
        /// <param name="port">The port number of the SSH service. If this value is <see langword="null"/>, the default value (22) for the service is used.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="port"/> is less than or equal to 0, or if <paramref name="port"/> is greater than 65535.</exception>
        public SshCheckDetails(int? port = null)
            : base(port)
        {
        }

        /// <inheritdoc/>
        /// <remarks>
        /// This class only supports <see cref="CheckTypeId.RemoteSsh"/> checks.
        /// </remarks>
        protected internal override bool SupportsCheckType(CheckTypeId checkTypeId)
        {
            return checkTypeId == CheckTypeId.RemoteSsh;
        }
    }
}
