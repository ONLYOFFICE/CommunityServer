// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    /// <summary>
    /// The Chaos Hander Option middleware class
    /// </summary>
    public class ChaosHandlerOption : IMiddlewareOption
    {
        /// <summary>
        /// Percentage of responses that will have KnownChaos responses injected, assuming no PlannedChaosFactory is provided
        /// </summary>
        public int ChaosPercentLevel { get; set; } = 10;
        /// <summary>
        /// List of failure responses that potentially could be returned when 
        /// </summary>
        public List<HttpResponseMessage> KnownChaos { get; set; } = null;
        /// <summary>
        /// Function to return chaos response based on current request.  This is used to reproduce detected failure modes.
        /// </summary>
        public Func<HttpRequestMessage, HttpResponseMessage> PlannedChaosFactory { get; set; } = null;
    }
}
