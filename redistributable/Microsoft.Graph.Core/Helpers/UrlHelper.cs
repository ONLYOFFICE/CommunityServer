// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    /// <summary>
    /// Helper class for working with URLs.
    /// </summary>
    public static class UrlHelper
    {
        /// <summary>
        /// Parse query options from the URL.
        /// </summary>
        /// <param name="resultUri"></param>
        /// <returns></returns>
        public static IDictionary<string, string> GetQueryOptions(Uri resultUri)
        {
            string[] queryParams = null;
            var queryValues = new Dictionary<string, string>();

            int fragmentIndex = resultUri.AbsoluteUri.IndexOf("#", StringComparison.Ordinal);
            if (fragmentIndex > 0 && fragmentIndex < resultUri.AbsoluteUri.Length + 1)
            {
                queryParams = resultUri.AbsoluteUri.Substring(fragmentIndex + 1).Split('&');
            }
            else if (fragmentIndex < 0)
            {
                if (!string.IsNullOrEmpty(resultUri.Query))
                {
                    queryParams = resultUri.Query.TrimStart('?').Split('&');
                }
            }

            if (queryParams != null)
            {
                foreach (var param in queryParams)
                {
                    if (!string.IsNullOrEmpty(param))
                    {
                        string[] kvp = param.Split('=');
                        queryValues.Add(kvp[0], WebUtility.UrlDecode(kvp[1]));
                    }
                }
            }

            return queryValues;
        }
    }
}
