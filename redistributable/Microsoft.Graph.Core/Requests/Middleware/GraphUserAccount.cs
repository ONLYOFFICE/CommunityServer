// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// Class representing the logged in MS graph user
    /// </summary>
    public class GraphUserAccount
    {
        /// <summary>
        /// The users email address
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The identity provider url
        /// </summary>
        public string Environment { get; set; }

        /// <summary>
        /// Users tenant id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Users id in a tenant
        /// </summary>
        public string ObjectId { get; set; }
    }
}
