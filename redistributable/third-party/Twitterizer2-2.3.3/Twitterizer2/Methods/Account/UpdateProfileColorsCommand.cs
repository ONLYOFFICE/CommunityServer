//-----------------------------------------------------------------------
// <copyright file="UpdateProfileColorsCommand.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The update profile colors command class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Commands
{
    using System;
#if !SILVERLIGHT
    using System.Drawing;
#endif
    using Twitterizer.Core;

    /// <summary>
    /// Sets one or more hex values that control the color scheme of the authenticating user's profile page on twitter.com
    /// </summary>
    [AuthorizedCommand]
#if !SILVERLIGHT
    [Serializable]
#endif
    internal class UpdateProfileColorsCommand : TwitterCommand<TwitterUser>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileColorsCommand"/> class.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        public UpdateProfileColorsCommand(OAuthTokens tokens, UpdateProfileColorsOptions options) :
            base(HTTPVerb.POST, "account/update_profile_colors.json", tokens, options)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }
        }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        public override void Init()
        {
            UpdateProfileColorsOptions options = (UpdateProfileColorsOptions)this.OptionalProperties;

#if !SILVERLIGHT
            if (options.BackgroundColor != Color.Empty)
            {
                this.RequestParameters.Add("profile_background_color", ColorTranslator.ToHtml(options.BackgroundColor));
            }
#else
            if (options.BackgroundColor != null)
            {
                this.RequestParameters.Add("profile_background_color", options.BackgroundColor);
            }
#endif

#if !SILVERLIGHT
            if (options.TextColor != Color.Empty)
            {
                this.RequestParameters.Add("profile_text_color", ColorTranslator.ToHtml(options.TextColor));
            }
#else
            if (options.TextColor != null)
            {
                this.RequestParameters.Add("profile_text_color", options.TextColor);
            }
#endif

#if !SILVERLIGHT
            if (options.LinkColor != Color.Empty)
            {
                this.RequestParameters.Add("profile_link_color", ColorTranslator.ToHtml(options.LinkColor));
            }
#else
            if (options.LinkColor != null)
            {
                this.RequestParameters.Add("profile_link_color", options.LinkColor);
            }
#endif

#if !SILVERLIGHT
            if (options.SidebarFillColor != Color.Empty)
            {
                this.RequestParameters.Add("profile_sidebar_fill_color", ColorTranslator.ToHtml(options.SidebarFillColor));
            }
#else
            if (options.SidebarFillColor != null)
            {
                this.RequestParameters.Add("profile_sidebar_fill_color", options.SidebarFillColor);
            }
#endif

#if !SILVERLIGHT
            if (options.SidebarBorderColor != Color.Empty)
            {
                this.RequestParameters.Add("profile_sidebar_border_color", ColorTranslator.ToHtml(options.SidebarBorderColor));
            }
#else
            if (options.SidebarBorderColor != null)
            {
                this.RequestParameters.Add("profile_sidebar_border_color", options.SidebarBorderColor);
            }
#endif
        }
    }
}
