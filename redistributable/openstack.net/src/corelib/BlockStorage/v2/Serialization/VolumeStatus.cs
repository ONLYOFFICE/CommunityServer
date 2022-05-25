using OpenStack.Serialization;

namespace OpenStack.BlockStorage.v2.Serialization
{
    /// <summary>
    /// Volume status.
    /// </summary>
    /// <exclude />
    public class VolumeStatus<T> : ResourceStatus
        where T : VolumeStatus<T>, new()
    {
        /// <summary>
        /// The volume is being created.
        /// </summary>
        public static readonly T Creating = new T {DisplayName = "creating"};

        /// <summary>
        /// The volume is ready to attach to an instance.
        /// </summary>
        public static readonly T Available = new T {DisplayName = "available"};

        /// <summary>
        /// The volume is attaching to an instance.
        /// </summary>
        public static readonly T Attaching = new T {DisplayName = "attaching"};

        /// <summary>
        /// The volume is attached to an instance.
        /// </summary>
        public static readonly T InUse = new T {DisplayName = "in-use"};

        /// <summary>
        /// The volume is being deleted.
        /// </summary>
        public static readonly T Deleting = new T {DisplayName = "deleting"};

        /// <summary>
        /// A volume creation error occurred.
        /// </summary>
        public static readonly T Error = new T {DisplayName = "error", IsError = true};

        /// <summary>
        /// A volume deletion error occurred.
        /// </summary>
        public static readonly T ErrorDeleting = new T {DisplayName = "error_deleting", IsError = true};

        /// <summary>
        /// A backup restoration error occurred.
        /// </summary>
        public static readonly T ErrorRestoring = new T {DisplayName = "error_restoring", IsError = true};

        /// <summary>
        /// An error occurred while attempting to extend a volume.
        /// </summary>
        public static readonly T ErrorExtending = new T {DisplayName = "error_extending", IsError = true};

        /// <summary>
        /// The volume is being backed up.
        /// </summary>
        public static readonly T BackingUp = new T {DisplayName = "backing-up"};

        /// <summary>
        /// A backup is being restored to the volume.
        /// </summary>
        public static readonly T RestoringBackup = new T {DisplayName = "restoring-backup"};
    }
}
