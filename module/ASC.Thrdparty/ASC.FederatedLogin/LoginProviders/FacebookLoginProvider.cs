/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class FacebookLoginProvider : BaseLoginProvider<FacebookLoginProvider>
    {
        private const string FacebookProfileUrl = "https://graph.facebook.com/v2.7/me?fields=email,id,birthday,link,first_name,last_name,gender,timezone,locale";

        public override string AccessTokenUrl { get { return "https://graph.facebook.com/v2.7/oauth/access_token"; } }
        public override string RedirectUri { get { return this["facebookRedirectUrl"]; } }
        public override string ClientID { get { return this["facebookClientId"]; } }
        public override string ClientSecret { get { return this["facebookClientSecret"]; } }
        public override string CodeUrl { get { return "https://www.facebook.com/v2.7/dialog/oauth/"; } }
        public override string Scopes { get { return "email,public_profile"; } }

        public FacebookLoginProvider() { }
        public FacebookLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) {}

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var facebookProfile = RequestHelper.PerformRequest(FacebookProfileUrl + "&access_token=" + accessToken);
            var loginProfile = ProfileFromFacebook(facebookProfile);
            return loginProfile;
        }

        internal static LoginProfile ProfileFromFacebook(string facebookProfile)
        {
            var jProfile = JObject.Parse(facebookProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                              {
                                  BirthDay = jProfile.Value<string>("birthday"),
                                  Link = jProfile.Value<string>("link"),
                                  FirstName = jProfile.Value<string>("first_name"),
                                  LastName = jProfile.Value<string>("last_name"),
                                  Gender = jProfile.Value<string>("gender"),
                                  EMail = jProfile.Value<string>("email"),
                                  Id = jProfile.Value<string>("id"),
                                  TimeZone = jProfile.Value<string>("timezone"),
                                  Locale = jProfile.Value<string>("locale"),
                                  Provider = ProviderConstants.Facebook,
                                  Avatar = string.Format("http://graph.facebook.com/{0}/picture?type=large", jProfile.Value<string>("id"))
                              };

            return profile;
        }
    }
}