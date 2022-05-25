namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the disk data reported agents for
    /// the <see cref="HostInformationType.Disks"/> information type.
    /// </summary>
    /// <see cref="IMonitoringService.GetDiskInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class MemoryInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Available"/> property.
        /// </summary>
        private long? _ram;

        /// <summary>
        /// This is the backing field for the <see cref="ActualFree"/> property.
        /// </summary>
        private long? _actualFree;

        /// <summary>
        /// This is the backing field for the <see cref="ActualUsed"/> property.
        /// </summary>
        private long? _actualUsed;

        /// <summary>
        /// This is the backing field for the <see cref="Free"/> property.
        /// </summary>
        private long? _free;

        /// <summary>
        /// This is the backing field for the <see cref="Used"/> property.
        /// </summary>
        private long? _used;

        /// <summary>
        /// This is the backing field for the <see cref="SwapFree"/> property.
        /// </summary>
        private long? _swapFree;

        /// <summary>
        /// This is the backing field for the <see cref="SwapUsed"/> property.
        /// </summary>
        private long? _swapUsed;

        /// <summary>
        /// This is the backing field for the <see cref="SwapPageIn"/> property.
        /// </summary>
        private long? _swapPageIn;

        /// <summary>
        /// This is the backing field for the <see cref="SwapPageOut"/> property.
        /// </summary>
        private long? _swapPageOut;

        /// <summary>
        /// This is the backing field for the <see cref="SwapTotal"/> property.
        /// </summary>
        private long? _swapTotal;

        /// <summary>
        /// This is the backing field for the <see cref="Total"/> property.
        /// </summary>
        private long? _total;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected MemoryInformation()
        {
        }

        /// <summary>
        /// Gets the number of bytes of RAM available to the system.
        /// </summary>
        public long? Available
        {
            get
            {
                return _ram * 1024 * 1024;
            }
        }

        /// <summary>
        /// Gets the number of bytes of free memory. This property counts purgeable operating system caches as free.
        /// </summary>
        public long? ActualFree
        {
            get
            {
                return _actualFree;
            }
        }

        /// <summary>
        /// Gets the number of bytes of used memory. This property counts purgeable operating system caches as free.
        /// </summary>
        public long? ActualUsed
        {
            get
            {
                return _actualUsed;
            }
        }

        /// <summary>
        /// Gets the number of bytes of free memory. This property counts purgeable operating system caches as used.
        /// </summary>
        public long? Free
        {
            get
            {
                return _free;
            }
        }

        /// <summary>
        /// Gets the number of bytes of used memory. This property counts purgeable operating system caches as used.
        /// </summary>
        public long? Used
        {
            get
            {
                return _used;
            }
        }

        /// <summary>
        /// Gets the number of bytes of free swap space.
        /// </summary>
        public long? SwapFree
        {
            get
            {
                return _swapFree;
            }
        }

        /// <summary>
        /// Gets the number of bytes of used swap space.
        /// </summary>
        public long? SwapUsed
        {
            get
            {
                return _swapUsed;
            }
        }

        /// <summary>
        /// Gets the number of pages swapped in.
        /// </summary>
        public long? SwapPageIn
        {
            get
            {
                return _swapPageIn;
            }
        }

        /// <summary>
        /// Gets the number of pages swapped out.
        /// </summary>
        public long? SwapPageOut
        {
            get
            {
                return _swapPageOut;
            }
        }

        /// <summary>
        /// Gets the total number of bytes of swap space.
        /// </summary>
        public long? SwapTotal
        {
            get
            {
                return _swapTotal;
            }
        }

        /// <summary>
        /// Gets the total number of bytes of memory.
        /// </summary>
        public long? Total
        {
            get
            {
                return _total;
            }
        }
    }
}
