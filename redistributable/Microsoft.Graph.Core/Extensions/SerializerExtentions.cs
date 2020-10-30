// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System.Net.Http;
    using System.Text;

    /// <summary>
    /// Extension methods for <see cref="ISerializer"/>
    /// </summary>
    public static class SerializerExtentions
    {
        /// <summary>
        /// Serialize an object into a <see cref="HttpContent"/> object 
        /// </summary>
        /// <param name="serializer"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static HttpContent SerializeAsJsonContent(this ISerializer serializer, object source )
        {
            var stringContent = serializer.SerializeObject(source);
            return new StringContent(stringContent, Encoding.UTF8, "application/json");
        }
        
    }
}
