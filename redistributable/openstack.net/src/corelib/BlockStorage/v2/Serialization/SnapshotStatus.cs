using OpenStack.Serialization;

namespace OpenStack.BlockStorage.v2.Serialization
{
    /// <summary>
    /// Volume snapshot status.
    /// </summary>
    /// <exclude />
    public class SnapshotStatus<T> : ResourceStatus
        where T : SnapshotStatus<T>, new()
    {
        /// <summary>
        /// The snapshot is being created.
        /// </summary>
        public static readonly T Creating = new T {DisplayName = "creating"};

        /// <summary>
        /// The snapshot is ready to use.
        /// </summary>
        public static readonly T Available = new T {DisplayName = "available"};

        /// <summary>
        /// The snapshot is being deleted.
        /// </summary>
        public static readonly T Deleting = new T {DisplayName = "deleting"};

        /// <summary>
        /// A snapshot creation error occurred.
        /// </summary>
        public static readonly T Error = new T {DisplayName = "error", IsError = true};

        /// <summary>
        /// A snapshot deletion error occurred.
        /// </summary>
        public static readonly T ErrorDeleting = new T {DisplayName = "error_deleting", IsError = true};
    }
}
