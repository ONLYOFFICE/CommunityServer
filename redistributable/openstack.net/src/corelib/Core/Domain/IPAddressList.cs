namespace net.openstack.Core.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using net.openstack.Core.Domain.Converters;
    using Newtonsoft.Json;

    /// <summary>
    /// This implementation of <see cref="List{T}"/> is used to ensure the elements
    /// are deserialized from a JSON string using the <see cref="IPAddressDetailsConverter"/>.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    [JsonArray(ItemConverterType = typeof(IPAddressDetailsConverter))]
    public class IPAddressList : List<IPAddress>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressList"/> class.
        /// that is empty and has the default initial capacity.
        /// </summary>
        public IPAddressList()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressList"/> class
        /// that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="capacity"/> is less than 0.</exception>
        public IPAddressList(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPAddressList"/> class that
        /// contains elements copied from the specified <paramref name="collection"/>
        /// and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="collection"/> is <see langword="null"/>.</exception>
        public IPAddressList(IEnumerable<IPAddress> collection)
            : base(collection)
        {
        }
    }
}
