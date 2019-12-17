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
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using Newtonsoft.Json.Linq;

namespace ASC.FederatedLogin.LoginProviders
{
    public class GoogleLoginProvider : BaseLoginProvider<GoogleLoginProvider>
    {
        public const string GoogleScopeContacts = "https://www.googleapis.com/auth/contacts.readonly";
        public const string GoogleScopeDrive = "https://www.googleapis.com/auth/drive";
        //https://developers.google.com/gmail/imap/xoauth2-protocol
        public const string GoogleScopeMail = "https://mail.google.com/";

        public const string GoogleUrlContacts = "https://www.google.com/m8/feeds/contacts/default/full/";
        public const string GoogleUrlFile = "https://www.googleapis.com/drive/v3/files/";
        public const string GoogleUrlFileUpload = "https://www.googleapis.com/upload/drive/v3/files";
        public const string GoogleUrlProfile = "https://people.googleapis.com/v1/people/me";

        public static readonly string[] GoogleDriveExt = new[] { ".gdoc", ".gsheet", ".gslides", ".gdraw" };
        public static string GoogleDriveMimeTypeFolder = "application/vnd.google-apps.folder";
        public static string FilesFields = "id,name,mimeType,parents,createdTime,modifiedTime,owners/displayName,lastModifyingUser/displayName,capabilities/canEdit,size";
        public static string ProfileFields = "emailAddresses,genders,names";

        public override string AccessTokenUrl { get { return "https://www.googleapis.com/oauth2/v4/token"; } }
        public override string CodeUrl { get { return "https://accounts.google.com/o/oauth2/v2/auth"; } }
        public override string RedirectUri { get { return this["googleRedirectUrl"]; } }
        public override string ClientID { get { return this["googleClientId"]; } }
        public override string ClientSecret { get { return this["googleClientSecret"]; } }
        public override string Scopes { get { return "https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email"; } }

        public GoogleLoginProvider() { }
        public GoogleLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) { }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
                throw new Exception("Login failed");

            return RequestProfile(accessToken);
        }

        public OAuth20Token Auth(HttpContext context)
        {
            return Auth(context, GoogleScopeContacts, (context.Request["access_type"] ?? "") == "offline"
                                                          ? new Dictionary<string, string>
                                                              {
                                                                  { "access_type", "offline" },
                                                                  { "prompt", "consent" }
                                                              }
                                                          : null);
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var googleProfile = RequestHelper.PerformRequest(GoogleUrlProfile + "?personFields=" + HttpUtility.UrlEncode(ProfileFields), headers: new Dictionary<string, string> { { "Authorization", "Bearer " + accessToken } });
            var loginProfile = ProfileFromGoogle(googleProfile);
            return loginProfile;
        }

        private static LoginProfile ProfileFromGoogle(string googleProfile)
        {
            var jProfile = JObject.Parse(googleProfile);
            if (jProfile == null) throw new Exception("Failed to correctly process the response");

            var profile = new LoginProfile
                {
                    Id = jProfile.Value<string>("resourceName").Replace("people/", ""),
                    Provider = ProviderConstants.Google,
                };

            var emailsArr = jProfile.Value<JArray>("emailAddresses");
            if (emailsArr != null)
            {
                var emailsList = emailsArr.ToObject<List<GoogleEmailAddress>>();
                if (emailsList.Count > 0)
                {
                    var ind = emailsList.FindIndex(googleEmail => googleEmail.metadata.primary);
                    profile.EMail = emailsList[ind > -1 ? ind : 0].value;
                }
            }

            var namesArr = jProfile.Value<JArray>("names");
            if (namesArr != null)
            {
                var namesList = namesArr.ToObject<List<GoogleName>>();
                if (namesList.Count > 0)
                {
                    var ind = namesList.FindIndex(googleName => googleName.metadata.primary);
                    var name = namesList[ind > -1 ? ind : 0];
                    profile.DisplayName = name.displayName;
                    profile.FirstName = name.givenName;
                    profile.LastName = name.familyName;
                }
            }

            var gendersArr = jProfile.Value<JArray>("genders");
            if (gendersArr != null)
            {
                var gendersList = gendersArr.ToObject<List<GoogleGender>>();
                if (gendersList.Count > 0)
                {
                    var ind = gendersList.FindIndex(googleGender => googleGender.metadata.primary);
                    profile.Gender = gendersList[ind > -1 ? ind : 0].value;
                }
            }

            return profile;
        }

        private class GoogleEmailAddress
        {
            public GoogleMetadata metadata = new GoogleMetadata();
            public string value = null;
        }

        private class GoogleGender
        {
            public GoogleMetadata metadata = new GoogleMetadata();
            public string value = null;
        }

        private class GoogleName
        {
            public GoogleMetadata metadata = new GoogleMetadata();
            public string displayName = null;
            public string familyName = null;
            public string givenName = null;
        }

        private class GoogleMetadata
        {
            public bool primary = false;
        }
    }
}