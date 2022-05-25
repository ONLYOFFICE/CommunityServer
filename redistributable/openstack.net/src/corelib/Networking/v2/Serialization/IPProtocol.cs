using OpenStack.Serialization;

namespace OpenStack.Networking.v2.Serialization
{
    /// <summary>
    /// Internet Protocols.
    /// </summary>
    /// <exclude />
    public class IPProtocol<T> : StringEnumeration
        where T : IPProtocol<T>, new()
    {
        /// <summary>
        /// ICMP
        /// </summary>
        public static readonly T ICMP = new T {DisplayName = "icmp"};

        /// <summary>
        /// TCP
        /// </summary>
        public static readonly T TCP = new T {DisplayName = "tcp"};

        /// <summary>
        /// UDP
        /// </summary>
        public static readonly T UDP = new T {DisplayName = "udp"};
    }
}
