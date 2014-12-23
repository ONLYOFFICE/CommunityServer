/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
    class LinkedInLoginProvider : ILoginProvider
    {
        private static InMemoryTokenManager ShortTermUserSessionTokenManager
        {
            get
            {
                var store = HttpContext.Current.Session;
                var tokenManager = (InMemoryTokenManager)store["linkedInShortTermManager"];
                if (tokenManager == null)
                {
                    string consumerKey = KeyStorage.Get("linkedInKey");
                    string consumerSecret = KeyStorage.Get("linkedInSecret");
                    tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                    store["linkedInShortTermManager"] = tokenManager;
                }
                return tokenManager;
            }
        }

        private static WebConsumer _signInConsumer = null;
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
        //<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        //<person>
        //  <id>FQJIJg-u80</id>
        //  <first-name>alexander</first-name>
        //  <last-name>rusanov</last-name>
        //</person>

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
                if (accessTokenResponse != null)
                {
                    //All ok. request info
                    var responce = LinkedInConsumer.GetProfile(SignIn, accessTokenResponse.AccessToken);
                    var document = XDocument.Parse(responce).CreateNavigator();
                    return new LoginProfile()
                               {
                                   Id = document.SelectNodeValue("//id"),
                                   FirstName = document.SelectNodeValue("//first-name"),
                                   LastName = document.SelectNodeValue("//last-name"),
                                   Avatar = document.SelectNodeValue("//picture-url"),
                                   Provider = ProviderConstants.LinkedIn
                               };
                }
                return LoginProfile.FromError(new Exception("Login failed"));
            }
            return null;
        }
    }
}