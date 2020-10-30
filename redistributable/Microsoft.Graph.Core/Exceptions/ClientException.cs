// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph
{
    using System;

    /// <summary>
    /// Graph client exception.
    /// </summary>
    public class ClientException : ServiceException
    {
        /// <summary>
        /// Creates a new client exception.
        /// </summary>
        /// <param name="error">The error that triggered the exception.</param>
        /// <param name="innerException">The possible innerException.</param>
        public ClientException(Error error, Exception innerException = null) : base(error, innerException)
        {
        }
    }
}
