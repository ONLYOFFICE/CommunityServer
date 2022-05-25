using OpenStack.Serialization;

namespace OpenStack.Compute.v2_6
{
    /// <summary>
    /// The remote console protocol
    /// </summary>
    public class ConsoleProtocol : StringEnumeration
    {
        /// <summary />
        protected ConsoleProtocol()
        { }

        /// <summary />
        protected ConsoleProtocol(string displayName)
            : base(displayName)
        { }

        /// <summary>
        /// VNC
        /// </summary>
        public static readonly ConsoleProtocol VNC = new ConsoleProtocol("vnc");

        /// <summary>
        /// RDP
        /// </summary>
        public static readonly ConsoleProtocol RDP = new ConsoleProtocol("rdp-html5");

        /// <summary>
        /// Serial
        /// </summary>
        public static readonly ConsoleProtocol Serial = new ConsoleProtocol("serial");

        /// <summary>
        /// Spice
        /// </summary>
        public static readonly ConsoleProtocol Spice = new ConsoleProtocol("spice-html5");
    }
}