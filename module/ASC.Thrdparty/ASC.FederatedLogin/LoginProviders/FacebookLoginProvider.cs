/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using DotNetOpenAuth.ApplicationBlock.Facebook;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth2;
using Facebook;
using FacebookClient = DotNetOpenAuth.ApplicationBlock.FacebookClient;

namespace ASC.FederatedLogin.LoginProviders
{
    class FacebookLoginProvider : ILoginProvider
    {
        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            var builder = new UriBuilder(context.Request.GetUrlRewriter()) {Query = "p=" + context.Request["p"]};
            var oauth = new FacebookOAuthClient
            {
                AppId = KeyStorage.Get("facebookAppID"),
                AppSecret = KeyStorage.Get("facebookAppSecret"),
                RedirectUri = builder.Uri
            };
            FacebookOAuthResult result;
            if (FacebookOAuthResult.TryParse(context.Request.GetUrlRewriter(), out result))
            {
                if (result.IsSuccess)
                {
                    var accessToken = (Facebook.JsonObject)oauth.ExchangeCodeForAccessToken(result.Code);
                    var request = WebRequest.Create("https://graph.facebook.com/me?access_token=" + Uri.EscapeDataString((string)accessToken["access_token"]));
                    using (var response = request.GetResponse())
                    {
                        using (var responseStream = response.GetResponseStream())
                        {
                            var graph = FacebookGraph.Deserialize(responseStream);
                            var profile = ProfileFromFacebook(graph);
                            return profile;
                        }
                    }
                }
                return LoginProfile.FromError(new Exception(result.ErrorReason));
            }
            //Maybe we didn't query
            var extendedPermissions = new[] { "email", "user_about_me" };
            var parameters = new Dictionary<string, object>
                                 {
                                     { "display", "popup" }
                                 };

            if (extendedPermissions.Length > 0)
            {
                var scope = new StringBuilder();
                scope.Append(string.Join(",", extendedPermissions));
                parameters["scope"] = scope.ToString();
            }
            var loginUrl = oauth.GetLoginUrl(parameters);
            context.Response.Redirect(loginUrl.ToString());
            return LoginProfile.FromError(new Exception("Failed to login with facebook"));
        }

        internal static LoginProfile ProfileFromFacebook(FacebookGraph graph)
        {
            var profile = new LoginProfile
                              {
                                  BirthDay = graph.Birthday,
                                  Link = graph.Link.ToString(),
                                  FirstName = graph.FirstName,
                                  LastName = graph.LastName,
                                  Gender = graph.Gender,
                                  DisplayName = graph.FirstName + graph.LastName,
                                  EMail = graph.Email,
                                  Id = graph.Id.ToString(),
                                  TimeZone = graph.Timezone,
                                  Locale = graph.Locale,
                                  Provider = ProviderConstants.Facebook,
                                  Avatar = string.Format("http://graph.facebook.com/{0}/picture?type=large", graph.Id)
                              };

            return profile;
        }
    }
}