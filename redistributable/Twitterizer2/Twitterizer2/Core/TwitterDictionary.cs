//-----------------------------------------------------------------------
// <copyright file="TwitterDictionary.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The base class for object dictionaries.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Core
{
    using System;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;
    using System.Collections.Generic;

    /// <summary>
    /// The base class for object dictionaries.
    /// </summary>
    /// <typeparam name="T">The type of key object stored in the collection.</typeparam>
    /// <typeparam name="T2">The type of value object stored in the collection.</typeparam>
#if !SILVERLIGHT
    [Serializable]
#endif
    [DataContract]
    public abstract class TwitterDictionary<T, T2> : Dictionary<T, T2>
        //where T : class, ITwitterObject
        where T2: class, ITwitterObject        
    {
        /// <summary>
        /// Gets or sets the annotations.
        /// </summary>
        /// <value>The annotations.</value>
        [DataMember]
        public Dictionary<string, string> Annotations { get; set; }
    }
}
