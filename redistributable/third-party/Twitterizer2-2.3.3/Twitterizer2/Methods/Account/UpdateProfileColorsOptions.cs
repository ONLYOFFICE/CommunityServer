//-----------------------------------------------------------------------
// <copyright file="UpdateProfileColorsOptions.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The update profile colors options class.</summary>
//-----------------------------------------------------------------------

namespace Twitterizer
{
#if !SILVERLIGHT
    using System.Drawing;
#endif

    /// <summary>
    /// Optional properties for the <see cref="Twitterizer.TwitterUser"/>.Profile*Colors methods.
    /// </summary>
    public class UpdateProfileColorsOptions : OptionalProperties
    {
#if !SILVERLIGHT
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateProfileColorsOptions"/> class.
        /// </summary>
        public UpdateProfileColorsOptions()
        {
            BackgroundColor = Color.Empty;
            TextColor = Color.Empty;
            LinkColor = Color.Empty;
            SidebarFillColor = Color.Empty;
            SidebarBorderColor = Color.Empty;
        }

        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        public Color TextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the link.
        /// </summary>
        /// <value>The color of the link.</value>
        public Color LinkColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the sidebar fill.
        /// </summary>
        /// <value>The color of the sidebar fill.</value>
        public Color SidebarFillColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the sidebar border.
        /// </summary>
        /// <value>The color of the sidebar border.</value>
        public Color SidebarBorderColor { get; set; }
#else
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        /// <value>The color of the text.</value>
        public string TextColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the link.
        /// </summary>
        /// <value>The color of the link.</value>
        public string LinkColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the sidebar fill.
        /// </summary>
        /// <value>The color of the sidebar fill.</value>
        public string SidebarFillColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the sidebar border.
        /// </summary>
        /// <value>The color of the sidebar border.</value>
        public string SidebarBorderColor { get; set; }
#endif
    }
}
