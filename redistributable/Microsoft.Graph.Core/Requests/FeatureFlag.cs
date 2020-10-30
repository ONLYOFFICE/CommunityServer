// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    /// <summary>
    /// Feature Flags
    /// </summary>
    [Flags]
    public enum FeatureFlag
    {
        /// None set
        None = 0x00000000,
        /// Redirect Handler
        RedirectHandler = 0x00000001,
        /// Retry Handler
        RetryHandler = 0x00000002,
        /// Auth Handler
        AuthHandler = 0x00000004,
        /// Default Handler
        DefaultHttpProvider = 0x00000008,
        /// Logging Handler
        LoggingHandler = 0x00000010,
        /// Service Discovery Handler
        ServiceDiscoveryHandler = 0x00000020,
        /// Compression Handler
        CompressionHandler = 0x00000040,
        /// Connection Pool Manager
        ConnectionPoolManager = 0x00000080,
        /// Long Running Operation Handler 
        LongRunningOperationHandler = 0x00000100,
        /// Batch Request Content Used
        BatchRequestContext = 0x00000200,
        /// Page Iterator task Used
        PageIteratorTask = 0x00000400,
        /// File Upload task Used
        FileUploadTask = 0x00000800

    }
}
