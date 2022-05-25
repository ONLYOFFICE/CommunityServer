using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Server event statuses.
    /// </summary>
    /// <exclude />
    public class ServerEventStatus<T> : ResourceStatus
        where T : ServerEventStatus<T>, new()
    {
        /// <summary />
        public static readonly T Success = new T {DisplayName = "Success" };

        /// <summary />
        public static readonly T Error = new T {DisplayName = "Error", IsError = true};
    }
}
