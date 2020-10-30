/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.Common.Logging;
using ASC.FederatedLogin;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Files.Classes;
using ASC.Web.Files.ThirdPartyApp;

namespace ASC.Web.Files.Helpers
{
    public class WordpressToken
    {
        public static ILog Log = Global.Logger;
        public const string AppAttr = "wordpress";

        public static OAuth20Token GetToken()
        {
            return Token.GetToken(AppAttr);
        }

        public static void SaveToken(OAuth20Token token)
        {
            if (token == null) throw new ArgumentNullException("token");
            Token.SaveToken(new Token(token, AppAttr));
        }
        public static void DeleteToken(OAuth20Token token)
        {
            if (token == null) throw new ArgumentNullException("token");
            Token.DeleteToken(AppAttr);

        }
    }
    public class WordpressHelper
    {
        public static ILog Log = Global.Logger;
        public enum WordpressStatus
        {
            draft = 0,
            publish = 1
        }

        public static string GetWordpressMeInfo(string token)
        {
            try
            {
                return WordpressLoginProvider.GetWordpressMeInfo(token);
            }
            catch (Exception ex)
            {
                Log.Error("Get Wordpress info about me ", ex);
                return "";
            }
           
        }

        public static bool CreateWordpressPost(string title, string content, int status, string blogId, OAuth20Token token)
        {
            try
            {
                var wpStatus = ((WordpressStatus)status).ToString();
                WordpressLoginProvider.CreateWordpressPost(title, content, wpStatus, blogId, token);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error("Create Wordpress post ", ex);
                return false;
            }
        }
    }
}