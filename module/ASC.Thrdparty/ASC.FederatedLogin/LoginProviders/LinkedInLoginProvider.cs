/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.Xml.Linq;
using System.Xml.XPath;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.Profile;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using ASC.Thrdparty.TokenManagers;
using DotNetOpenAuth.ApplicationBlock;
using DotNetOpenAuth.OAuth;

namespace ASC.FederatedLogin.LoginProviders
{
    public class LinkedInLoginProvider : ILoginProvider
    {
        private static InMemoryTokenManager ShortTermUserSessionTokenManager
        {
            get
            {
                var store = HttpContext.Current.Session;
                var tokenManager = (InMemoryTokenManager) store["linkedInShortTermManager"];
                if (tokenManager == null)
                {
                    var consumerKey = KeyStorage.Get("linkedInKey");
                    var consumerSecret = KeyStorage.Get("linkedInSecret");
                    tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                    store["linkedInShortTermManager"] = tokenManager;
                }
                return tokenManager;
            }
        }

        private static WebConsumer _signInConsumer;
        private static readonly object SignInConsumerInitLock = new object();

        private static WebConsumer SignIn
        {
            get
            {
                if (_signInConsumer == null)
                {
                    lock (SignInConsumerInitLock)
                    {
                        if (_signInConsumer == null)
                        {
                            _signInConsumer = new WebConsumer(LinkedInConsumer.ServiceDescription, ShortTermUserSessionTokenManager);
                        }
                    }
                }

                return _signInConsumer;
            }
        }

        public LoginProfile ProcessAuthoriztion(HttpContext context, IDictionary<string, string> @params)
        {
            var token = context.Request["oauth_token"];
            if (string.IsNullOrEmpty(token))
            {
                LinkedInConsumer.RequestAuthorization(SignIn);
            }
            else
            {
                var accessTokenResponse = SignIn.ProcessUserAuthorization();
                try
                {
                    return token == null
                               ? LoginProfile.FromError(new Exception("Login failed"))
                               : RequestProfile(accessTokenResponse.AccessToken);
                }
                catch (Exception ex)
                {
                    return LoginProfile.FromError(ex);
                }
            }
            return null;
        }

        public LoginProfile GetLoginProfile(string accessToken)
        {
            try
            {
                return RequestProfile(accessToken);
            }
            catch (Exception ex)
            {
                return LoginProfile.FromError(ex);
            }
        }

        private static LoginProfile RequestProfile(string accessToken)
        {
            var responce = LinkedInConsumer.GetProfile(SignIn, accessToken);
            var document = XDocument.Parse(responce).CreateNavigator();
            return new LoginProfile
                {
                    Id = document.SelectNodeValue("//id"),
                    FirstName = document.SelectNodeValue("//first-name"),
                    LastName = document.SelectNodeValue("//last-name"),
                    Avatar = document.SelectNodeValue("//picture-url"),
                    Provider = ProviderConstants.LinkedIn
                };
        }
    }
}