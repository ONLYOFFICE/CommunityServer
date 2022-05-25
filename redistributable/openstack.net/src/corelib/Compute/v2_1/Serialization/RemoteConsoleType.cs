using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// The remote console type
    /// </summary>
    /// <exclude />
    public class RemoteConsoleType<T> : StringEnumeration
        where T : RemoteConsoleType<T>, new()
    {
        /// <summary>
        /// noVNC
        /// </summary>
        public static T NoVnc = new T {DisplayName = "novnc"};

        /// <summary>
        /// XVP VNC
        /// </summary>
        public static T XvpVnc = new T {DisplayName = "xvpvnc"};

        /// <summary>
        /// RDP
        /// </summary>
        public static T RdpHtml5 = new T {DisplayName = "rdp-html5"};

        /// <summary>
        /// Serial
        /// </summary>
        public static T Serial = new T {DisplayName = "serial"};

        /// <summary>
        /// Spice HTML5
        /// </summary>
        public static T SpiceHtml5 = new T {DisplayName = "spice-html5"};
    }
}
