using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpenStack.ContentDeliveryNetworks.v1
{
    /// <summary>
    /// Content Delivery Network (CDN) service status
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ServiceStatus
    {
        /// <summary>
        /// The service is currently being created and deployed.
        /// </summary>
        [EnumMember(Value = "create_in_progress")]
        CreateInProgress,

        /// <summary>
        /// The service has been deployed and is ready to use.
        /// </summary>
        [EnumMember(Value = "deployed")]
        Deployed,

        /// <summary>
        /// The service is currently being updated.
        /// </summary>
        [EnumMember(Value = "update_in_progress")]
        UpdateInProgress,

        /// <summary>
        /// The service is currently being deleted.
        /// </summary>
        [EnumMember(Value = "delete_in_progress")]
        DeleteInProgress,

        /// <summary>
        /// The previous operation on the service failed. Looks for the details on <see cref="Service.Errors"/>
        /// </summary>
        [EnumMember(Value = "failed")]
        Failed
    }
}