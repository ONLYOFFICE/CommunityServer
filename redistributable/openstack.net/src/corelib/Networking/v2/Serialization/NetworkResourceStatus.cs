using OpenStack.Serialization;

namespace OpenStack.Networking.v2
{
    /// <summary>
    /// Networking resource status. Applies to networks, routers, interfaces, floating ips etc.
    /// </summary>
    /// <exclude />
    public class NetworkResourceStatus<T> : ResourceStatus
        where T : NetworkResourceStatus<T>, new()
    {
        /// <summary>
        /// The resource is in an unknown state.
        /// </summary>
        public static readonly T Unknown = new T { DisplayName = "UNKNOWN" };

        /// <summary>
        /// The resource is active.
        /// </summary>
        public static readonly T Active = new T { DisplayName = "ACTIVE" };

        /// <summary>
        /// The resource is unavilable.
        /// </summary>
        public static readonly T Down = new T { DisplayName = "DOWN" };
    }
}