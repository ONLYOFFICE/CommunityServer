// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    /// <summary>
    /// The auth middleware option class
    /// </summary>
    public class AuthenticationHandlerOption : IMiddlewareOption
    {
        /// <summary>
        /// An authentication provider
        /// </summary>
        internal IAuthenticationProvider AuthenticationProvider { get; set; }

        /// <summary>
        /// An authentication provider option.
        /// </summary>
        public IAuthenticationProviderOption AuthenticationProviderOption { get; set; }
    }
}
