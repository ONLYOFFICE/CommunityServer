using System.Collections.Generic;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Optional filter and paging options when listing ports.
    /// </summary>
    public class PortListOptions : FilterOptions
    {
        /// <summary>
        /// Filter by the associated device identifier.
        /// </summary>
        public Identifier DeviceId { get; set; }

        /// <summary>
        /// Filter by the entity that is using the port.
        /// </summary>
        public Identifier DeviceOwner { get; set; }

        /// <summary>
        /// Filter by the associated network.
        /// </summary>
        public Identifier NetworkId { get; set; }

        /// <summary>
        /// Filter by the MAC address.
        /// </summary>
        public string MACAddress { get; set; }

        /// <summary>
        /// Filter by the port status.
        /// </summary>
        public PortStatus Status { get; set; }

        /// <summary>
        /// Filter by the port name.
        /// </summary>
        public string Name { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = new Dictionary<string, object>
            {
                ["device_id"] = DeviceId,
                ["device_owner"] = DeviceOwner,
                ["network_id"] = NetworkId,
                ["mac_address"] = MACAddress,
                ["status"] = Status,
                ["display_name"] = Name
            };

            return queryString;
        }
    }
}
