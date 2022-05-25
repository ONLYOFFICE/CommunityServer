namespace net.openstack.Core.Domain.Converters
{
    using System.Net;
    using Newtonsoft.Json;

    /// <summary>
    /// This implementation of <see cref="JsonConverter"/> allows for JSON serialization
    /// and deserialization of <see cref="IPAddress"/> objects using a simple string
    /// representation. Serialization is performed using <see cref="IPAddress.ToString"/>,
    /// and deserialization is performed using <see cref="IPAddress.Parse"/>.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses-d1e3014.html">List Addresses (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses_by_Network-d1e3118.html">List Addresses by Network (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    public class IPAddressSimpleConverter : SimpleStringJsonConverter<IPAddress>
    {
        /// <remarks>
        /// If <paramref name="str"/> is an empty string, this method returns <see langword="null"/>.
        /// Otherwise, this method uses <see cref="IPAddress.Parse"/> for deserialization.
        /// </remarks>
        /// <inheritdoc/>
        protected override IPAddress ConvertToObject(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            return IPAddress.Parse(str);
        }
    }
}
