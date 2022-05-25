using OpenStack.Serialization;

namespace OpenStack.Images.v2.Serialization
{
    /// <summary>
    /// Server image status.
    /// </summary>
    /// <exclude />
    public class ImageStatus<T> : ResourceStatus
        where T : ImageStatus<T>, new()
    {
        /// <summary />
        public static readonly T Active = new T {DisplayName = "ACTIVE"};

        /// <summary />
        public static readonly T Saving = new T {DisplayName = "SAVING"};

        /// <summary />
        public static readonly T Deleted = new T {DisplayName = "DELETED"};

        /// <summary />
        public static readonly T Error = new T {DisplayName = "ERROR", IsError = true};

        /// <summary />
        public static readonly T Unknown = new T {DisplayName = "UNKNOWN"};
    }
}
