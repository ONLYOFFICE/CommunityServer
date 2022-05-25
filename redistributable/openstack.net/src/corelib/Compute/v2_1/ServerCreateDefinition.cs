using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Newtonsoft.Json;
using OpenStack.Compute.v2_1.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Defines a new server instance.
    /// </summary>
    [JsonConverter(typeof(ServerCreateDefinitionConverter))]
    public class ServerCreateDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCreateDefinition"/> class.
        /// </summary>
        /// <param name="name">The server name.</param>
        /// <param name="imageId">The image identifier.</param>
        /// <param name="flavorId">The flavor identifier.</param>
        public ServerCreateDefinition(string name, Identifier imageId, Identifier flavorId)
        {
            Name = name;
            ImageId = imageId;
            FlavorId = flavorId;
            SecurityGroups = new List<SecurityGroupReference>();
            Networks = new List<ServerNetworkDefinition>();
            Metadata = new Dictionary<string, string>();
            Personality = new List<Personality>();
            BlockDeviceMapping = new List<ServerBlockDeviceMapping>();
        }

        /// <inheritdoc cref="ServerSummary.Name" />
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <inheritdoc cref="Server.Image" />
        [JsonProperty("imageRef")]
        public Identifier ImageId { get; set; }

        /// <inheritdoc cref="Server.Flavor" />
        [JsonProperty("flavorRef")]
        public Identifier FlavorId { get; set; }

        /// <inheritdoc cref="Server.SecurityGroups" />
        [JsonProperty("security_groups")]
        public IList<SecurityGroupReference> SecurityGroups { get; set; }

        /// <inheritdoc cref="Server.AvailabilityZone" />
        [JsonProperty("availability_zone")]
        public string AvailabilityZone { get; set; }

        /// <summary>
        /// Configuration information or scripts to use upon launch. Must be Base64 encoded.
        /// </summary>
        [JsonProperty("user_data")]
        public string UserData { get; set; }

        /// <summary>
        /// Specifies the networks to which the server should be attached.
        /// </summary>
        [JsonProperty("networks")]
        public IList<ServerNetworkDefinition> Networks { get; set; }

        /// <inheritdoc cref="Server.Metadata" />
        [JsonProperty("metadata")]
        public IDictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// The file path and contents, text only, to inject into the server at launch.
        /// <para>The maximum size of the file path data is 255 bytes.
        /// The maximum limit is The number of allowed bytes in the decoded, rather than encoded, data.</para>
        /// </summary>
        [JsonProperty("personality")]
        public IList<Personality> Personality { get; set; }

        /// <summary>
        /// Enables you to boot a server from a volume.
        /// </summary>
        [JsonProperty("block_device_mapping_v2")]
        public IList<ServerBlockDeviceMapping> BlockDeviceMapping { get; set; }

        /// <summary>
        /// Indicates whether a configuration drive enables metadata injection. 
        /// </summary>
        [JsonProperty("config_drive")]
        public bool ShouldUseConfigurationDrive { get; set; }

        /// <inheritdoc cref="Server.KeyPairName" />
        [JsonProperty("key_name")]
        public string KeyPairName { get; set; }

        /// <summary>
        /// Specifies hints for the compute scheduler.
        /// </summary>
        [JsonIgnore] // Serialized at the same level as "server"
        public SchedulerHints SchedulerHints { get; set; }

        /// <inheritdoc cref="Server.DiskConfig" />
        [JsonProperty("OS-DCF:diskConfig")]
        public DiskConfiguration DiskConfig { get; set; }

        /// <summary>
        /// Load the UserData from the specified file path.
        /// </summary>
        /// <param name="path">The user data file path.</param>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive). </exception>
        /// <exception cref="IOException">An I/O error occurred while opening the file. </exception>
        /// <exception cref="UnauthorizedAccessException">This operation is not supported on the current platform.-or- <paramref name="path" /> specified a directory.-or- The caller does not have the required permission. </exception>
        /// <exception cref="FileNotFoundException">The file specified in <paramref name="path" /> was not found. </exception>
        /// <exception cref="SecurityException">The caller does not have the required permission. </exception>
        public void LoadUserDataFromFile(string path)
        {
            byte[] contents = File.ReadAllBytes(path);
            UserData = Convert.ToBase64String(contents);
        }

        /// <summary>
        /// Configures the server to boot from an existing volume.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="deleteVolumeWithServer">if set to <c>true</c> [delete volume with server].</param>
        public void ConfigureBootFromVolume(Identifier volumeId, bool deleteVolumeWithServer = false)
        {
            BlockDeviceMapping.Add(new ServerBlockDeviceMapping
            {
                SourceType = ServerBlockDeviceType.Volume,
                SourceId = volumeId,
                BootIndex = 0,
                DeleteWithServer = deleteVolumeWithServer
            });
        }

        /// <summary>
        /// Configures the server to boot from a copy of an existing volume.
        /// </summary>
        /// <param name="volumeId">The volume identifier.</param>
        /// <param name="volumeSize">Size of the volume.</param>
        /// <param name="deleteVolumeWithServer">if set to <c>true</c> [delete volume with server].</param>
        public void ConfigureBootFromNewVolume(Identifier volumeId, int volumeSize, bool deleteVolumeWithServer = false)
        {
            BlockDeviceMapping.Add(new ServerBlockDeviceMapping
            {
                SourceType = ServerBlockDeviceType.Volume,
                SourceId = volumeId,
                BootIndex = 0,
                DestinationType = ServerBlockDeviceType.Volume,
                DestinationVolumeSize = volumeSize,
                DeleteWithServer = deleteVolumeWithServer
            });
        }

        /// <summary>
        /// Configures the server to boot from a new volume, copied from the base server image.
        /// </summary>
        /// <param name="volumeSize">Size of the volume.</param>
        /// <param name="deleteVolumeWithServer">if set to <c>true</c> [delete volume with server].</param>
        public void ConfigureBootFromNewVolume(int volumeSize, bool deleteVolumeWithServer = false)
        {
            BlockDeviceMapping.Add(new ServerBlockDeviceMapping
            {
                SourceType = ServerBlockDeviceType.Image,
                SourceId = ImageId,
                BootIndex = 0,
                DestinationType = ServerBlockDeviceType.Volume,
                DestinationVolumeSize = volumeSize,
                DeleteWithServer = deleteVolumeWithServer
            });
        }
    }
}