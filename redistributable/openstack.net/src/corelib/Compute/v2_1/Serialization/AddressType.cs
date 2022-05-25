using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Server address types.
    /// </summary>
    /// <exclude />
    public class AddressType<T> : StringEnumeration
        where T : AddressType<T>, new()
    {
        /// <summary>
        /// Fixed IP address.
        /// </summary>
        public static readonly T Fixed = new T {DisplayName = "fixed"};

        /// <summary>
        /// Floating IP address.
        /// </summary>
        public static readonly T Floating = new T {DisplayName = "floating"};
    }
}
