//-----------------------------------------------------------------------
// <copyright file="RequestStatus.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net/)
// 
//  Copyright (c) 2010, Patrick "Ricky" Smith (ricky@digitally-born.com)
//  All rights reserved.
//  
//  Redistribution and use in source and binary forms, with or without modification, are 
//  permitted provided that the following conditions are met:
// 
//  - Redistributions of source code must retain the above copyright notice, this list 
//    of conditions and the following disclaimer.
//  - Redistributions in binary form must reproduce the above copyright notice, this list 
//    of conditions and the following disclaimer in the documentation and/or other 
//    materials provided with the distribution.
//  - Neither the name of the Twitterizer nor the names of its contributors may be 
//    used to endorse or promote products derived from this software without specific 
//    prior written permission.
// 
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND 
//  ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED 
//  WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//  IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, 
//  INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT 
//  NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR 
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
//  WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
//  ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
//  POSSIBILITY OF SUCH DAMAGE.
// </copyright>
// <author>Ricky Smith</author>
// <summary>The twitter status class. Provides information about the last request made.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    /// <summary>
    /// Describes the result status of a request
    /// </summary>
    public enum RequestResult
    {
        /// <summary>
        /// The request was completed successfully
        /// </summary>
        Success,

        /// <summary>
        /// The URI requested is invalid or the resource requested, such as a user, does not exists.
        /// </summary>
        FileNotFound,

        /// <summary>
        /// The request was invalid.  An accompanying error message will explain why.
        /// </summary>
        BadRequest,

        /// <summary>
        /// Authentication credentials were missing or incorrect.
        /// </summary>
        Unauthorized,

        /// <summary>
        /// Returned by the Search API when an invalid format is specified in the request.
        /// </summary>
        NotAcceptable,

        /// <summary>
        /// The authorized user, or client IP address, is being rate limited.
        /// </summary>
        RateLimited,

        /// <summary>
        /// Twitter is currently down.
        /// </summary>
        TwitterIsDown,

        /// <summary>
        /// Twitter is online, but is overloaded. Try again later.
        /// </summary>
        TwitterIsOverloaded,

        /// <summary>
        /// The request failed due to a connection issue or timeout.
        /// </summary>
        ConnectionFailure,

        /// <summary>
        /// Something unexpected happened. See the error message for additional information.
        /// </summary>
        Unknown,

        /// <summary>
        /// Failed to authenticate with the proxy.
        /// </summary>
        ProxyAuthenticationRequired
    }
}
