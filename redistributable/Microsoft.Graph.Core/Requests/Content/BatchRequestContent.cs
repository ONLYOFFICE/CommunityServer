// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// A <see cref="HttpContent"/> implementation to handle json batch requests.
    /// </summary>
    public class BatchRequestContent: HttpContent
    {
        /// <summary>
        /// A BatchRequestSteps property.
        /// </summary>
        public IReadOnlyDictionary<string, BatchRequestStep> BatchRequestSteps { get; private set; }

        /// <summary>
        /// Gets a serializer for serializing and deserializing JSON objects.
        /// </summary>
        public ISerializer Serializer { get; private set; }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        public BatchRequestContent()
            :this(new BatchRequestStep[] { },null)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        /// <param name="batchRequestSteps">A list of <see cref="BatchRequestStep"/> to add to the batch request content.</param>
        /// <param name="serializer">A serializer for serializing and deserializing JSON objects.</param>
        public BatchRequestContent(BatchRequestStep [] batchRequestSteps, ISerializer serializer = null)
            : this(batchRequestSteps)
        {
            this.Serializer = serializer ?? new Serializer();
        }

        /// <summary>
        /// Constructs a new <see cref="BatchRequestContent"/>.
        /// </summary>
        /// <param name="batchRequestSteps">A list of <see cref="BatchRequestStep"/> to add to the batch request content.</param>
        public BatchRequestContent(params BatchRequestStep[] batchRequestSteps)
        {
            if (batchRequestSteps == null)
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.InvalidArgument,
                    Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(batchRequestSteps))
                });

            if (batchRequestSteps.Count() > CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ClientException(new Error {
                    Code = ErrorConstants.Codes.MaximumValueExceeded,
                    Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests)
                });

            this.Headers.ContentType = new MediaTypeHeaderValue(CoreConstants.MimeTypeNames.Application.Json);

            BatchRequestSteps = new Dictionary<string, BatchRequestStep>();

            foreach (BatchRequestStep requestStep in batchRequestSteps)
            {
                if(requestStep.DependsOn != null && !ContainsCorrespondingRequestId(requestStep.DependsOn))
                {
                    throw new ClientException(new Error
                    {
                        Code = ErrorConstants.Codes.InvalidArgument,
                        Message = ErrorConstants.Messages.InvalidDependsOnRequestId
                    });
                }
                AddBatchRequestStep(requestStep);
            }

            this.Serializer = new Serializer();
        }

        /// <summary>
        /// Adds a <see cref="BatchRequestStep"/> to batch request content if doesn't exists.
        /// </summary>
        /// <param name="batchRequestStep">A <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>True or false based on addition or not addition of the provided <see cref="BatchRequestStep"/>. </returns>
        public bool AddBatchRequestStep(BatchRequestStep batchRequestStep)
        {
            if (batchRequestStep == null
                || BatchRequestSteps.ContainsKey(batchRequestStep.RequestId)
                || BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests //we should not add any more steps
                )
            {
                return false;
            }

            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return true;
        }

        /// <summary>
        /// Adds a <see cref="HttpRequestMessage"/> to batch request content.
        /// </summary>
        /// <param name="httpRequestMessage">A <see cref="HttpRequestMessage"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the newly created <see cref="BatchRequestStep"/></returns>
        public string AddBatchRequestStep(HttpRequestMessage httpRequestMessage)
        {
            if (BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.MaximumValueExceeded,
                    Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests)
                });

            string requestId = Guid.NewGuid().ToString();
            BatchRequestStep batchRequestStep = new BatchRequestStep(requestId, httpRequestMessage);
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return requestId;
        }

        /// <summary>
        /// Adds a <see cref="IBaseRequest"/> to batch request content
        /// </summary>
        /// <param name="request">A <see cref="BaseRequest"/> to use to build a <see cref="BatchRequestStep"/> to add.</param>
        /// <returns>The requestId of the  newly created <see cref="BatchRequestStep"/></returns>
        public string AddBatchRequestStep(IBaseRequest request)
        {
            if (BatchRequestSteps.Count >= CoreConstants.BatchRequest.MaxNumberOfRequests)
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.MaximumValueExceeded,
                    Message = string.Format(ErrorConstants.Messages.MaximumValueExceeded, "Number of batch request steps", CoreConstants.BatchRequest.MaxNumberOfRequests)
                });

            string requestId = Guid.NewGuid().ToString();
            BatchRequestStep batchRequestStep = new BatchRequestStep(requestId, request.GetHttpRequestMessage());
            (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Add(batchRequestStep.RequestId, batchRequestStep);
            return requestId;
        }

        /// <summary>
        /// Removes a <see cref="BatchRequestStep"/> from batch request content for the specified id.
        /// </summary>
        /// <param name="requestId">A unique batch request id to remove.</param>
        /// <returns>True or false based on removal or not removal of a <see cref="BatchRequestStep"/>.</returns>
        public bool RemoveBatchRequestStepWithId(string requestId)
        {
            if (string.IsNullOrEmpty(requestId))
                throw new ClientException(
                    new Error
                        {
                            Code = ErrorConstants.Codes.InvalidArgument,
                            Message = string.Format(ErrorConstants.Messages.NullParameter, nameof(requestId))
                        });

            bool isRemoved = false;
            if (BatchRequestSteps.ContainsKey(requestId)) {
                (BatchRequestSteps as IDictionary<string, BatchRequestStep>).Remove(requestId);
                isRemoved = true;
                foreach (KeyValuePair<string, BatchRequestStep> batchRequestStep in BatchRequestSteps)
                {
                    if (batchRequestStep.Value != null && batchRequestStep.Value.DependsOn != null)
                        while (batchRequestStep.Value.DependsOn.Remove(requestId)) ;
                }
            }
            return isRemoved;
        }

        internal async Task<JObject> GetBatchRequestContentAsync()
        {
            JObject batchRequest = new JObject();
            JArray batchRequestItems = new JArray();

            foreach (KeyValuePair<string, BatchRequestStep> batchRequestStep in BatchRequestSteps)
                batchRequestItems.Add(await GetBatchRequestContentFromStepAsync(batchRequestStep.Value));

            batchRequest.Add(CoreConstants.BatchRequest.Requests, batchRequestItems);

            return batchRequest;
        }

        private bool ContainsCorrespondingRequestId(IList<string> dependsOn)
        {
        	return dependsOn.All(requestId => BatchRequestSteps.ContainsKey(requestId));
        }

        private async Task<JObject> GetBatchRequestContentFromStepAsync(BatchRequestStep batchRequestStep)
        {
            JObject jRequestContent = new JObject
            {
                { CoreConstants.BatchRequest.Id, batchRequestStep.RequestId },
                { CoreConstants.BatchRequest.Url, GetRelativeUrl(batchRequestStep.Request.RequestUri) },
                { CoreConstants.BatchRequest.Method, batchRequestStep.Request.Method.Method }
            };
            if (batchRequestStep.DependsOn != null && batchRequestStep.DependsOn.Count() > 0)
                jRequestContent.Add(CoreConstants.BatchRequest.DependsOn, new JArray(batchRequestStep.DependsOn));

            if (batchRequestStep.Request.Content?.Headers != null && batchRequestStep.Request.Content.Headers.Count() > 0)
                jRequestContent.Add(CoreConstants.BatchRequest.Headers, GetContentHeader(batchRequestStep.Request.Content.Headers));

            if(batchRequestStep.Request != null && batchRequestStep.Request.Content != null)
            {
                jRequestContent.Add(CoreConstants.BatchRequest.Body, await GetRequestContentAsync(batchRequestStep.Request));
            }

            return jRequestContent;
        }

        private async Task<JObject> GetRequestContentAsync(HttpRequestMessage request)
        {
            try
            {
                HttpRequestMessage clonedRequest = await request.CloneAsync();

                using (Stream streamContent = await clonedRequest.Content.ReadAsStreamAsync())
                {
                    return Serializer.DeserializeObject<JObject>(streamContent);
                }
            }
            catch (Exception ex)
            {
                throw new ClientException(new Error
                {
                    Code = ErrorConstants.Codes.InvalidRequest,
                    Message = ErrorConstants.Messages.UnableToDeserializexContent
                }, ex);
            }
        }

        private JObject GetContentHeader(HttpContentHeaders headers)
        {
            JObject jHeaders = new JObject();
            foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
            {
                jHeaders.Add(header.Key, GetHeaderValuesAsString(header.Value));
            }
            return jHeaders;
        }

        private string GetHeaderValuesAsString(IEnumerable<string> headerValues)
        {
            if (headerValues == null || headerValues.Count() == 0)
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            foreach (string headerValue in headerValues)
            {
                builder.Append(headerValue);
            }

            return builder.ToString();
        }

        private string GetRelativeUrl(Uri requestUri)
        {
            string version = "v1.0";
            if (requestUri.AbsoluteUri.Contains("beta"))
                version = "beta";

            return requestUri.AbsoluteUri.Substring(requestUri.AbsoluteUri.IndexOf(version) + version.ToCharArray().Count());
        }

        /// <summary>
        /// Serialize the HTTP content to a stream as an asynchronous operation.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        /// <returns></returns>
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (StreamWriter streamWritter = new StreamWriter(stream, new UTF8Encoding(), 1024, true))
            using (JsonTextWriter textWritter = new JsonTextWriter(streamWritter))
            {
                JObject batchContent = await GetBatchRequestContentAsync();
                batchContent.WriteTo(textWritter);
            }
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HTTP content.</param>
        /// <returns></returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
