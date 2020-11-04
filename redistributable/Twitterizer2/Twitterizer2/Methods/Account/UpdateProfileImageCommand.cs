//-----------------------------------------------------------------------
// <copyright file="UpdateProfileImageCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The update profile image command class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
    using Twitterizer.Core;

#if !SILVERLIGHT
    [Serializable]
#endif
    internal class UpdateProfileImageCommand : TwitterCommand<TwitterUser>
    {
        private readonly byte[] imageData;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileImageCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="image">The image.</param>
        /// <param name="options">The options.</param>
        public UpdateProfileImageCommand(OAuthTokens tokens, byte[] image, OptionalProperties options)
            : base(HTTPVerb.POST, "account/update_profile_image.json", tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (image == null || image.Length == 0)
            {
                throw new ArithmeticException("Image data cannot be null or zero length.");
            }

            if (image.Length > 716800)
            {
                throw new ArithmeticException("Image cannot exceed 700Kb in size.");
            }

            this.imageData = image;
            this.Multipart = true;
        }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            this.RequestParameters.Add("image", this.imageData);
            this.RequestParameters.Add("include_entities", "true");
        }
    }
}
