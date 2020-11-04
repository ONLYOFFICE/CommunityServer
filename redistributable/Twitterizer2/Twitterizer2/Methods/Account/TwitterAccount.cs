//-----------------------------------------------------------------------
// <copyright file="TwitterAccount.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The TwitterAccount class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    using Twitterizer.Core;

    /// <summary>
    /// Provides methods to request and modify details of an authorized user's account details.
    /// </summary>
    public static class TwitterAccount
    {
        /// <summary>
        /// Verifies the user's credentials.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> VerifyCredentials(OAuthTokens tokens, VerifyCredentialsOptions options)
        {
            Commands.VerifyCredentialsCommand command = new Commands.VerifyCredentialsCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Verifies the user's credentials.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> VerifyCredentials(OAuthTokens tokens)
        {
            return VerifyCredentials(tokens, null);
        }

        /// <summary>
        /// Sets one or more hex values that control the color scheme of the authenticating user's profile page on twitter.com
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The user, with updated data, as a <see cref="TwitterUser"/>
        /// </returns>
        public static TwitterResponse<TwitterUser> UpdateProfileColors(OAuthTokens tokens, UpdateProfileColorsOptions options)
        {
            Commands.UpdateProfileColorsCommand command = new Twitterizer.Commands.UpdateProfileColorsCommand(tokens, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Updates the authenticating user's profile image.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="imageData">The image data.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The user, with updated data, as a <see cref="TwitterUser"/>
        /// </returns>
        public static TwitterResponse<TwitterUser> UpdateProfileImage(OAuthTokens tokens, byte[] imageData, OptionalProperties options = null)
        {
            Commands.UpdateProfileImageCommand command = new Twitterizer.Commands.UpdateProfileImageCommand(tokens, imageData, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Updates the authenticating user's profile image.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="imageLocation">The image location.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// The user, with updated data, as a <see cref="TwitterUser"/>
        /// </returns>
        public static TwitterResponse<TwitterUser> UpdateProfileImage(OAuthTokens tokens, string imageLocation, OptionalProperties options = null)
        {
            return UpdateProfileImage(tokens, System.IO.File.ReadAllBytes(imageLocation), options);
        }

        /// <summary>
        /// Updates the authenticating user's profile background image. This method can also be used to enable or disable the profile background image.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="imageData">The image data.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> UpdateProfileBackgroundImage(OAuthTokens tokens, byte[] imageData = null, UpdateProfileBackgroundImageOptions options = null)
        {
            if (imageData == null && options == null)
            {
                throw new System.ArgumentNullException("imageData", "You must provide image data or indicate you wish to not use any image in the options argument.");
            }

            Commands.UpdateProfileBackgroundImageCommand command = new Twitterizer.Commands.UpdateProfileBackgroundImageCommand(tokens, imageData, options);

            return CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Updates the authenticating user's profile background image. This method can also be used to enable or disable the profile background image.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="imageLocation">The image location.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUser> UpdateProfileBackgroundImage(OAuthTokens tokens, string imageLocation, UpdateProfileBackgroundImageOptions options = null)
        {
            return UpdateProfileBackgroundImage(tokens, System.IO.File.ReadAllBytes(imageLocation), options);
        }

        /// <summary>
        /// Sets values that users are able to set under the "Account" tab of their settings page. Only the parameters specified will be updated.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static TwitterResponse<TwitterUser> UpdateProfile(OAuthTokens tokens, UpdateProfileOptions options)
        {
            Commands.UpdateProfileCommand command = new Commands.UpdateProfileCommand(tokens, options);
            return Core.CommandPerformer.PerformAction(command);
        }
    }
}
