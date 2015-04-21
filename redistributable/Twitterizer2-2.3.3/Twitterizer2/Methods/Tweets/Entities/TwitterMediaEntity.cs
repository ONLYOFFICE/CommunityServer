//-----------------------------------------------------------------------
// <copyright file="TwitterMediaEntity.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The twitter url entity class</summary>
//-----------------------------------------------------------------------

namespace Twitterizer.Entities
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Represents a pre-parsed media entity located within the body of a <see cref="Twitterizer.TwitterStatus.Text"/>.
    /// </summary>
    /// <remarks></remarks>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TwitterMediaEntity : TwitterUrlEntity
    {
        /// <summary>
        /// The list of currently available and supported media types.
        /// </summary>
        /// <remarks></remarks>
        public enum MediaTypes
        {
            /// <summary>
            /// (default) Indicates the media type returned is unsupported.
            /// </summary>
            Unknown,

            /// <summary>
            /// Indicates the media type returned is a photo.
            /// </summary>
            Photo
        }

        /// <summary>
        /// Gets or sets the type of the media.
        /// </summary>
        /// <value>The type of the media.</value>
        /// <remarks></remarks>
        public MediaTypes MediaType { get; set; }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks></remarks>
        public decimal Id { get; set; }

        /// <summary>
        /// Gets or sets the id string.
        /// </summary>
        /// <value>The id string.</value>
        /// <remarks></remarks>
        public string IdString { get; set; }

        /// <summary>
        /// Gets or sets the media URL.
        /// </summary>
        /// <value>The media URL.</value>
        /// <remarks></remarks>
        public string MediaUrl { get; set; }

        /// <summary>
        /// Gets or sets the media URL secure.
        /// </summary>
        /// <value>The media URL secure.</value>
        /// <remarks></remarks>
        public string MediaUrlSecure { get; set; }

        /// <summary>
        /// Gets or sets the sizes.
        /// </summary>
        /// <value>The sizes.</value>
        /// <remarks></remarks>
        public List<MediaSize> Sizes { get; set; }

        /// <summary>
        /// Represents the display size of a media entity.
        /// </summary>
        /// <remarks></remarks>
        public class MediaSize
        {
            /// <summary>
            /// The enumerated types of reszing that could be applied to the media entity.
            /// </summary>
            /// <remarks></remarks>
            public enum MediaSizeResizes
            {
                /// <summary>
                /// Indicates that the resizing method was unrecognized.
                /// </summary>
                Unknown,
                /// <summary>
                /// Indicates that the media entity was cropped.
                /// </summary>
                Crop,
                /// <summary>
                /// Indicates that the media entity was resized to fit without cropping.
                /// </summary>
                Fit
            }

            /// <summary>
            /// The list of recognized media sizes.
            /// </summary>
            /// <remarks></remarks>
            public enum MediaSizes
            {
                /// <summary>
                /// (default) Indicates that the size provided by the API was unrecognized.
                /// </summary>
                Unknown,
                /// <summary>
                /// Indicates that the media entity is a thumbnail size.
                /// </summary>
                Thumb,
                /// <summary>
                /// Indicates that the media entity is a small size.
                /// </summary>
                Small,
                /// <summary>
                /// Indicates that the media entity is a medium size.
                /// </summary>
                Medium,
                /// <summary>
                /// Indicates that the media entity is a large size.
                /// </summary>
                Large
            }

            /// <summary>
            /// Gets or sets the size.
            /// </summary>
            /// <value>The size.</value>
            /// <remarks></remarks>
            public MediaSizes Size { get; set; }

            /// <summary>
            /// Gets or sets the width.
            /// </summary>
            /// <value>The width.</value>
            /// <remarks></remarks>
            public int Width { get; set; }

            /// <summary>
            /// Gets or sets the height.
            /// </summary>
            /// <value>The height.</value>
            /// <remarks></remarks>
            public int Height { get; set; }

            /// <summary>
            /// Gets or sets the resize.
            /// </summary>
            /// <value>The resize.</value>
            /// <remarks></remarks>
            public MediaSizeResizes Resize { get; set; }
        }
    }
}
