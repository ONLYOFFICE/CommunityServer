using System.Collections.Generic;

namespace OpenStack.Networking.v2.Layer3
{
    /// <summary>
    /// Optional filter and paging options when listing ports.
    /// </summary>
    public class FloatingIPListOptions : FilterOptions
    {
        /// <summary>
        /// Filter by status.
        /// </summary>
        public FloatingIPStatus Status { get; set; }

        /// <summary>
        /// Filter by the network that contains the floating IP address.
        /// </summary>
        public Identifier NetworkId { get; set; }

        /// <summary>
        /// Filter by the associated router.
        /// </summary>
        public Identifier RouterId { get; set; }

        /// <summary>
        /// Filter by the associated port.
        /// </summary>
        public Identifier PortId { get; set; }

        /// <summary>
        /// Filter by the floating ip address.
        /// </summary>
        public string FloatingIPAddress { get; set; }

        /// <summary>
        /// Filter by the associated fixed ip address.
        /// </summary>
        public string FixedIPAddress { get; set; }

        /// <summary />
        protected override IDictionary<string, object> BuildQueryString()
        {
            var queryString = new Dictionary<string, object>
            {
                ["floating_network_id"] = NetworkId,
                ["router_id"] = RouterId,
                ["port_id"] = PortId,
                ["floating_ip_address"] = FloatingIPAddress,
                ["fixed_ip_address"] = FixedIPAddress,
                ["status"] = Status
            };

            return queryString;
        }
    }
}
