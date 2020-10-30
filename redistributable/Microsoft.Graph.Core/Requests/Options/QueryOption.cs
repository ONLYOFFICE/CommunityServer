// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// A query option to be added to the request.
    /// https://developer.microsoft.com/en-us/graph/docs/concepts/query_parameters
    /// </summary>
    public class QueryOption : Option
    {
        /// <summary>
        /// Create a query option.
        /// </summary>
        /// <param name="name">The name of the query option, or parameter.</param>
        /// <param name="value">The value of the query option.</param>
        public QueryOption(string name, string value)
            : base(name, value)
        {
        }
    }
}
