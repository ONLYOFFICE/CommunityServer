using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Server statuses.
    /// </summary>
    /// <exclude />
    public class ServerStatus<T> : ResourceStatus
        where T : ServerStatus<T>, new()
    {
        /// <summary>
        /// The server is active.
        /// </summary>
        public static readonly T Active = new T {DisplayName = "ACTIVE"};

        /// <summary>
        /// The server has not finished the original build process.
        /// </summary>
        public static readonly T Building = new T {DisplayName = "BUILDING"};

        /// <summary>
        /// The server is permanently deleted.
        /// </summary>
        public static readonly T Deleted = new T {DisplayName = "DELETED"};

        /// <summary>
        /// The server is in Error.
        /// </summary>
        public static readonly T Error = new T {DisplayName = "ERROR", IsError = true};

        /// <summary>
        /// The server is hard rebooting. This is equivalent to pulling the power plug on a physical server, plugging it back in, and rebooting it.
        /// </summary>
        public static readonly T HardReboot = new T {DisplayName = "HARD_REBOOT"};

        /// <summary>
        /// The server is being migrated to a new host.
        /// </summary>
        public static readonly T Migrating = new T {DisplayName = "MIGRATING"};

        /// <summary>
        /// The Password is being reset on the server.
        /// </summary>
        public static readonly T Password = new T {DisplayName = "PASSWORD"};

        /// <summary>
        /// In a Paused state, the state of the server is stored in RAM.A Paused server continues to run in frozen state.
        /// </summary>
        public static readonly T Paused = new T {DisplayName = "PAUSED"};

        /// <summary>
        /// The server is in a soft Reboot state. A Reboot command was passed to the operating system.
        /// </summary>
        public static readonly T Reboot = new T {DisplayName = "REBOOT"};

        /// <summary>
        /// The server is currently being rebuilt from an image.
        /// </summary>
        public static readonly T Rebuild = new T {DisplayName = "REBUILD"};

        /// <summary>
        /// The server is in rescue mode. A rescue image is running with the original server image attached.
        /// </summary>
        public static readonly T Rescue = new T {DisplayName = "RESCUE"};

        /// <summary>
        /// Server is performing the differential copy of data that changed during its initial copy. Server is down for this stage.
        /// </summary>
        public static readonly T Resizing = new T {DisplayName = "RESIZED"};

        /// <summary>
        /// The resize or migration of a server failed for some reason. The destination server is being cleaned up and the original source server is restarting.
        /// </summary>
        public static readonly T RevertResize = new T {DisplayName = "REVERT_RESIZE"};

        /// <summary>
        /// The server is marked as deleted but the disk images are still available to restore.
        /// </summary>
        public static readonly T SoftDeleted = new T {DisplayName = "SOFT_DELETED"};

        /// <summary>
        /// The server is powered off and the disk image still persists.
        /// </summary>
        public static readonly T Stopped = new T {DisplayName = "SHUTOFF"};

        /// <summary>
        /// The server is suspended, either by request or necessity.
        /// </summary>
        public static readonly T Suspended = new T {DisplayName = "SUSPENDED"};

        /// <summary>
        /// The state of the server is unknown.Contact your cloud provider.
        /// </summary>
        public static readonly T Unknown = new T {DisplayName = "UNKNOWN"};

        /// <summary>
        /// System is awaiting confirmation that the server is operational after a move or resize.
        /// </summary>
        public static readonly T VerifyResize = new T {DisplayName = "VERIFY_RESIZE"};

    }
}
