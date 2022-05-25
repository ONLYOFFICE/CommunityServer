namespace net.openstack.Providers.Rackspace.Objects.LoadBalancers
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of an SSL termination configuration in the
    /// load balancer service.
    /// </summary>
    /// <seealso cref="ILoadBalancerService.GetSslConfigurationAsync"/>
    /// <seealso cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class LoadBalancerSslConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Enabled"/> property.
        /// </summary>
        [JsonProperty("enabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private bool? _enabled;

        /// <summary>
        /// This is the backing field for the <see cref="SecureTrafficOnly"/> property.
        /// </summary>
        [JsonProperty("secureTrafficOnly", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private bool? _secureTrafficOnly;

        /// <summary>
        /// This is the backing field for the <see cref="SecurePort"/> property.
        /// </summary>
        [JsonProperty("securePort", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _securePort;

        /// <summary>
        /// This is the backing field for the <see cref="PrivateKey"/> property.
        /// </summary>
        [JsonProperty("privatekey", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _privateKey;

        /// <summary>
        /// This is the backing field for the <see cref="Certificate"/> property.
        /// </summary>
        [JsonProperty("certificate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _certificate;

        /// <summary>
        /// This is the backing field for the <see cref="IntermediateCertificate"/> property.
        /// </summary>
        [JsonProperty("intermediateCertificate", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _intermediateCertificate;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerSslConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected LoadBalancerSslConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerSslConfiguration"/> class
        /// using the specified configuration.
        /// </summary>
        /// <remarks>
        /// Configurations created with this constructor will not overwrite the private key or
        /// certificates stored on the server during a call to
        /// <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.
        /// </remarks>
        /// <param name="enabled"><see langword="true"/> to enable SSL termination on the load balancer; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, the current configuration for the value is not changed by a call to <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.</param>
        /// <param name="secureTrafficOnly"><see langword="true"/> to require encryption for all traffic through the load balancer; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, the current configuration for the value is not changed by a call to <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.</param>
        /// <param name="securePort">The port on which the SSL termination load balancer will listen for secure traffic. If this value is <see langword="null"/>, the current configuration for the value is not changed by a call to <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="securePort"/> is less than 0 or greater than 65535.
        /// </exception>
        public LoadBalancerSslConfiguration(bool? enabled, bool? secureTrafficOnly, int? securePort)
        {
            if (securePort < 0 || securePort > 65535)
                throw new ArgumentOutOfRangeException("securePort");

            _enabled = enabled;
            _secureTrafficOnly = secureTrafficOnly;
            _securePort = securePort;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadBalancerSslConfiguration"/> class
        /// using the specified configuration and certificates.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> to enable SSL termination on the load balancer; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, the current configuration for the value is not changed by a call to <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.</param>
        /// <param name="secureTrafficOnly"><see langword="true"/> to require encryption for all traffic through the load balancer; otherwise, <see langword="false"/>. If this value is <see langword="null"/>, the current configuration for the value is not changed by a call to <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.</param>
        /// <param name="securePort">The port on which the SSL termination load balancer will listen for secure traffic. If this value is <see langword="null"/>, the current configuration for the value is not changed by a call to <see cref="ILoadBalancerService.UpdateSslConfigurationAsync"/>.</param>
        /// <param name="privateKey">The private key for the SSL certificate.</param>
        /// <param name="certificate">The certificate used for SSL termination.</param>
        /// <param name="intermediateCertificate">The user's intermediate certificate used for SSL termination.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="privateKey"/> is <see langword="null"/> and either <paramref name="certificate"/> or <paramref name="intermediateCertificate"/> is not <see langword="null"/>.
        /// <para>-or-</para>
        /// <para>If <paramref name="certificate"/> is <see langword="null"/> and <paramref name="privateKey"/> is not <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="privateKey"/> is empty.
        /// <para>-or-</para>
        /// <para>If <paramref name="certificate"/> is empty.</para>
        /// <para>-or-</para>
        /// <para>If <paramref name="intermediateCertificate"/> is empty.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <paramref name="securePort"/> is less than 0 or greater than 65535.
        /// </exception>
        public LoadBalancerSslConfiguration(bool? enabled, bool? secureTrafficOnly, int securePort, string privateKey, string certificate, string intermediateCertificate)
            : this(enabled, secureTrafficOnly, securePort)
        {
            if (privateKey == null && (certificate != null || intermediateCertificate != null))
                throw new ArgumentNullException("privateKey");
            if (certificate == null && privateKey != null)
                throw new ArgumentNullException("certificate");
            if (privateKey == string.Empty)
                throw new ArgumentException("privateKey cannot be empty");
            if (certificate == string.Empty)
                throw new ArgumentException("certificate cannot be empty");
            if (intermediateCertificate == string.Empty)
                throw new ArgumentException("intermediateCertificate cannot be empty");

            _privateKey = privateKey;
            _certificate = certificate;
            _intermediateCertificate = intermediateCertificate;
        }

        /// <summary>
        /// Gets a value indicating whether or not SSL termination is enabled on the load balancer.
        /// </summary>
        public bool? Enabled
        {
            get
            {
                return _enabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether or not the load balancer should <em>only</em> accept secure traffic.
        /// </summary>
        public bool? SecureTrafficOnly
        {
            get
            {
                return _secureTrafficOnly;
            }
        }

        /// <summary>
        /// Gets a value indicating which port the load balancer should listen for secure traffic on.
        /// </summary>
        public int? SecurePort
        {
            get
            {
                return _securePort;
            }
        }

        /// <summary>
        /// Gets the private key for the certificate used for SSL termination on the load balancer.
        /// </summary>
        public string PrivateKey
        {
            get
            {
                return _privateKey;
            }
        }

        /// <summary>
        /// Gets the certificate used for SSL termination on the load balancer.
        /// </summary>
        public string Certificate
        {
            get
            {
                return _certificate;
            }
        }

        /// <summary>
        /// Gets the intermediate certificate used for SSL termination on the load balancer.
        /// </summary>
        public string IntermediateCertificate
        {
            get
            {
                return _intermediateCertificate;
            }
        }
    }
}
