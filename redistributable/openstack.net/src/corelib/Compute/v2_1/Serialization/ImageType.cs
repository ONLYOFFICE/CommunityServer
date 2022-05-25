using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Server image types.
    /// </summary>
    /// <exclude />
    public class ImageType<T> : StringEnumeration
        where T : ImageType<T>, new()
    {
        /// <summary>
        /// Base Image
        /// </summary>
        public static readonly T Base = new T{DisplayName = "base"};

        /// <summary>
        /// Server Backup
        /// </summary>
        public static readonly T Backup = new T{DisplayName = "backup"};

        /// <summary>
        /// Server Snapshot
        /// </summary>
        public static readonly T Snapshot = new T{DisplayName = "snapshot"};
    }
}
