/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace ASC.Thrdparty.Twitter
{
    /// <summary>
    /// Contains methods for getting data from Twitter
    /// </summary>
    public class TwitterDataProvider
    {
        public enum ImageSize
        {
            Small,
            Original
        }

        private TwitterClient _twitterClient;


        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="apiInfo">TwitterApiInfo object</param>
        public TwitterDataProvider(TwitterApiInfo apiInfo)
        {
            if (apiInfo == null)
                throw new ArgumentNullException("apiInfo");

            _twitterClient = new TwitterClient(apiInfo.ConsumerKey, apiInfo.ConsumerSecret, apiInfo.AccessToken, apiInfo.AccessTokenSecret);
        }

        private static String ParseTweetTextIntoHtml(String text)
        {
            text = Regex.Replace(text, "(https?://([-\\w\\.]+)+(/([\\w/_\\.]*(\\?\\S+)?(#\\S+)?)?)?)", "<a href='$1'>$1</a>");
            text = Regex.Replace(text, "@(\\w+)", "<a href='https://twitter.com/$1'>@$1</a>");
            text = Regex.Replace(text, "\\s#(\\w+)", "<a href='https://twitter.com/search?q=%23$1&src=hash'>#$1</a>");

            return text;
        }

        /// <summary>
        /// Gets tweets posted by specified user
        /// </summary>   
        /// <returns>Message list</returns>
        public List<Message> GetUserTweets(string screenName, int messageCount)
        {                      

            var tweets = _twitterClient.Timelines.GetUserTimelineAsync(new GetUserTimelineParameters(screenName)
            {
                PageSize = messageCount
            }).Result;

            return tweets.Select(x => (Message)new TwitterMessage
            {
                UserName = x.CreatedBy.Name,
                PostedOn = x.CreatedAt.DateTime,
                Source = SocialNetworks.Twitter,
                Text = ParseTweetTextIntoHtml(x.Text),
                UserImageUrl = x.CreatedBy.ProfileImageUrl,
                UserId = x.Id.ToString()
            }).Take(20).ToList();
        }

        /// <summary>
        /// Gets last 20 users
        /// </summary>
        /// <param name="search">Search string</param>
        /// <returns>TwitterUserInfo list</returns>
        public List<TwitterUserInfo> FindUsers(string search)
        {
            var findedUsers = _twitterClient.Search.SearchUsersAsync(search).Result;

            return findedUsers.Select(x => new TwitterUserInfo
            {
                UserID = x.Id,
                Description = x.Description,
                ScreenName = x.ScreenName,
                SmallImageUrl = x.ProfileImageUrl,
                UserName = x.Name
            }).Take(20).ToList();
        }

        /// <summary>
        /// Gets url of User image
        /// </summary>
        /// <returns>Url of image or null if resource does not exist</returns>
        public string GetUrlOfUserImage(string userScreenName, ImageSize imageSize)
        {
            var userInfo = _twitterClient.Users.GetUserAsync(userScreenName).Result;

            if (userInfo == null) return null;

            var profileImageUrl = userInfo.ProfileImageUrl;

            var size = GetTwitterImageSizeText(imageSize);

            if (size == "original")
                profileImageUrl = profileImageUrl.Replace("_normal", String.Empty);

            return profileImageUrl;
        }

        private static string GetTwitterImageSizeText(ImageSize imageSize)
        {
            var result = "original";

            if (imageSize == ImageSize.Small)
            {
                result = "normal";
            }

            return result;
        }
    }
}