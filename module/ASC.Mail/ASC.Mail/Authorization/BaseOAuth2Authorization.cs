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
using ASC.Common.Logging;
using ASC.Core.Common.Configuration;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;

namespace ASC.Mail.Authorization
{
    public class BaseOAuth2Authorization<T> where T : Consumer, ILoginProvider, new()
    {
        public readonly ILog log;

        private readonly T loginProvider;

        public string ClientId
        {
            get { return loginProvider.ClientID; }
        }

        public string ClientSecret
        {
            get { return loginProvider.ClientSecret; }
        }

        public string RedirectUrl
        {
            get { return loginProvider.RedirectUri; }
        }

        public string RefreshUrl
        {
            get { return loginProvider.AccessTokenUrl; }
        }

        public BaseOAuth2Authorization(ILog log)
        {
            if (null == log)
                throw new ArgumentNullException("log");

            this.log = log;
            loginProvider = ConsumerFactory.Get<T>();

            try
            {
                if (String.IsNullOrEmpty(loginProvider.ClientID))
                    throw new ArgumentNullException("ClientId");

                if (String.IsNullOrEmpty(loginProvider.ClientSecret))
                    throw new ArgumentNullException("ClientSecret");

                if (String.IsNullOrEmpty(loginProvider.RedirectUri))
                    throw new ArgumentNullException("RedirectUrl");
        }
            catch (Exception ex)
            {
                log.ErrorFormat("GoogleOAuth2Authorization() Exception:\r\n{0}\r\n", ex.ToString());
            }
        }

        public OAuth20Token RequestAccessToken(string refreshToken)
        {
            var token = new OAuth20Token
                {
                    ClientID = ClientId,
                    ClientSecret = ClientSecret,
                    RedirectUri = RedirectUrl,
                    RefreshToken = refreshToken,
                };

            try
            {
                return OAuth20TokenHelper.RefreshToken<T>(token);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("RequestAccessToken() Exception:\r\n{0}\r\n", ex.ToString());
                return null;
            }
        }
    }
}