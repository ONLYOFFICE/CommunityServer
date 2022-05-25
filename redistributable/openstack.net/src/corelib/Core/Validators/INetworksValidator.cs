namespace net.openstack.Core.Validators
{
    using System;
    using net.openstack.Core.Exceptions;
    using net.openstack.Core.Providers;

    /// <summary>
    /// Represents an object that validates arguments for an implementation of <see cref="INetworksProvider"/>
    /// prior to sending the calls to the underlying REST API.
    /// </summary>
    public interface INetworksValidator
    {
        /// <summary>
        /// Validates an IP address range (CIDR) formatted as a string.
        /// </summary>
        /// <param name="cidr">The IP address range.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="cidr"/> is <see langword="null"/>.</exception>
        /// <exception cref="CidrFormatException">If <paramref name="cidr"/> is not in the correct format.</exception>
        void ValidateCidr(string cidr);
    }
}
