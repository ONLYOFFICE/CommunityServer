namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the filesystem data reported agents for
    /// the <see cref="HostInformationType.Filesystems"/> information type.
    /// </summary>
    /// <see cref="IMonitoringService.GetFilesystemInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class FilesystemInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="DirectoryName"/> property.
        /// </summary>
        [JsonProperty("dir_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _dirName;

        /// <summary>
        /// This is the backing field for the <see cref="Options"/> property.
        /// </summary>
        [JsonProperty("options", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _options;

        /// <summary>
        /// This is the backing field for the <see cref="DeviceName"/> property.
        /// </summary>
        [JsonProperty("dev_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _devName;

        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("sys_type_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private string _sysTypeName;

        /// <summary>
        /// This is the backing field for the <see cref="BytesUsed"/> property.
        /// </summary>
        [JsonProperty("used", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _used;

        /// <summary>
        /// This is the backing field for the <see cref="BytesAvailable"/> property.
        /// </summary>
        [JsonProperty("avail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _available;

        /// <summary>
        /// This is the backing field for the <see cref="BytesFree"/> property.
        /// </summary>
        [JsonProperty("free", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _free;

        /// <summary>
        /// This is the backing field for the <see cref="BytesTotal"/> property.
        /// </summary>
        [JsonProperty("total", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _total;

        /// <summary>
        /// This is the backing field for the <see cref="FileNodes"/> property.
        /// </summary>
        [JsonProperty("files", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _files;

        /// <summary>
        /// This is the backing field for the <see cref="AvailableFileNodes"/> property.
        /// </summary>
        [JsonProperty("free_files", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private long? _freeFiles;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="FilesystemInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected FilesystemInformation()
        {
        }

        /// <summary>
        /// Gets the name of the directory where the filesystem is mounted.
        /// </summary>
        public string DirectoryName
        {
            get
            {
                return _dirName;
            }
        }

        /// <summary>
        /// Gets the filesystem mount options.
        /// </summary>
        public string Options
        {
            get
            {
                return _options;
            }
        }

        /// <summary>
        /// Gets the name of the device from which this filesystem is mounted.
        /// </summary>
        public string DeviceName
        {
            get
            {
                return _devName;
            }
        }

        /// <summary>
        /// Gets the name of the filesystem.
        /// </summary>
        public string Name
        {
            get
            {
                return _sysTypeName;
            }
        }

        /// <summary>
        /// Gets the used space on the filesystem.
        /// </summary>
        public long? BytesUsed
        {
            get
            {
                return _used * 1024;
            }
        }

        /// <summary>
        /// Gets the available space on the filesystem.
        /// </summary>
        public long? BytesAvailable
        {
            get
            {
                return _available * 1024;
            }
        }

        /// <summary>
        /// Gets the free space available on the filesystem.
        /// </summary>
        public long? BytesFree
        {
            get
            {
                return _free * 1024;
            }
        }

        /// <summary>
        /// Gets the total space on the filesystem.
        /// </summary>
        public long? BytesTotal
        {
            get
            {
                return _total * 1024;
            }
        }

        /// <summary>
        /// Gets the number of file nodes on the filesystem.
        /// </summary>
        public long? FileNodes
        {
            get
            {
                return _files;
            }
        }

        /// <summary>
        /// Gets the number of free file nodes on the filesystem.
        /// </summary>
        public long? AvailableFileNodes
        {
            get
            {
                return _freeFiles;
            }
        }
    }
}
