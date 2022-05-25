using System;
using System.Net;
using net.openstack.Core.Exceptions;
using net.openstack.Core.Validators;

namespace net.openstack.Providers.Rackspace.Validators
{
    /// <summary>
    /// Provides an implementation of <see cref="INetworksValidator"/> for
    /// operation with Rackspace's Cloud Networks product.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    public class CloudNetworksValidator : INetworksValidator
    {
        /// <summary>
        /// A default instance of <see cref="CloudNetworksValidator"/>.
        /// </summary>
        private static readonly CloudNetworksValidator _default = new CloudNetworksValidator();

        /// <summary>
        /// Gets a default instance of <see cref="CloudNetworksValidator"/>.
        /// </summary>
        public static CloudNetworksValidator Default
        {
            get
            {
                return _default;
            }
        }

        /// <inheritdoc/>
        public void ValidateCidr(string cidr)
        {
            if (cidr == null)
                throw new ArgumentNullException("cidr");
            if (string.IsNullOrEmpty(cidr))
                throw new CidrFormatException("cidr cannot be empty");

            if (!cidr.Contains("/"))
                throw new CidrFormatException(string.Format("ERROR: CIDR {0} is missing /", cidr));

            var parts = cidr.Split('/');

            if (parts.Length != 2)
                throw new CidrFormatException(string.Format("ERROR: CIDR {0} must have exactly one / character", cidr));

            var ipAddress = parts[0];
            var cidr_range = parts[1];

            if (!IsIpAddress(ipAddress))
                throw new CidrFormatException(string.Format("ERROR: IP address segment ({0}) of CIDR is not a valid IP address", ipAddress));

            int cidrInt;
            if (!int.TryParse(cidr_range, out cidrInt))
                throw new CidrFormatException(string.Format("ERROR: CIDR range segment {0} must be an integer", cidr_range));

            if (cidrInt < 1 || cidrInt > 32)
                throw new CidrFormatException(string.Format("ERROR: CIDR range segment {0} must be between 1 and 32", cidr_range));
        }

        /// <summary>
        /// Returns true if the string matches the ip address pattern (xxx.xxx.xxx.xxx) 
        /// and all octets are numeric and less than 256
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool IsIpAddress(string address)
        {
            IPAddress ipAddress;
            return IPAddress.TryParse(address, out ipAddress);
        }

    }
}
