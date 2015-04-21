//-----------------------------------------------------------------------
// <copyright file="TwitterIdCollection.cs" company="Patrick 'Ricky' Smith">
//  This file is part of the Twitterizer library (http://www.twitterizer.net)
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
// <summary>The twitter id collection class.</summary>
//-----------------------------------------------------------------------


namespace Twitterizer
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Core;

    /// <summary>
    /// Holds a collection of ID values
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterIdCollection : Collection<decimal>, ITwitterObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        /// <remarks></remarks>
        public TwitterIdCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterIdCollection"/> class.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <remarks></remarks>
        public TwitterIdCollection(List<decimal> items)
        {
            items.ForEach(Add);
        }

        /// <summary>
        /// Annotations are additional pieces of data, supplied by Twitter clients, in a non-structured dictionary.
        /// </summary>
        /// <value>The annotations.</value>
        public Dictionary<string, string> Annotations { get; set; }

        /// <summary>
        /// Performs an explicit conversion from <see cref="List{T}"/> to <see cref="Twitterizer.TwitterIdCollection"/>.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>The result of the conversion.</returns>
        /// <remarks></remarks>
        public static explicit operator TwitterIdCollection (List<decimal> collection)
        {
            TwitterIdCollection newCollection = new TwitterIdCollection();
            foreach (var item in collection)
            {
                newCollection.Add(item);
            }

            return newCollection;
        }
    }

    /// <summary>
    /// Holds extension methods related to the <see cref="Twitterizer.TwitterIdCollection"/> class.
    /// </summary>
    /// <remarks></remarks>
    public static class TwitterIdCollectionExtensions
    {
        /// <summary>
        /// Converts the collection to a <see cref="Twitterizer.TwitterIdCollection"/> class.
        /// </summary>
        /// <param name="old">The old.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TwitterIdCollection ToIdCollection(this IEnumerable<decimal> old)
        {
            TwitterIdCollection newCollection = new TwitterIdCollection();
            foreach (var item in old)
            {
                newCollection.Add(item);
            }
            return newCollection;
        }
    }
}
