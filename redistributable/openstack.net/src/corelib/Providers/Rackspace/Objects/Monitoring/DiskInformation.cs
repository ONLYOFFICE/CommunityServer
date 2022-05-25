namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
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
    public class DiskInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="ReadBytes"/> property.
        /// </summary>
        [JsonProperty("read_bytes")]
        private long? _readBytes;

        /// <summary>
        /// This is the backing field for the <see cref="ReadCount"/> property.
        /// </summary>
        [JsonProperty("reads")]
        private long? _readCount;

        /// <summary>
        /// This is the backing field for the <see cref="ReadTime"/> property.
        /// </summary>
        [JsonProperty("rtime")]
        private long? _readTime;

        /// <summary>
        /// This is the backing field for the <see cref="WriteBytes"/> property.
        /// </summary>
        [JsonProperty("write_bytes")]
        private long? _writeBytes;

        /// <summary>
        /// This is the backing field for the <see cref="WriteCount"/> property.
        /// </summary>
        [JsonProperty("writes")]
        private long? _writeCount;

        /// <summary>
        /// This is the backing field for the <see cref="WriteTime"/> property.
        /// </summary>
        [JsonProperty("wtime")]
        private long? _writeTime;

        /// <summary>
        /// This is the backing field for the <see cref="IOTime"/> property.
        /// </summary>
        [JsonProperty("time")]
        private long? _time;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="DiskInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected DiskInformation()
        {
        }

        /// <summary>
        /// Gets the total number of bytes read from disk.
        /// </summary>
        public long? ReadBytes
        {
            get
            {
                return _readBytes;
            }
        }

        /// <summary>
        /// Gets the total number of completed read requests.
        /// </summary>
        public long? ReadCount
        {
            get
            {
                return _readCount;
            }
        }

        /// <summary>
        /// Gets the total time spent reading from disk.
        /// </summary>
        public TimeSpan? ReadTime
        {
            get
            {
                if (_readTime == null)
                    return null;

                return TimeSpan.FromMilliseconds(_readTime.Value);
            }
        }

        /// <summary>
        /// Gets the total number of bytes written to disk.
        /// </summary>
        public long? WriteBytes
        {
            get
            {
                return _writeBytes;
            }
        }

        /// <summary>
        /// Gets the total number of completed write requests.
        /// </summary>
        public long? WriteCount
        {
            get
            {
                return _writeCount;
            }
        }

        /// <summary>
        /// Gets the total time spent writing to disk.
        /// </summary>
        public TimeSpan? WriteTime
        {
            get
            {
                if (_writeTime == null)
                    return null;

                return TimeSpan.FromMilliseconds(_writeTime.Value);
            }
        }

        /// <summary>
        /// Gets the total time spent on disk I/O operations.
        /// </summary>
        public TimeSpan? IOTime
        {
            get
            {
                if (_time == null)
                    return null;

                return TimeSpan.FromMilliseconds(_time.Value);
            }
        }

        /// <summary>
        /// Gets the device name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }
    }
}
