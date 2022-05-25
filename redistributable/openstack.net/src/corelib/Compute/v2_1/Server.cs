using System;
using System.Collections.Generic;
using System.Extensions;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OpenStack.Compute.v2_1.Serialization;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// A virtual machine (VM) instance running on a host.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "server")]
    public class Server : ServerSummary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Server"/> class.
        /// </summary>
        public Server()
        {
            Addresses = new Dictionary<string, IList<ServerAddress>>();
            AttachedVolumes = new List<ServerVolumeReference>();
            Metadata = new ServerMetadata();
            SecurityGroups = new List<SecurityGroupReference>();
        }

        private string _adminPassword;

        /// <summary>
        /// The IP addresses for the server.
        /// </summary>
        [JsonProperty("addresses")]
        public IDictionary<string, IList<ServerAddress>> Addresses { get; set; }

        /// <summary>
        /// The flavor for the server instance.
        /// </summary>
        [JsonProperty("flavor")]
        public FlavorReference Flavor { get; set; }

        /// <summary>
        /// The date and time when the resource was created.
        /// </summary>
        [JsonProperty("created")]
        public DateTimeOffset? Created { get; set; }

        /// <summary>
        /// The image for the server instance.
        /// </summary>
        [JsonProperty("image")]
        public ImageReference Image { get; set; }

        /// <summary>
        /// The administrative password.
        /// <para>The password is only available immediately after creating the server, and otherwise is empty.</para>
        /// </summary>
        [JsonProperty("adminPass")]
        public string AdminPassword
        {
            get { return _adminPassword; }
            set
            {
                // This is only set once, then never again. Capture it for safekeeping
                _adminPassword = value ?? _adminPassword;
            }
        }

        /// <summary>
        /// The name of associated key pair, if any. 
        /// </summary>
        [JsonProperty("key_name")]
        public string KeyPairName { get; set; }

        /// <summary>
        /// The associated metadata key and value pairs.
        /// </summary>
        [JsonProperty("metadata")]
        public ServerMetadata Metadata { get; set; }

        /// <summary>
        /// The server v4 IP address.
        /// </summary>
        [JsonProperty("accessIPv4")]
        public string IPv4Address { get; set; }

        /// <summary>
        /// The server v6 IP address.
        /// </summary>
        [JsonProperty("accessIPv6")]
        public string IPv6Address { get; set; }

        /// <summary>
        /// The host identifier.
        /// </summary>
        [JsonProperty("hostId")]
        public Identifier HostId { get; set; }

        /// <summary>
        /// The server disk configuration.
        /// </summary>
        [JsonProperty("OS-DCF:diskConfig")]
        public DiskConfiguration DiskConfig { get; set; }

        /// <summary>
        /// The availability zone in which the server is located.
        /// </summary>
        [JsonProperty("OS-EXT-AZ:availability_zone")]
        public string AvailabilityZone { get; set; }

        // TODO: These are operator only. If only we had extension properties... (https://github.com/dotnet/roslyn/issues/112) Need to figure out how to handle stuff like this.
        //[JsonProperty("OS-EXT-SRV-ATTR:host")]
        //public string HostName { get; set; }
        //[JsonProperty("OS-EXT-SRV-ATTR:hypervisor_hostname")]
        //public string HypervisorHostName { get; set; }
        //[JsonProperty("OS-EXT-SRV-ATTR:instance_name")]
        //public string InstanceName { get; set; }

        /// <summary>
        /// The power state of the server.
        /// </summary>
        [JsonProperty("OS-EXT-STS:power_state")]
        public string PowerState { get; set; }

        /// <summary>
        /// The task state of the server.
        /// </summary>
        [JsonProperty("OS-EXT-STS:task_state")]
        public string TaskState { get; set; }

        /// <summary>
        /// The underlying VM state.
        /// </summary>
        [JsonProperty("OS-EXT-STS:vm_state")]
        public string VMState { get; set; }

        /// <summary>
        /// The date and time when the server was launched.
        /// </summary>
        [JsonProperty("OS-SRV-USG:launched_at")]
        public DateTimeOffset? Launched { get; set; }

        /// <summary>
        ///  The date and time when the server was deleted.
        /// </summary>
        [JsonProperty("OS-SRV-USG:terminated_at")]
        public DateTimeOffset? Deleted { get; set; }

        /// <summary>
        /// A percentage value of the build progress.
        /// </summary>
        [JsonProperty("progress")]
        public int Progress { get; set; }

        /// <summary>
        /// The attached volumes, if any.
        /// </summary>
        [JsonProperty("os-extended-volumes:volumes_attached")]
        public IList<ServerVolumeReference> AttachedVolumes { get; set; }

        /// <summary>
        /// Associated security groups.
        /// </summary>
        [JsonProperty("security_groups")]
        public IList<SecurityGroupReference> SecurityGroups { get; set; }

        /// <summary>
        /// The server status.
        /// </summary>
        [JsonProperty("status")]
        public ServerStatus Status { get; set; }

        /// <summary>
        /// The date and time when the resource was updated.
        /// </summary>
        [JsonProperty("updated")]
        public DateTimeOffset? LastModified { get; set; }

        /// <summary>
        /// Waits the until the server is active.
        /// </summary>
        /// <param name="refreshDelay">The refresh delay.</param>
        /// <param name="timeout">The timeout.</param>
        /// <param name="progress">The progress.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <exception cref="InvalidOperationException">When the <see cref="Server" /> instance was not constructed by the <see cref="ComputeService" />, as it is missing the appropriate internal state to execute service calls.</exception>
        public Task WaitUntilActiveAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WaitForStatusAsync(ServerStatus.Active, refreshDelay, timeout, progress, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.WaitUntilServerIsDeletedAsync{TServer,TStatus}" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public override async Task WaitUntilDeletedAsync(TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.WaitUntilDeletedAsync(refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            Status = ServerStatus.Deleted;
        }

        /// <inheritdoc cref="ComputeApi.WaitForServerStatusAsync{TServer,TStatus}(string,TStatus,TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatusAsync(ServerStatus status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForServerStatusAsync<Server, ServerStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.WaitForServerStatusAsync{TServer,TStatus}(string,IEnumerable{TStatus},TimeSpan?,TimeSpan?,IProgress{bool},CancellationToken)" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task WaitForStatusAsync(IEnumerable<ServerStatus> status, TimeSpan? refreshDelay = null, TimeSpan? timeout = null, IProgress<bool> progress = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var owner = this.GetOwnerOrThrow<ComputeApi>();
            var result = await owner.WaitForServerStatusAsync<Server, ServerStatus>(Id, status, refreshDelay, timeout, progress, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.UpdateServerAsync{T}" />
        /// <exception cref="InvalidOperationException">When the <see cref="Server"/> instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task UpdateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            var request = new ServerUpdateDefinition();
            this.CopyProperties(request);

            var result = await compute.UpdateServerAsync<Server>(Id, request, cancellationToken).ConfigureAwait(false);
            result.CopyProperties(this);
        }

        /// <inheritdoc cref="ComputeApi.AttachVolumeAsync{T}" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public async Task<ServerVolume> AttachVolumeAsync(ServerVolumeDefinition volume, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            var result = await compute.AttachVolumeAsync<ServerVolume>(Id, volume, cancellationToken).ConfigureAwait(false);
            AttachedVolumes.Add(result);
            ((IChildResource)result).SetParent(this);
            return result;
        }

        /// <inheritdoc cref="ComputeApi.AssociateFloatingIPAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public virtual async Task AssociateFloatingIPAsync(AssociateFloatingIPRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.AssociateFloatingIPAsync(Id, request, cancellationToken);

            Addresses = await compute.ListServerAddressesAsync<ServerAddressCollection>(Id, cancellationToken);
        }

        /// <inheritdoc cref="ComputeApi.DisassociateFloatingIPAsync" />
        /// <exception cref="InvalidOperationException">When this instance was not constructed by the <see cref="ComputeService"/>, as it is missing the appropriate internal state to execute service calls.</exception>
        public virtual async Task DisassociateFloatingIPAsync(string floatingIPAddress, CancellationToken cancellationToken = default(CancellationToken))
        {
            var compute = this.GetOwnerOrThrow<ComputeApi>();
            await compute.DisassociateFloatingIPAsync(Id, floatingIPAddress, cancellationToken);

            // Remove the address from the current instance immediately
            foreach (KeyValuePair<string, IList<ServerAddress>> group in Addresses)
            {
                foreach (ServerAddress address in group.Value)
                {
                    if (address.Type == AddressType.Floating && address.IP == floatingIPAddress)
                    {
                        Addresses[group.Key].Remove(address);
                        return;
                    }
                }
            }
        }

        /// <summary />
        [OnDeserialized]
        private void OnDeserializedMethod(StreamingContext context)
        {
            Metadata.SetParent(this);
            foreach (var volume in AttachedVolumes)
            {
                volume.SetParent(this);
            }
        }
    }
}