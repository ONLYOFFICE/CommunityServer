//-----------------------------------------------------------------------
// <copyright file="TwitterUser.cs" company="Patrick 'Ricky' Smith">
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
// <summary>The TwitterUser class.</summary>
//-----------------------------------------------------------------------
namespace Twitterizer
{
    using System;
    using System.Diagnostics;
#if !SILVERLIGHT
    using System.Drawing;
#endif
    using Newtonsoft.Json;
    using Core;
    using System.Runtime.Serialization;

    /// <include file='TwitterUser.xml' path='TwitterUser/TwitterUser/*'/>
    [JsonObject(MemberSerialization.OptIn)]
    [DebuggerDisplay("@{ScreenName}")]
#if !SILVERLIGHT
    [Serializable]
#endif
    [DataContract]
    public class TwitterUser : TwitterObject
    {
        #region Properties
        /// <summary>
        /// Gets or sets the User ID.
        /// </summary>
        /// <value>The User ID.</value>
        [DataMember, JsonProperty(PropertyName = "id")]
        public decimal Id { get; set; }

        /// <summary>
        /// Gets or sets the string id.
        /// </summary>
        /// <value>The string id.</value>
        [DataMember, JsonProperty(PropertyName = "id_str")]
        public string StringId { get; set; }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        [DataMember, JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        /// <value>The location.</value>
        [DataMember, JsonProperty(PropertyName = "location")]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember, JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has not altered the theme or background of their profile.
        /// </summary>
        /// <value><c>false</c> if the user has altered the theme or background of their profile; otherwise, <c>true</c>.</value>
        [DataMember, JsonProperty(PropertyName = "default_profile")]
        public bool IsDefaultProfile { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has not uploaded their own avatar and a default egg avatar is used instead.
        /// </summary>
        /// <value><c>false</c> if the user has uploaded their own avatar and a default egg avatar is used instead; otherwise, <c>true</c>.</value>
        [DataMember, JsonProperty(PropertyName = "default_profile_image")]
        public bool IsDefaultProfileImage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user is a translator.
        /// </summary>
        /// <value><c>true</c> if this user is translator; otherwise, <c>false</c>.</value>
        [DataMember, JsonProperty(PropertyName = "is_translator")]
        public bool IsTranslator { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        [DataMember, JsonProperty(PropertyName = "status")]
        public TwitterStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the created date.
        /// </summary>
        /// <value>The created date.</value>
        [JsonProperty(PropertyName = "created_at")]
        [JsonConverter(typeof(TwitterizerDateConverter))]
        [DataMember]
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        /// <value>The time zone.</value>
        [DataMember, JsonProperty(PropertyName = "time_zone")]
        public string TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the number of followers.
        /// </summary>
        /// <value>The number of followers.</value>
        [DataMember, JsonProperty(PropertyName = "followers_count")]
        public long? NumberOfFollowers { get; set; }

        /// <summary>
        /// Gets or sets the number of statuses.
        /// </summary>
        /// <value>The number of statuses.</value>
        [DataMember, JsonProperty(PropertyName = "statuses_count")]
        public long NumberOfStatuses { get; set; }

        /// <summary>
        /// Gets or sets the number of friends.
        /// </summary>
        /// <value>The number of friends.</value>
        [DataMember, JsonProperty(PropertyName = "friends_count")]
        public long NumberOfFriends { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user has enabled contributors access to his or her account.
        /// </summary>
        /// <value>The is contributors enabled value.</value>
        [DataMember, JsonProperty(PropertyName = "contributors_enabled")]
        public bool IsContributorsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>The language.</value>
        [DataMember, JsonProperty(PropertyName = "lang")]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the screenname.
        /// </summary>
        /// <value>The screenname.</value>
        [DataMember, JsonProperty(PropertyName = "screen_name")]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets the number of favorites.
        /// </summary>
        /// <value>The number of favorites.</value>
        [DataMember, JsonProperty(PropertyName = "favourites_count")]
        public long NumberOfFavorites { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user is protected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user is protected; otherwise, <c>false</c>.
        /// </value>
        [DataMember, JsonProperty(PropertyName = "protected")]
        public bool IsProtected { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this user is geo enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user is geo enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember, JsonProperty(PropertyName = "geo_enabled")]
        public bool? IsGeoEnabled { get; set; }

        /// <summary>
        /// Gets or sets the countries the user is withheld from.
        /// </summary>
        /// <value>The countries withheld from.</value>
        /// <remarks>See also: https://dev.twitter.com/blog/new-withheld-content-fields-api-responses </remarks>
        [DataMember, JsonProperty(PropertyName = "withheld_in_countries")]
        public string CountriesWithheld { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the content being withheld is the "status" or a "user."
        /// </summary>
        /// <value>The withheld scope.</value>
        /// <remarks>See also: https://dev.twitter.com/blog/new-withheld-content-fields-api-responses </remarks>
        [DataMember, JsonProperty(PropertyName = "withheld_scope")]
        public string WithheldScope { get; set; }

        /// <summary>
        /// Gets or sets the time zone offset.
        /// </summary>
        /// <value>The time zone offset.</value>
        /// <remarks>Also called the Coordinated Universal Time (UTC) offset.</remarks>
        [DataMember, JsonProperty(PropertyName = "utc_offset")]
        public double? TimeZoneOffset { get; set; }

        /// <summary>
        /// Gets or sets the user's website.
        /// </summary>
        /// <value>The website address.</value>
        [DataMember, JsonProperty(PropertyName = "url")]
        public string Website { get; set; }

        /// <summary>
        /// Gets or sets the listed count.
        /// </summary>
        /// <value>The listed count.</value>
        [DataMember, JsonProperty(PropertyName = "listed_count")]
        public int ListedCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [follow request sent].
        /// </summary>
        /// <value><c>true</c> if [follow request sent]; otherwise, <c>false</c>.</value>
        [DataMember, JsonProperty(PropertyName = "follow_request_sent")]
        public bool? FollowRequestSent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user is verified.
        /// </summary>
        /// <value><c>true</c> if the user is verified; otherwise, <c>false</c>.</value>
        [DataMember, JsonProperty(PropertyName = "verified")]
        public bool? Verified { get; set; }

        #region Profile Layout Properties
        /// <summary>
        /// Gets or sets the color of the profile background.
        /// </summary>
        /// <value>The color of the profile background.</value>
        [DataMember, JsonProperty(PropertyName = "profile_background_color")]
        public string ProfileBackgroundColorString { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the color of the profile background.
        /// </summary>
        /// <value>The color of the profile background.</value>
        [DataMember]
        public Color ProfileBackgroundColor
        {
            get
            {
                return ConversionUtility.FromTwitterString(this.ProfileBackgroundColorString);
            }
        }
#endif
        /// <summary>
        /// Gets or sets a value indicating whether this user's profile background image is tiled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this user's profile background image is tiled; otherwise, <c>false</c>.
        /// </value>
        [DataMember, JsonProperty(PropertyName = "profile_background_tile")]
        public bool? IsProfileBackgroundTiled { get; set; }

        /// <summary>
        /// Gets or sets the color of the profile link.
        /// </summary>
        /// <value>The color of the profile link.</value>
        [DataMember, JsonProperty(PropertyName = "profile_link_color")]
        public string ProfileLinkColorString { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the color of the profile link.
        /// </summary>
        /// <value>The color of the profile link.</value>
        [DataMember]
        public Color ProfileLinkColor
        {
            get
            {
                return ConversionUtility.FromTwitterString(this.ProfileLinkColorString);
            }
        }
#endif

        /// <summary>
        /// Gets or sets the profile background image location.
        /// </summary>
        /// <value>The profile background image location.</value>
        [DataMember, JsonProperty(PropertyName = "profile_background_image_url")]
        public string ProfileBackgroundImageLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user wants their uploaded background image to be used.
        /// </summary>
        /// <value><c>true</c> if the user wants their uploaded background image to be used; otherwise, <c>false</c>.</value>
        [DataMember, JsonProperty(PropertyName = "profile_use_background_image")]
        public bool UseProfileBackgroundImage { get; set; }

        /// <summary>
        /// Gets or sets the HTTPS-based URL pointing to the standard web representation of the user's uploaded profile banner.
        /// </summary>
        /// <remarks>By adding a final path element of the URL, you can obtain different image sizes optimized for specific displays. See also: https://dev.twitter.com/docs/user-profile-images-and-banners </remarks>
        /// <value>The profile banner location.</value>
        [DataMember, JsonProperty(PropertyName = "profile_banner_url")]
        public string ProfileBannerLocation { get; set; }

        /// <summary>
        /// Gets or sets the color of the profile text.
        /// </summary>
        /// <value>The color of the profile text.</value>
        [DataMember, JsonProperty(PropertyName = "profile_text_color")]
        public string ProfileTextColorString { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the color of the profile text.
        /// </summary>
        /// <value>The color of the profile text.</value>
        [DataMember]
        public Color ProfileTextColor
        {
            get
            {
                return ConversionUtility.FromTwitterString(this.ProfileTextColorString);
            }
        }
#endif

        /// <summary>
        /// Gets or sets the profile image location.
        /// </summary>
        /// <value>The profile image location.</value>
        [DataMember, JsonProperty(PropertyName = "profile_image_url")]
        public string ProfileImageLocation { get; set; }

        /// <summary>
        /// Gets or sets the secure profile image location (https).
        /// </summary>
        /// <value>The profile image location.</value>
        [DataMember, JsonProperty(PropertyName = "profile_image_url_https")]
        public string ProfileImageSecureLocation { get; set; }

        /// <summary>
        /// Gets or sets the color of the profile sidebar border.
        /// </summary>
        /// <value>The color of the profile sidebar border.</value>
        [DataMember, JsonProperty(PropertyName = "profile_sidebar_border_color")]
        public string ProfileSidebarBorderColorString { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the color of the profile sidebar border.
        /// </summary>
        /// <value>The color of the profile sidebar border.</value>
        [DataMember]
        public Color ProfileSidebarBorderColor
        {
            get
            {
                return ConversionUtility.FromTwitterString(this.ProfileSidebarBorderColorString);
            }
        }
#endif

        /// <summary>
        /// Gets or sets the profile sidebar fill color string.
        /// </summary>
        /// <value>The profile sidebar fill color string.</value>
        [DataMember, JsonProperty(PropertyName = "profile_sidebar_fill_color")]
        public string ProfileSidebarFillColorString { get; set; }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the color of the profile sidebar fill.
        /// </summary>
        /// <value>The color of the profile sidebar fill.</value>
        [DataMember]
        public Color ProfileSidebarFillColor
        {
            get
            {
                return ConversionUtility.FromTwitterString(this.ProfileSidebarFillColorString);
            }
        }
#endif
        #endregion

        #endregion

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByIDWithTokensAndOptions"]/*'/>
        public static TwitterResponse<TwitterUser> Show(OAuthTokens tokens, decimal id, OptionalProperties options)
        {
            Commands.ShowUserCommand command = new Commands.ShowUserCommand(tokens, id, string.Empty, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByIDWithOptions"]/*'/>
        public static TwitterResponse<TwitterUser> Show(decimal id, OptionalProperties options)
        {
            return Show(null, id, options);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByIDWithTokens"]/*'/>
        public static TwitterResponse<TwitterUser> Show(OAuthTokens tokens, decimal id)
        {
            return Show(tokens, id, null);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByID"]/*'/>
        public static TwitterResponse<TwitterUser> Show(decimal id)
        {
            return Show(null, id, null);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByUsernameWithTokensAndOptions"]/*'/>
        public static TwitterResponse<TwitterUser> Show(OAuthTokens tokens, string username, OptionalProperties options)
        {
            Commands.ShowUserCommand command = new Commands.ShowUserCommand(tokens, 0, username, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByUsernameWithOptions"]/*'/>
        public static TwitterResponse<TwitterUser> Show(string username, OptionalProperties options)
        {
            return Show(null, username, options);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByUsernameWithTokens"]/*'/>
        public static TwitterResponse<TwitterUser> Show(OAuthTokens tokens, string username)
        {
            return Show(tokens, username, null);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Show[@name="ByUsername"]/*'/>
        public static TwitterResponse<TwitterUser> Show(string username)
        {
            return Show(null, username, null);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Search[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Search[@name="WithTokensAndOptions"]/*'/>
        public static TwitterResponse<TwitterUserCollection> Search(OAuthTokens tokens, string query, UserSearchOptions options)
        {
            Commands.UserSearchCommand command = new Commands.UserSearchCommand(tokens, query, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <include file='TwitterUser.xml' path='TwitterUser/Search[@name="Common"]/*'/>
        /// <include file='TwitterUser.xml' path='TwitterUser/Search[@name="WithTokens"]/*'/>
        public static TwitterResponse<TwitterUserCollection> Search(OAuthTokens tokens, string query)
        {
            return Search(tokens, query, null);
        }

        /// <summary>
        /// Return up to 100 users worth of extended information, specified by either ID, screen name, or combination of the two.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="options">The options.</param>
        /// <returns></returns>
        public static TwitterResponse<TwitterUserCollection> Lookup(OAuthTokens tokens, LookupUsersOptions options)
        {
            Commands.LookupUsersCommand command = new Commands.LookupUsersCommand(tokens, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        #region Retweeted By
        /// <summary>
        /// Show user objects of up to 100 members who retweeted the status.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="statusId">The status id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of user objects.</returns>
        public static TwitterResponse<TwitterUserCollection> RetweetedBy(OAuthTokens tokens, decimal statusId, RetweetedByOptions options)
        {
            Commands.RetweetedByCommand command = new Commands.RetweetedByCommand(tokens, statusId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Show user objects of up to 100 members who retweeted the status.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="statusId">The status id.</param>
        /// <returns>A collection of user objects.</returns>
        public static TwitterResponse<TwitterUserCollection> RetweetedBy(OAuthTokens tokens, decimal statusId)
        {
            return RetweetedBy(tokens, statusId, null);
        }

        /// <summary>
        /// Show user ids of up to 100 members who retweeted the status.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="statusId">The status id.</param>
        /// <param name="options">The options.</param>
        /// <returns>A collection of user ids.</returns>
        public static TwitterResponse<UserIdCollection> RetweetedByIds(OAuthTokens tokens, decimal statusId, RetweetedByIdsOptions options)
        {
            Commands.RetweetedByIdsCommand command = new Commands.RetweetedByIdsCommand(tokens, statusId, options);

            return Core.CommandPerformer.PerformAction(command);
        }

        /// <summary>
        /// Show user ids of up to 100 members who retweeted the status.
        /// </summary>
        /// <param name="tokens">The tokens.</param>
        /// <param name="statusId">The status id.</param>
        /// <returns>A collection of user ids.</returns>
        public static TwitterResponse<UserIdCollection> RetweetedByIds(OAuthTokens tokens, decimal statusId)
        {
            return RetweetedByIds(tokens, statusId, null);
        }
        #endregion
    }
}
