//-----------------------------------------------------------------------
// <copyright file="TwitterObject.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The base class for all data objects.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer.Core
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the callback signature for asynchronous methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="result">The result.</param>
    /// <remarks></remarks>
    public delegate void TwitterAsyncCallback<T>(T result)
            where T : ITwitterObject;

    /// <summary>
    /// The base object class
    /// </summary>
#if !SILVERLIGHT
    [System.Serializable]
#endif
    public class TwitterObject : ITwitterObject
    {
        /// <summary>
        /// The format that all twitter dates are in.
        /// </summary>
        protected const string DateFormat = "ddd MMM dd HH:mm:ss zz00 yyyy";

        /// <summary>
        /// The format that all twitter search api dates are in.
        /// </summary>
        protected const string SearchDateFormat = "ddd, dd MMM yyyy HH:mm:ss +zz00";

        /// <summary>
        /// Annotations are additional pieces of data, supplied by Twitter clients, in a non-structured dictionary.
        /// </summary>
        /// <value>The annotations.</value>
        [JsonProperty(PropertyName = "annotations")]
        public Dictionary<string, string> Annotations { get; set; }
    }
}
