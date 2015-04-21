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


#if DEBUG
using ASC.SocialMedia.LinkedIn;
using DotNetOpenAuth.OAuth.ChannelElements;
using DotNetOpenAuth.OAuth.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ASC.SocialMedia.Tests
{
    class LinkedInDBTokenManager : IConsumerTokenManager
    {
        public string ConsumerKey { get; private set; }

        public string ConsumerSecret { get; private set; }


        public LinkedInDBTokenManager(string consumerKey, string consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
        }


        public string GetTokenSecret(string token)
        {
            return "43db680e-6c6a-4069-9155-9c16ef815586";
        }

        public void ExpireRequestTokenAndStoreNewAccessToken(string consumerKey, string requestToken, string accessToken, string accessTokenSecret)
        {
            throw new NotImplementedException();
        }

        public TokenType GetTokenType(string token)
        {
            throw new NotImplementedException();
        }

        public void StoreNewRequestToken(UnauthorizedTokenRequest request, ITokenSecretContainingMessage response)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class LinkedInTest
    {
        [TestMethod]
        public void GetUserInfoTest()
        {
            var tokenManager = new LinkedInDBTokenManager("qnwIL9_wRC4Ew3iLl5sdEKvEDaSTgFn-RRaedF0XfXLZov0jDCq577Ta6wDLZr_8", "gJCNJ4UsvfCgPGHQRQt0CJ82GZTN6njeT1XxhyUaSsYHBAtCf58EE0P0ocBcLLqp");
            var provider = new LinkedInDataProvider(tokenManager, "8a17d3b4-5e99-4f5f-8ad3-5c9f0b28d9d1");
            var userInfo = provider.GetUserInfo("A_lDUH3Vb3");
        }
    }
}
#endif