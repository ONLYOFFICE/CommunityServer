/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
using System.Linq;
using System.Net;
using System.Web;
using Facebook;
using log4net;

namespace ASC.Thrdparty.Facebook
{
    public class FacebookDataProvider
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (FacebookDataProvider));
        private readonly FacebookApiInfo _apiInfo;
        private const string Status = "status";
        private const string Link = "link";
        private const string Photo = "photo";
        private const string Video = "video";


        public FacebookDataProvider(FacebookApiInfo apiInfo)
        {
            if (apiInfo == null) throw new ArgumentNullException("apiInfo");

            _apiInfo = apiInfo;
        }

        public FacebookUserInfo LoadCurrentUserInfo()
        {
            try
            {
                var client = new FacebookClient
                    {
                        AccessToken = _apiInfo.AccessToken
                    };

                var jsonData = (JsonObject)client.Get("me");

                return Map(jsonData);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw CreateException(ex);
            }
        }

        public FacebookUserInfo LoadUserInfo(string userID)
        {
            try
            {
                var client = new FacebookClient
                    {
                        AccessToken = _apiInfo.AccessToken
                    };

                var jsonData = (JsonObject)client.Get(userID);

                return Map(jsonData);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw CreateException(ex);
            }
        }

        public List<Message> LoadUserWall(string userID, int messageCount)
        {
            try
            {
                var client = new FacebookClient { AccessToken = _apiInfo.AccessToken };

                var parameters = new Dictionary<string, object>
                    {
                        {
                            "limit", messageCount
                        }
                    };

                var url = String.Format("https://graph.facebook.com/{0}/feed", userID);
                var jsonData = (JsonObject)client.Get(url, parameters);
                return MapMessage(jsonData);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw CreateException(ex);
            }
        }

        public List<FacebookUserInfo> FindPages(string search, bool isUser)
        {
            try
            {
                const int pageRowCount = 20;

                var client = new FacebookClient
                    {
                        AccessToken = _apiInfo.AccessToken
                    };

                var parameters = new Dictionary<string, object>
                    {
                        {
                            "limit", pageRowCount
                        }
                    };

                var url = String.Format("https://graph.facebook.com/search?q={0}&type={1}", HttpUtility.UrlEncode(search), isUser ? "user" : "page");
                var jsonData = (JsonObject)client.Get(url, parameters);

                return MapUserList(jsonData);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw CreateException(ex);
            }
        }

        public enum ImageSize
        {
            Small,
            Original
        }

        /// <summary>
        /// Gets url of User image
        /// </summary>
        /// <returns>Url of image or null if does not exist</returns>
        public string GetUrlOfUserImage(string userID, ImageSize imageSize)
        {
            var url = String.Format("http://graph.facebook.com/{0}/picture?type={1}", userID, GetFacebookImageSizeText(imageSize));

            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            try
            {
                using (var response = request.GetResponse())
                {
                    if (response.ContentType.ToLower().Contains("text/javascript"))
                        return null;
                    return response.ResponseUri.ToString();
                }
            }
            catch (WebException ex)
            {
                log.Error(ex);
                throw CreateException(ex);
            }
        }

        private static string GetFacebookImageSizeText(ImageSize imageSize)
        {
            var result = "large";
            if (imageSize == ImageSize.Small)
                result = "small";
            return result;
        }


        private static FacebookUserInfo Map(JsonObject jsonUser)
        {
            return new FacebookUserInfo
                {
                    UserID = Convert.ToString(jsonUser["id"]),
                    UserName = Convert.ToString(jsonUser["name"]),
                };
        }

        private static List<FacebookUserInfo> MapUserList(JsonObject jsonUsers)
        {
            var jsonUserArray = (JsonArray)jsonUsers["data"];

            return (from JsonObject jsonUser in jsonUserArray
                    select new FacebookUserInfo
                        {
                            UserID = Convert.ToString(jsonUser["id"]),
                            UserName = Convert.ToString(jsonUser["name"])
                        }).ToList();
        }

        private List<Message> MapMessage(JsonObject jsonData)
        {
            var messages = new List<Message>();

            var data = (JsonArray)jsonData["data"];

            foreach (JsonObject jsonMsg in data)
            {
                var messageType = (string)jsonMsg["type"];

                switch (messageType)
                {
                    case Status:
                        messages.Add(CreateStatusMessage(jsonMsg));
                        break;
                    case Link:
                        messages.Add(CreateLinkMessage(jsonMsg));
                        break;
                    case Photo:
                        messages.Add(CreatePhotoMessage(jsonMsg));
                        break;
                    case Video:
                        messages.Add(CreateVideoMessage(jsonMsg));
                        break;
                }
            }

            return messages;
        }

        private static Message CreateLinkMessage(JsonObject jsonData)
        {
            var msg = new FacebookMessage();

            var text = "<br />";

            if (jsonData.ContainsKey("message"))
            {
                text += HttpUtility.HtmlEncode((string)jsonData["message"]);
                text += "<br /><br />";
            }
            text += String.Format("<a href='{0}' target='_blank'>{1}</a><br />", jsonData["link"], jsonData["name"]);

            if (jsonData.ContainsKey("caption"))
            {
                text += "<span class='facebookLinkDescription'>" + jsonData["caption"] + "</span>";
                text += "<br />";
            }
            text += "<br />";
            if (jsonData.ContainsKey("description"))
            {
                text += "<span class='facebookLinkDescription'>" + jsonData["description"] + "</span>";
                text += "<br />";
            }

            msg.Text = text;
            msg.PostedOn = Convert.ToDateTime(jsonData["updated_time"].ToString());
            msg.Source = SocialNetworks.Facebook;

            var jsonUser = (JsonObject)(jsonData["from"]);
            msg.UserName = jsonUser["name"].ToString();
            msg.UserImageUrl = String.Format("http://graph.facebook.com/{0}/picture?type=small", jsonUser["id"]);

            return msg;
        }

        private static Message CreatePhotoMessage(JsonObject jsonData)
        {
            var msg = new FacebookMessage();

            var text = "<br />";

            if (jsonData.ContainsKey("message"))
            {
                text += HttpUtility.HtmlEncode((string)jsonData["message"]);
                text += "<br /><br />";
            }
            text += String.Format("<a href='{0}' target='_blank'><img src='{1}' /></a><br /><br />", jsonData["link"], jsonData["picture"]);

            msg.Text = text;
            msg.PostedOn = Convert.ToDateTime(jsonData["updated_time"].ToString());
            msg.Source = SocialNetworks.Facebook;

            var jsonUser = (JsonObject)(jsonData["from"]);
            msg.UserName = jsonUser["name"].ToString();
            msg.UserImageUrl = String.Format("http://graph.facebook.com/{0}/picture?type=small", jsonUser["id"]);

            return msg;
        }

        private static Message CreateVideoMessage(JsonObject jsonData)
        {
            var msg = new FacebookMessage();

            var text = "<br />";

            if (jsonData.ContainsKey("message"))
            {
                text += HttpUtility.HtmlEncode((string)jsonData["message"]);
                text += "<br /><br />";
            }
            text += String.Format("<a href='{0}' target='_blank'><img src='{1}' /></a><br /><br />", jsonData["link"], jsonData["picture"]);

            msg.Text = text;
            msg.PostedOn = Convert.ToDateTime(jsonData["updated_time"].ToString());
            msg.Source = SocialNetworks.Facebook;

            var jsonUser = (JsonObject)(jsonData["from"]);
            msg.UserName = jsonUser["name"].ToString();
            msg.UserImageUrl = String.Format("http://graph.facebook.com/{0}/picture?type=small", jsonUser["id"]);

            return msg;
        }

        private static Message CreateStatusMessage(JsonObject jsonData)
        {
            var msg = new FacebookMessage
                {
                    Text = HttpUtility.HtmlEncode((string)jsonData["message"]),
                    PostedOn = Convert.ToDateTime(jsonData["updated_time"].ToString()),
                    Source = SocialNetworks.Facebook
                };

            var jsonUser = (JsonObject)(jsonData["from"]);
            msg.UserName = jsonUser["name"].ToString();
            msg.UserImageUrl = String.Format("http://graph.facebook.com/{0}/picture?type=small", jsonUser["id"]);

            return msg;
        }

        private static Exception CreateException(Exception ex)
        {
            if (ex is FacebookApiLimitException)
            {
                throw new APILimitException();
            }
            if (ex is FacebookOAuthException)
            {
                throw new OAuthException();
            }
            throw new SocialMediaException();
        }
    }
}