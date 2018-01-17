/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetOpenAuth.OAuth;
using log4net;

namespace ASC.Thrdparty.Twitter
{
    /// <summary>
    /// Contains methods for getting data from Twitter
    /// </summary>
    public class TwitterDataProvider
    {
        private static WebConsumer _signInConsumer;
        private static readonly object SignInConsumerInitLock = new object();

        private WebConsumer TwitterSignIn
        {
            get
            {
                if (_signInConsumer != null) return _signInConsumer;

                lock (SignInConsumerInitLock)
                {
                    if (_signInConsumer == null)
                    {
                        var tokenManagerHolder = new InMemoryTokenManager(_apiInfo.ConsumerKey, _apiInfo.ConsumerSecret);

                        return _signInConsumer = new WebConsumer(TwitterConsumer.SignInWithTwitterServiceDescription, tokenManagerHolder);
                    }
                }

                return _signInConsumer;
            }
        }

        private readonly TwitterApiInfo _apiInfo;

        public enum ImageSize
        {
            Small,
            Original
        }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="apiInfo">TwitterApiInfo object</param>
        public TwitterDataProvider(TwitterApiInfo apiInfo)
        {
            if (apiInfo == null)
                throw new ArgumentNullException("apiInfo");

            _apiInfo = apiInfo;

            try
            {
                TwitterSignIn.TokenManager.ExpireRequestTokenAndStoreNewAccessToken(
                    apiInfo.ConsumerKey,
                    String.Empty,
                    apiInfo.AccessToken,
                    apiInfo.AccessTokenSecret);
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(typeof (TwitterDataProvider)).Error("TwitterSignIn in TwitterDataProvider", ex);
            }
        }

        private static String ParseTweetTextIntoHtml(String text)
        {
            text = Regex.Replace(text, "(https?://([-\\w\\.]+)+(/([\\w/_\\.]*(\\?\\S+)?(#\\S+)?)?)?)", "<a href='$1'>$1</a>");
            text = Regex.Replace(text, "@(\\w+)", "<a href='https://twitter.com/$1'>@$1</a>");
            text = Regex.Replace(text, "\\s#(\\w+)", "<a href='https://twitter.com/search?q=%23$1&src=hash'>#$1</a>");

            return text;
        }

        private static DateTime ParseTweetDateTime(String dateTimeAsText)
        {
            var month = dateTimeAsText.Substring(4, 3).Trim();
            var dayInMonth = dateTimeAsText.Substring(8, 2).Trim();
            var time = dateTimeAsText.Substring(11, 9).Trim();
            var year = dateTimeAsText.Substring(25, 5).Trim();

            var dateTime = string.Format("{0}-{1}-{2} {3}", dayInMonth, month, year, time);
            return DateTime.Parse(dateTime, CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Gets tweets posted by specified user
        /// </summary>   
        /// <returns>Message list</returns>
        public List<Message> GetUserTweets(decimal? userID, string screenName, int messageCount)
        {
            var localUserId = 0;

            if (userID.HasValue)
                localUserId = (int)userID.Value;

            var userTimeLine = TwitterConsumer.GetUserTimeLine(TwitterSignIn, _apiInfo.AccessToken, localUserId, screenName, true, messageCount);

            if (userTimeLine == null) return new List<Message>();

            return userTimeLine.Select(x => (Message)(new TwitterMessage
                {
                    UserName = x["user"].Value<String>("name"),
                    PostedOn = ParseTweetDateTime(x.Value<String>("created_at")),
                    Source = SocialNetworks.Twitter,
                    Text = ParseTweetTextIntoHtml(x.Value<String>("text")),
                    UserImageUrl = x["user"].Value<String>("profile_image_url")
                })).Take(20).ToList();
        }

        /// <summary>
        /// Gets last 20 users
        /// </summary>
        /// <param name="search">Search string</param>
        /// <returns>TwitterUserInfo list</returns>
        public List<TwitterUserInfo> FindUsers(string search)
        {
            var findedUsers = TwitterConsumer.SearchUsers(TwitterSignIn, search, _apiInfo.AccessToken);

            if (findedUsers == null)
                return new List<TwitterUserInfo>();

            return findedUsers.Select(x => new TwitterUserInfo
                {
                    UserID = x.Value<Decimal>("id"),
                    Description = x.Value<String>("description"),
                    ScreenName = x.Value<String>("screen_name"),
                    SmallImageUrl = x.Value<String>("profile_image_url"),
                    UserName = x.Value<String>("name")
                }).Take(20).ToList();

        }

        /// <summary>
        /// Gets url of User image
        /// </summary>
        /// <returns>Url of image or null if resource does not exist</returns>
        public string GetUrlOfUserImage(string userScreenName, ImageSize imageSize)
        {
            var userInfo = TwitterConsumer.GetUserInfo(TwitterSignIn, 0, userScreenName, _apiInfo.AccessToken);

            if (userInfo == null) return null;

            var profileImageUrl = userInfo.Value<String>("profile_image_url");

            var size = GetTwitterImageSizeText(imageSize);

            if (size == "original")
                profileImageUrl = profileImageUrl.Replace("_normal", String.Empty);

            return profileImageUrl;

        }

        private static string GetTwitterImageSizeText(ImageSize imageSize)
        {
            var result = "original";
            if (imageSize == ImageSize.Small)
                result = "normal";
            return result;
        }
    }
}