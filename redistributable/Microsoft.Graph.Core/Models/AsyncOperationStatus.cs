// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    /// <summary>
    /// The type AsyncOperationStatus.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [JsonConverter(typeof(DerivedTypeConverter))]
    public partial class AsyncOperationStatus
    {

        /// <summary>
        /// Gets or sets operation.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "operation", Required = Required.Default)]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets percentageComplete.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "percentageComplete", Required = Required.Default)]
        public double? PercentageComplete { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "status", Required = Required.Default)]
        public string Status { get; set; }
    
        /// <summary>
        /// Gets or sets additional data.
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }
    
    }
}
