// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    /// <summary>
    /// The error response object from the service on an unsuccessful call.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ErrorResponse
    {
        /// <summary>
        /// The <see cref="Error"/> returned by the service.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public Error Error { get; set; }

        /// <summary>
        /// Additional data returned in the call.
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }
    }
}
