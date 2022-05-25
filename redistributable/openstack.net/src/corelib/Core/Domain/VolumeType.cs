namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the type of a volume in the Block Storage service.
    /// </summary>
    /// <seealso cref="IBlockStorageProvider.ListVolumeTypes"/>
    /// <seealso cref="IBlockStorageProvider.DescribeVolumeType"/>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class VolumeType : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the unique identifier for this volume type.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name of the volume type.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; private set; }
    }
}
