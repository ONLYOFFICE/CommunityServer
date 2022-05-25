namespace net.openstack.Core.Domain
{
    using net.openstack.Core.Providers;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the detailed information for a container stored in an Object Storage Provider.
    /// </summary>
    /// <seealso cref="IObjectStorageProvider"/>
    /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showAccountDetails_v1__account__storage_account_services.html">Show account details and list containers (OpenStack Object Storage API v1 Reference)</seealso>
    /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class Container : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets the name of the container.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showAccountDetails_v1__account__storage_account_services.html">Show account details and list containers (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the number of objects in the container.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        /// <remarks>
        /// This field is <see href="http://en.wikipedia.org/wiki/Eventual_consistency">eventually consistent</see>.
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showAccountDetails_v1__account__storage_account_services.html">Show account details and list containers (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("count")]
        public int Count { get; private set; }

        /// <summary>
        /// Gets the total space utilized by the objects in this container.
        /// <note type="warning">The value of this property is not defined by OpenStack, and may not be consistent across vendors.</note>
        /// </summary>
        /// <remarks>
        /// This field is <see href="http://en.wikipedia.org/wiki/Eventual_consistency">eventually consistent</see>.
        /// </remarks>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showAccountDetails_v1__account__storage_account_services.html">Show account details and list containers (OpenStack Object Storage API v1 Reference)</seealso>
        /// <seealso href="http://docs.rackspace.com/files/api/v1/cf-devguide/content/GET_listcontainers_v1__account__accountServicesOperations_d1e000.html">Show Account Details and List Containers (Rackspace Cloud Files Developer Guide - API v1)</seealso>
        [JsonProperty("bytes")]
        public long Bytes { get; private set; }
    }
}
