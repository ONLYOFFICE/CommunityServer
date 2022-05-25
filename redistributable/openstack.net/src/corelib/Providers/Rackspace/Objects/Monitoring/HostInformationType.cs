namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using System.Collections.Concurrent;
    using net.openstack.Core;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents a host information type in the monitoring service.
    /// </summary>
    /// <remarks>
    /// This class functions as a strongly-typed enumeration of known host information types,
    /// with added support for unknown types supported by a server extension.
    /// </remarks>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonConverter(typeof(HostInformationType.Converter))]
    public sealed class HostInformationType : ExtensibleEnum<HostInformationType>
    {
        private static readonly ConcurrentDictionary<string, HostInformationType> _types =
            new ConcurrentDictionary<string, HostInformationType>(StringComparer.OrdinalIgnoreCase);
        private static readonly HostInformationType _cpus = HostInformationType.FromName("cpus");
        private static readonly HostInformationType _disks = HostInformationType.FromName("disks");
        private static readonly HostInformationType _filesystems = HostInformationType.FromName("filesystems");
        private static readonly HostInformationType _memory = HostInformationType.FromName("memory");
        private static readonly HostInformationType _networkInterfaces = HostInformationType.FromName("network_interfaces");
        private static readonly HostInformationType _processes = HostInformationType.FromName("processes");
        private static readonly HostInformationType _system = HostInformationType.FromName("system");
        private static readonly HostInformationType _who = HostInformationType.FromName("who");

        /// <summary>
        /// Initializes a new instance of the <see cref="HostInformationType"/> class with the specified name.
        /// </summary>
        /// <inheritdoc/>
        private HostInformationType(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Gets the <see cref="HostInformationType"/> instance with the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The unique <see cref="HostInformationType"/> instance with the specified name.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">If <paramref name="name"/> is empty.</exception>
        public static HostInformationType FromName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name cannot be empty");

            return _types.GetOrAdd(name, i => new HostInformationType(i));
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on the host's CPUs.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetCpuInformationAsync"/>
        public static HostInformationType Cpus
        {
            get
            {
                return _cpus;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on the host's disks.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetDiskInformationAsync"/>
        public static HostInformationType Disks
        {
            get
            {
                return _disks;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on the host's filesystems.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetFilesystemInformationAsync"/>
        public static HostInformationType Filesystems
        {
            get
            {
                return _filesystems;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on the host's memory.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetMemoryInformationAsync"/>
        public static HostInformationType Memory
        {
            get
            {
                return _memory;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on the host's network interfaces.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetNetworkInterfaceInformationAsync"/>
        public static HostInformationType NetworkInterfaces
        {
            get
            {
                return _networkInterfaces;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on the host's processes.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetProcessInformationAsync"/>
        public static HostInformationType Processes
        {
            get
            {
                return _processes;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for system information for the host.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetSystemInformationAsync"/>
        public static HostInformationType System
        {
            get
            {
                return _system;
            }
        }

        /// <summary>
        /// Gets a <see cref="HostInformationType"/> for information on users who are logged into the host.
        /// </summary>
        /// <seealso cref="IMonitoringService.GetLoginInformationAsync"/>
        public static HostInformationType Who
        {
            get
            {
                return _who;
            }
        }

        /// <summary>
        /// Provides support for serializing and deserializing <see cref="HostInformationType"/>
        /// objects to JSON string values.
        /// </summary>
        /// <threadsafety static="true" instance="false"/>
        /// <preliminary/>
        private sealed class Converter : ConverterBase
        {
            /// <inheritdoc/>
            protected override HostInformationType FromName(string name)
            {
                return HostInformationType.FromName(name);
            }
        }
    }
}
