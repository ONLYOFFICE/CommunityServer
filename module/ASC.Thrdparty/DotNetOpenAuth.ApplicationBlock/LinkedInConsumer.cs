/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
            using (var response = consumer.Channel.WebRequestHandler.GetResponse(request))
            using (var reader = response.GetResponseReader())
                return reader.ReadToEnd();
        }
    }
}