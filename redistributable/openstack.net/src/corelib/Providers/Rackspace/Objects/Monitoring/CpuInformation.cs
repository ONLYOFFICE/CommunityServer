namespace net.openstack.Providers.Rackspace.Objects.Monitoring
{
    using System;
    using Newtonsoft.Json;
    using ExtensibleJsonObject = net.openstack.Core.Domain.ExtensibleJsonObject;

    /// <summary>
    /// This class models the JSON representation of the CPU data reported agents for
    /// the <see cref="HostInformationType.Cpus"/> information type.
    /// </summary>
    /// <see cref="IMonitoringService.GetCpuInformationAsync"/>
    /// <threadsafety static="true" instance="false"/>
    /// <preliminary/>
    [JsonObject(MemberSerialization.OptIn)]
    public class CpuInformation : ExtensibleJsonObject
    {
#pragma warning disable 649 // Field 'fieldName' is never assigned to, and will always have its default value {value}
        /// <summary>
        /// This is the backing field for the <see cref="Name"/> property.
        /// </summary>
        [JsonProperty("name")]
        private string _name;

        /// <summary>
        /// This is the backing field for the <see cref="Vendor"/> property.
        /// </summary>
        [JsonProperty("vendor")]
        private string _vendor;

        /// <summary>
        /// This is the backing field for the <see cref="Model"/> property.
        /// </summary>
        [JsonProperty("model")]
        private string _model;

        /// <summary>
        /// This is the backing field for the <see cref="Frequency"/> property.
        /// </summary>
        [JsonProperty("mhz")]
        private int? _mhz;

        /// <summary>
        /// This is the backing field for the <see cref="IdleTime"/> property.
        /// </summary>
        [JsonProperty("idle")]
        private long? _idle;

        /// <summary>
        /// This is the backing field for the <see cref="InterruptTime"/> property.
        /// </summary>
        [JsonProperty("irq")]
        private long? _irq;

        /// <summary>
        /// This is the backing field for the <see cref="SoftInterruptTime"/> property.
        /// </summary>
        [JsonProperty("soft_irq")]
        private long? _softIrq;

        /// <summary>
        /// This is the backing field for the <see cref="LowPriorityTime"/> property.
        /// </summary>
        [JsonProperty("nice")]
        private long? _nice;

        /// <summary>
        /// This is the backing field for the <see cref="System"/> property.
        /// </summary>
        [JsonProperty("sys")]
        private long? _sys;

        /// <summary>
        /// This is the backing field for the <see cref="UserTime"/> property.
        /// </summary>
        [JsonProperty("user")]
        private long? _user;

        /// <summary>
        /// This is the backing field for the <see cref="WaitTime"/> property.
        /// </summary>
        [JsonProperty("wait")]
        private long? _wait;

        /// <summary>
        /// This is the backing field for the <see cref="TotalTime"/> property.
        /// </summary>
        [JsonProperty("total")]
        private long? _total;

        /// <summary>
        /// This is the backing field for the <see cref="ProcessorCount"/> property.
        /// </summary>
        [JsonProperty("total_cores")]
        private int? _totalCores;

        /// <summary>
        /// This is the backing field for the <see cref="SocketCount"/> property.
        /// </summary>
        [JsonProperty("total_sockets")]
        private int? _totalSockets;
#pragma warning restore 649

        /// <summary>
        /// Initializes a new instance of the <see cref="CpuInformation"/> class
        /// during JSON deserialization.
        /// </summary>
        [JsonConstructor]
        protected CpuInformation()
        {
        }

        /// <summary>
        /// Gets the CPU name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the CPU vendor name.
        /// </summary>
        public string Vendor
        {
            get
            {
                return _vendor;
            }
        }

        /// <summary>
        /// Gets the CPU vendor model string.
        /// </summary>
        public string Model
        {
            get
            {
                return _model;
            }
        }

        /// <summary>
        /// Gets the CPU clock speed in MHz.
        /// </summary>
        public int? Frequency
        {
            get
            {
                return _mhz;
            }
        }

        /// <summary>
        /// Gets the CPU time spent idle.
        /// </summary>
        public TimeSpan? IdleTime
        {
            get
            {
                if (_idle == null)
                    return null;

                return TimeSpan.FromMilliseconds(_idle.Value);
            }
        }

        /// <summary>
        /// Gets the CPU time spent servicing/handling hardware interrupts.
        /// </summary>
        public TimeSpan? InterruptTime
        {
            get
            {
                if (_irq == null)
                    return null;

                return TimeSpan.FromMilliseconds(_irq.Value);
            }
        }

        /// <summary>
        /// Gets the CPU time spent servicing/handling software interrupts.
        /// </summary>
        public TimeSpan? SoftInterruptTime
        {
            get
            {
                if (_softIrq == null)
                    return null;

                return TimeSpan.FromMilliseconds(_softIrq.Value);
            }
        }

        /// <summary>
        /// Gets the CPU time spent on low-priority processes.
        /// </summary>
        public TimeSpan? LowPriorityTime
        {
            get
            {
                if (_nice == null)
                    return null;

                return TimeSpan.FromMilliseconds(_nice.Value);
            }
        }

        /// <summary>
        /// Gets the CPU time spent in kernel space.
        /// </summary>
        public TimeSpan? KernelTime
        {
            get
            {
                if (_sys == null)
                    return null;

                return TimeSpan.FromMilliseconds(_sys.Value);
            }
        }

        /// <summary>
        /// Gets the CPU time spent in user space.
        /// </summary>
        public TimeSpan? UserTime
        {
            get
            {
                if (_user == null)
                    return null;

                return TimeSpan.FromMilliseconds(_user.Value);
            }
        }

        /// <summary>
        /// Gets the CPU time spent waiting on I/O operations to complete.
        /// </summary>
        public TimeSpan? WaitTime
        {
            get
            {
                if (_wait == null)
                    return null;

                return TimeSpan.FromMilliseconds(_wait.Value);
            }
        }

        /// <summary>
        /// Gets the total CPU time.
        /// </summary>
        public TimeSpan? TotalTime
        {
            get
            {
                if (_total == null)
                    return null;

                return TimeSpan.FromMilliseconds(_total.Value);
            }
        }

        /// <summary>
        /// Gets the total number of processor cores on all sockets.
        /// </summary>
        public int? ProcessorCount
        {
            get
            {
                return _totalCores;
            }
        }

        /// <summary>
        /// Gets the total number of CPU sockets.
        /// </summary>
        public int? SocketCount
        {
            get
            {
                return _totalSockets;
            }
        }
    }
}
