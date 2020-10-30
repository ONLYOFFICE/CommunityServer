// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// The error object contained in 400 and 500 responses returned from the service.
    /// Models OData protocol, 9.4 Error Response Body
    /// http://docs.oasis-open.org/odata/odata/v4.01/csprd05/part1-protocol/odata-v4.01-csprd05-part1-protocol.html#_Toc14172757
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class Error
    {
        /// <summary>
        /// This code represents the HTTP status code when this Error object accessed from the ServiceException.Error object.
        /// This code represent a sub-code when the Error object is in the InnerError or ErrorDetails object.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "code", Required = Required.Default)]
        public string Code { get; set; }
        
        /// <summary>
        /// The error message.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "message", Required = Required.Default)]
        public string Message { get; set; }

        /// <summary>
        /// Indicates the target of the error, for example, the name of the property in error.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "target", Required = Required.Default)]
        public string Target { get; set; }

        /// <summary>
        /// An array of details that describe the error[s] encountered with the request.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "details", Required = Required.Default)]
        public IEnumerable<ErrorDetail> Details { get; set; }

        /// <summary>
        /// The inner error of the response. These are additional error objects that may be more specific than the top level error.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "innererror", Required = Required.Default)]
        public Error InnerError { get; set; }

        /// <summary>
        /// The Throw site of the error.
        /// </summary>
        public string ThrowSite { get; internal set; }

        /// <summary>
        /// Gets or set the client-request-id header returned in the response headers collection. 
        /// </summary>
        public string ClientRequestId { get; internal set; }

        /// <summary>
        /// The AdditionalData property bag.
        /// </summary>
        [JsonExtensionData(ReadData = true)]
        public IDictionary<string, object> AdditionalData { get; set; }

        /// <summary>
        /// Concatenates the error into a string.
        /// </summary>
        /// <returns>A human-readable string error response.</returns>
        public override string ToString()
        {
            var errorStringBuilder = new StringBuilder();

            if (!string.IsNullOrEmpty(this.Code))
            {
                errorStringBuilder.AppendFormat("Code: {0}", this.Code);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Message))
            {
                errorStringBuilder.AppendFormat("Message: {0}", this.Message);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.Target))
            {
                errorStringBuilder.AppendFormat("Target: {0}", this.Target);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (this.Details != null && this.Details.GetEnumerator().MoveNext())
            {
                errorStringBuilder.Append("Details:");
                errorStringBuilder.Append(Environment.NewLine);

                int i = 0;
                foreach (var detail in this.Details)
                {
                    errorStringBuilder.AppendFormat("\tDetail{0}:{1}", i, detail.ToString());
                    errorStringBuilder.Append(Environment.NewLine);
                    i++;
                }
            }

            if (this.InnerError != null)
            {
                errorStringBuilder.Append("Inner error:");
                errorStringBuilder.Append(Environment.NewLine);
                errorStringBuilder.Append("\t" + this.InnerError.ToString());
            }

            if (!string.IsNullOrEmpty(this.ThrowSite))
            {
                errorStringBuilder.AppendFormat("Throw site: {0}", this.ThrowSite);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (!string.IsNullOrEmpty(this.ClientRequestId))
            {
                errorStringBuilder.AppendFormat("ClientRequestId: {0}", this.ClientRequestId);
                errorStringBuilder.Append(Environment.NewLine);
            }

            if (this.AdditionalData != null && this.AdditionalData.GetEnumerator().MoveNext())
            {
                errorStringBuilder.Append("AdditionalData:");
                errorStringBuilder.Append(Environment.NewLine);
                foreach (var prop in this.AdditionalData)
                {
                    errorStringBuilder.AppendFormat("\t{0}: {1}", prop.Key, prop.Value.ToString());
                    errorStringBuilder.Append(Environment.NewLine);
                }
            }

            return errorStringBuilder.ToString();
        }
    }
}
