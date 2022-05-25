namespace net.openstack.Providers.Rackspace.Objects.Databases
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class represents the volume configuration for a database instance. For
    /// running instances, the volume configuration also reports the approximate
    /// fraction of volume space currently in use by the database instance.
    /// </summary>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class DatabaseVolumeConfiguration : ExtensibleJsonObject
    {
        /// <summary>
        /// This is the backing field for the <see cref="Size"/> property.
        /// </summary>
        [JsonProperty("size", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? _size;

#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Used"/> property.
        /// </summary>
        [JsonProperty("used", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private double? _used;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseVolumeConfiguration"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DatabaseVolumeConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseVolumeConfiguration"/> class
        /// with the specified size.
        /// </summary>
        /// <param name="size">The size of the database volume in GB</param>
        /// <exception cref="ArgumentOutOfRangeException">If <paramref name="size"/> is less than or equal to 0.</exception>
        public DatabaseVolumeConfiguration(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size");

            _size = size;
        }

        /// <summary>
        /// Gets the size of the database instance volume in GB.
        /// </summary>
        /// <value>
        /// The size of the database instance volume in GB, or <see langword="null"/> if the size is not available.
        /// </value>
        public int? Size
        {
            get
            {
                return _size;
            }
        }

        /// <summary>
        /// Gets an approximation of the amount of space currently in use.
        /// </summary>
        /// <value>
        /// A value between 0 and 1 indicating the approximate fraction of disk space currently
        /// in use on the database volume, or <see langword="null"/> if the current usage is not available.
        /// </value>
        public double? Used
        {
            get
            {
                return _used;
            }
        }
    }
}
