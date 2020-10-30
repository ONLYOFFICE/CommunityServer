// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// The graph request context class
    /// </summary>
    public class GraphRequestContext
    {
        /// <summary>
        /// A ClientRequestId property
        /// </summary>
        public string ClientRequestId { get; set; }

        /// <summary>
        /// A MiddlewareOptions property
        /// </summary>
        public IDictionary<string, IMiddlewareOption> MiddlewareOptions { get; set; }

        /// <summary>
        /// A CancellationToken property
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// A FeatureUsage property
        /// </summary>
        public FeatureFlag FeatureUsage { get; set; }

        /// <summary>
        /// A <see cref="GraphUserAccount"/> property representing the logged in user
        /// </summary>
        public GraphUserAccount User { get; set; }
    }
}
