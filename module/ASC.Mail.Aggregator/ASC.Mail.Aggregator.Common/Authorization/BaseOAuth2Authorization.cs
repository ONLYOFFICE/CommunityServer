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
using ASC.Mail.Aggregator.Common.Logging;
using DotNetOpenAuth.OAuth2;

namespace ASC.Mail.Aggregator.Common.Authorization
{
    public class BaseOAuth2Authorization
    {
        public readonly ILogger log;

        public string ClientId { get; protected set; }
        public string ClientSecret { get; protected set; }
        public string RedirectUrl { get; protected set; }
        public AuthorizationServerDescription ServerDescription { get; protected set; }
        public List<string> Scope { get; protected set; }

        public BaseOAuth2Authorization(ILogger log)
        {
            if(null == log)
                throw new ArgumentNullException("log");

            this.log = log;
        }

        private IAuthorizationState PrepareAuthorizationState(string refreshToken)
        {
            return new AuthorizationState(Scope)
            {
                RefreshToken = refreshToken,
                Callback = new Uri(RedirectUrl),
            };

        }

        public IAuthorizationState RequestAccessToken(string refreshToken)
        {
            {
                WebServerClient consumer = new WebServerClient(ServerDescription, ClientId, ClientSecret)
                {
                    AuthorizationTracker = new AuthorizationTracker(Scope)
                };

                IAuthorizationState grantedAccess = PrepareAuthorizationState(refreshToken);

                if (grantedAccess != null)
                {
                    try
                    {
                        consumer.ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ClientSecret);
                        consumer.RefreshAuthorization(grantedAccess);

                        return grantedAccess;
                    }
                    catch (Exception ex)
                    {
                        log.Error("RefreshAuthorization() Exception:\r\n{0}\r\n", ex.ToString());
                    }
                }

                return null;
            }
        }
    }
}
