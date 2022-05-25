using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1.Serialization
{
    /// <summary>
    /// Controls how the API partitions the disk when you create, rebuild, or resize servers.
    /// </summary>
    /// <exclude />
    public class DiskConfiguration<T> : StringEnumeration
        where T : DiskConfiguration<T>, new()
    {
        /// <summary>
        /// The API builds the server with a single partition the size of the target flavor disk. The API automatically adjusts the file system to fit the entire partition. 
        /// </summary>
        public static readonly T Auto = new T {DisplayName = "AUTO"};

        /// <summary>
        /// The API builds the server by using whatever partition scheme and file system is in the source image. If the target flavor disk is larger, the API does not partition the remaining disk space.
        /// </summary>
        public static readonly T Manual = new T {DisplayName = "MANUAL"};
    }
}
