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
using System.Configuration;
using System.Web.Configuration;
using ASC.Thrdparty.Configuration;

namespace ASC.Mail.Aggregator.Common.Authorization
{
    public class GoogleOAuth2Authorization : BaseOAuth2Authorization
    {
        public GoogleOAuth2Authorization(ILogger log) : base(log)
        {
            Func<string, string> getConfigVal = value =>
                                                (ConfigurationManager.AppSettings.Get(value) ??
                                                 WebConfigurationManager.AppSettings.Get(value) ?? 
                                                 KeyStorage.Get(value));

            try
            {
                ClientId = getConfigVal("googleClientId");
                ClientSecret = getConfigVal("googleClientSecret");

                if (String.IsNullOrEmpty(ClientId)) throw new ArgumentNullException("ClientId");
                if (String.IsNullOrEmpty(ClientSecret)) throw new ArgumentNullException("ClientSecret");
            }
            catch (Exception ex)
            {
                log.Error("GoogleOAuth2Authorization() Exception:\r\n{0}\r\n", ex.ToString());
            }

            RedirectUrl = "urn:ietf:wg:oauth:2.0:oob";
 
            ServerDescription = new AuthorizationServerDescription
            {
                AuthorizationEndpoint = new Uri("https://accounts.google.com/o/oauth2/auth?access_type=offline"),
                TokenEndpoint = new Uri("https://www.googleapis.com/oauth2/v3/token"),
                ProtocolVersion = ProtocolVersion.V20,
            };

            Scope = new List<string>
            {
                "https://mail.google.com/"
            };
        }
    }
}
