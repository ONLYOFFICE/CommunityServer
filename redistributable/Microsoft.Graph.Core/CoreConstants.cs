// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System;

namespace Microsoft.Graph
{
    /// <summary>
    /// Constants for the Graph Core library.
    /// </summary>
    public static class CoreConstants
    {
        /// <summary>
        /// Polling interval for task completion.
        /// </summary>
        public const int PollingIntervalInMs = 5000;

        /// <summary>
        /// Header constants.
        /// </summary>
        public static class Headers
        {
            /// Authorization bearer.
            public const string Bearer = "Bearer";
            
            /// SDK Version header
            public const string SdkVersionHeaderName = "SdkVersion";

            /// SDK Version header
            public const string SdkVersionHeaderValueFormatString = "{0}-dotnet-{1}.{2}.{3}";

            /// Content-Type header
            public const string FormUrlEncodedContentType = "application/x-www-form-urlencoded";

            /// Throw-site header
            public const string ThrowSiteHeaderName = "X-ThrowSite";

            /// Client Request Id
            public const string ClientRequestId = "client-request-id";

            /// Feature Flag
            public const string FeatureFlag = "FeatureFlag";

        }

        /// <summary>
        /// MimeType constants.
        /// </summary>
        public static class MimeTypeNames
        {
            /// <summary>
            /// MimeTypeNames.Application constants.
            /// </summary>
            public static class Application
            {
                /// JSON content type value
                public const string Json = "application/json";
            }
        }

        /// <summary>
        /// Serialization constants.
        /// </summary>
        public static class Serialization
        {
            /// OData type
            public const string ODataType = "@odata.type";

            /// OData next link
            internal const string ODataNextLink = "@nextLink";
        }

        /// <summary>
        /// Batch request constants.
        /// </summary>
        public static class BatchRequest
        {
            /// <summary>
            /// Maximum number of individual requests.
            /// </summary>
            public const int MaxNumberOfRequests = 20;

            internal const string Id = "id";

            internal const string Url = "url";

            internal const string Body = "body";

            internal const string DependsOn = "dependsOn";

            internal const string Method = "method";

            internal const string Requests = "requests";

            internal const string Responses = "responses";

            internal const string Status = "status";

            internal const string Headers = "headers";

        }

        /// <summary>
        /// Encoding constants
        /// </summary>
        public static class Encoding
        {
            /// gzip encoding.
            public const string GZip = "gzip";
        }
    }
}
