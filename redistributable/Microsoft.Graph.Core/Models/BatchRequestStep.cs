// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    /// <summary>
    /// A single batch request step.
    /// </summary>
    public class BatchRequestStep
    {
        /// <summary>
        /// A unique batch request id property.
        /// </summary>
        public string RequestId { get; private set; }

        /// <summary>
        /// A http request message for an individual batch request operation.
        /// </summary>
        public HttpRequestMessage Request { get; private set; }

        /// <summary>
        /// An OPTIONAL array of batch request ids specifying the order of execution for individual batch requests.
        /// </summary>
        public List<string> DependsOn { get; set; }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestStep"/>.
        /// </summary>
        /// <param name="requestId">A unique batch request id.</param>
        /// <param name="httpRequestMessage">A http request message for an individual batch request operation.</param>
        /// <param name="dependsOn">An OPTIONAL array of batch request ids specifying the order of execution for individual batch requests.</param>
        public BatchRequestStep(string requestId, HttpRequestMessage httpRequestMessage, List<string> dependsOn = null)
        {
            RequestId = (!string.IsNullOrEmpty(requestId)) ? requestId : throw new ClientException(
                new Error
                {
                    Code = ErrorConstants.Codes.InvalidArgument,
                    Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(requestId))
                });

            Request = httpRequestMessage ?? throw new ClientException(new Error
            {
                Code = ErrorConstants.Codes.InvalidArgument,
                Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(httpRequestMessage))
            });

            DependsOn = dependsOn;
        }
    }
}
