namespace net.openstack.Core.Domain
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides the details of an object stored in an Object Storage provider.
    /// </summary>
    /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showContainerDetails_v1__account___container__storage_container_services.html">Show container details and list objects (OpenStack Object Storage API v1 Reference)</seealso>
    /// <threadsafety static="true" instance="false"/>
    [JsonObject(MemberSerialization.OptIn)]
    public class ContainerObject : ExtensibleJsonObject
    {
        /// <summary>
        /// Gets a "name" associated with the object.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showContainerDetails_v1__account___container__storage_container_services.html">Show container details and list objects (OpenStack Object Storage API v1 Reference)</seealso>
        [JsonProperty("name")]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the "hash" value associated with the object.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showContainerDetails_v1__account___container__storage_container_services.html">Show container details and list objects (OpenStack Object Storage API v1 Reference)</seealso>
        [JsonProperty("hash")]
        public string Hash { get; private set; }

        /// <summary>
        /// Gets the "bytes" value associated with the object.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showContainerDetails_v1__account___container__storage_container_services.html">Show container details and list objects (OpenStack Object Storage API v1 Reference)</seealso>
        [JsonProperty("bytes")]
        public long Bytes { get; private set; }

        /// <summary>
        /// Gets the "content type" value associated with the object.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showContainerDetails_v1__account___container__storage_container_services.html">Show container details and list objects (OpenStack Object Storage API v1 Reference)</seealso>
        [JsonProperty("content_type")]
        public string ContentType { get; private set; }

        /// <summary>
        /// Gets the "last modified" value associated with the object.
        /// <note type="warning">The value of this property is not defined. Do not use.</note>
        /// </summary>
        /// <seealso href="http://docs.openstack.org/api/openstack-object-storage/1.0/content/GET_showContainerDetails_v1__account___container__storage_container_services.html">Show container details and list objects (OpenStack Object Storage API v1 Reference)</seealso>
        [JsonProperty("last_modified")]
        public DateTimeOffset LastModified { get; private set; }
    }
}
