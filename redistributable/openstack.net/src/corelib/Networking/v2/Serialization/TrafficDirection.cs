using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Direction of network traffic.
    /// </summary>
    /// <exclude />
    public class TrafficDirection<T> : StringEnumeration
        where T : TrafficDirection<T>, new()
    {
        /// <summary>
        /// Incoming network traffic (ingress).
        /// </summary>
        public static readonly T Incoming = new T { DisplayName = "ingress" };

        /// <summary>
        /// Outgoing network traffic (egress).
        /// </summary>
        public static readonly T Outgoing = new T { DisplayName = "egress" };
    }
}
