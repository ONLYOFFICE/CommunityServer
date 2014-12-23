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
using System.Configuration;
using System.Web;
using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OAuth;
using DotNetOpenAuth.OAuth.ChannelElements;

namespace DotNetOpenAuth.ApplicationBlock
{
    public class LinkedInConsumer
    {
        /// <summary>
        /// The Consumer to use for accessing Google data APIs.
        /// </summary>
        public static readonly ServiceProviderDescription ServiceDescription = new ServiceProviderDescription
        {
            RequestTokenEndpoint = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/requestToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
            UserAuthorizationEndpoint = new MessageReceivingEndpoint("https://www.linkedin.com/uas/oauth/authenticate", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
            AccessTokenEndpoint = new MessageReceivingEndpoint("https://api.linkedin.com/uas/oauth/accessToken", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest),
            TamperProtectionElements = new ITamperProtectionChannelBindingElement[] { new HmacSha1SigningBindingElement() },
        };

        private static readonly MessageReceivingEndpoint ProfileEndpoint = new MessageReceivingEndpoint("http://api.linkedin.com/v1/people/~:(id,first-name,last-name,picture-url)", HttpDeliveryMethods.AuthorizationHeaderRequest | HttpDeliveryMethods.GetRequest);


        //http://api.linkedin.com/v1/people/~
        public static void RequestAuthorization(WebConsumer consumer)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }
            Uri callback = HttpRequestExtensions.GetUrlRewriter(HttpContext.Current.Request.Headers, Util.GetCallbackUrlFromContext().StripQueryArgumentsWithPrefix("oauth_"));
            var request = consumer.PrepareRequestUserAuthorization(callback, null, null);
            consumer.Channel.Send(request);
        }

        public static string GetProfile(ConsumerBase consumer, string accessToken)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException("consumer");
            }

            var request = consumer.PrepareAuthorizedRequest(ProfileEndpoint, accessToken, new Dictionary<string, string>());
            var response = consumer.Channel.WebRequestHandler.GetResponse(request);
            string body = response.GetResponseReader().ReadToEnd();
            return body;
        }
    }
}