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