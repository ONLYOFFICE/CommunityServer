//-----------------------------------------------------------------------
// <copyright file="UpdateProfileBackgroundImageCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The update profile background image command class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
    using System;

    /// <summary>
    /// An options class for updating the user's profile background image.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public sealed class UpdateProfileBackgroundImageOptions : OptionalProperties
    {
        /// <summary>
        /// Gets or sets a value indicating whether or not to tile the background image.
        /// </summary>
        /// <value><c>true</c> if tiled; otherwise, <c>false</c>.</value>
        public bool? Tiled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display the profile background image or not.
        /// </summary>
        /// <value><c>true</c> to use an image; otherwise, <c>false</c>.</value>
        public bool UseImage { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileBackgroundImageOptions"/> class.
        /// </summary>
        public UpdateProfileBackgroundImageOptions()
        {
            this.UseImage = true;
        }
    }
}
