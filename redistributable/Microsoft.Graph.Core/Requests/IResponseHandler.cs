// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;

namespace Microsoft.Graph
{
    /// <summary>
    /// The interface required for all response handlers.
    /// </summary>
    public interface IResponseHandler
    {
        /// <summary>
        /// Process raw HTTP response into the requested domain type.
        /// </summary>
        /// <typeparam name="T">The type to return</typeparam>
        /// <param name="response">The HttpResponseMessage to handle.</param>
        /// <returns></returns>
        Task<T> HandleResponse<T>(HttpResponseMessage response);
    }
}
