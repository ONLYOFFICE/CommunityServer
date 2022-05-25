namespace net.openstack.Core.Domain.Converters
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Sockets;
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// This implementation of <see cref="JsonConverter"/> allows for JSON serialization
    /// and deserialization of <see cref="IPAddress"/> objects in the "address details"
    /// format used by operations such as <see cref="IComputeProvider.ListAddresses"/>
    /// and  <see cref="IComputeProvider.ListAddressesByNetwork"/>.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses-d1e3014.html">List Addresses (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <seealso href="http://docs.openstack.org/api/openstack-compute/2/content/List_Addresses_by_Network-d1e3118.html">List Addresses by Network (OpenStack Compute API v2 and Extensions Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    public class IPAddressDetailsConverter : JsonConverter
    {
        /// <remarks>
        /// Serialization is performed by creating an <see cref="AddressDetails"/> instance
        /// equivalent to the given <see cref="IPAddress"/> instance and serializing that as
        /// a JSON object.
        /// </remarks>
        /// <inheritdoc/>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            IPAddress address = value as IPAddress;
            if (address == null)
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Unexpected value when converting IP address. Expected {0}, got {1}.", typeof(IPAddress), value.GetType()));

            AddressDetails details = new AddressDetails(address);
            serializer.Serialize(writer, details);
        }

        /// <remarks>
        /// Deserialization is performed by deserializing the JSON value as an <see cref="AddressDetails"/>
        /// object, following by using <see cref="IPAddress.Parse"/> to convert the value of
        /// <see cref="AddressDetails.Address"/> to an <see cref="IPAddress"/> instance.
        /// </remarks>
        /// <inheritdoc/>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (objectType != typeof(IPAddress))
                throw new JsonSerializationException(string.Format(CultureInfo.InvariantCulture, "Expected target type {0}, found {1}.", typeof(IPAddress), objectType));

            AddressDetails details = serializer.Deserialize<AddressDetails>(reader);
            if (details == null)
                return null;

            return IPAddress.Parse(details.Address);
        }

        /// <returns><see langword="true"/> if <paramref name="objectType"/> equals <see cref="IPAddress"/>; otherwise, <see langword="false"/>.</returns>
        /// <inheritdoc/>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(IPAddress);
        }

        /// <summary>
        /// Represents a network address in a format compatible with communication with the Compute Service APIs.
        /// </summary>
        /// <seealso cref="IComputeProvider.ListAddresses"/>
        /// <seealso cref="IComputeProvider.ListAddressesByNetwork"/>
        /// <threadsafety static="true" instance="false"/>
        [JsonObject(MemberSerialization.OptIn)]
        protected class AddressDetails : ExtensibleJsonObject
        {
            /// <summary>
            /// Gets the network address. This is an IPv4 address if <see cref="Version"/> is <c>"4"</c>,
            /// or an IPv6 address if <see cref="Version"/> is <c>"6"</c>.
            /// </summary>
            [JsonProperty("addr")]
            public string Address
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the network address version. The value is either <c>"4"</c> or <c>"6"</c>.
            /// </summary>
            [JsonProperty("version")]
            public string Version
            {
                get;
                private set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AddressDetails"/> class.
            /// </summary>
            /// <remarks>
            /// This constructor is used for JSON deserialization.
            /// </remarks>
            [JsonConstructor]
            protected AddressDetails()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AddressDetails"/> class
            /// using the given IP address.
            /// </summary>
            /// <param name="address">The IP address.</param>
            /// <exception cref="ArgumentNullException">If <paramref name="address"/> is <see langword="null"/>.</exception>
            public AddressDetails(IPAddress address)
            {
                if (address == null)
                    throw new ArgumentNullException("address");

                Address = address.ToString();
                switch (address.AddressFamily)
                {
                case AddressFamily.InterNetwork:
                    Version = "4";
                    break;

                case AddressFamily.InterNetworkV6:
                    Version = "6";
                    break;

                default:
                    throw new ArgumentException("The specified address family is not supported.");
                }
            }
        }
    }
}
