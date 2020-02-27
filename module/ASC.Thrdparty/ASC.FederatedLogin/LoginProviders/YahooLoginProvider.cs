/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using ASC.FederatedLogin.Profile;

namespace ASC.FederatedLogin.LoginProviders
{
    public class YahooLoginProvider : BaseLoginProvider<YahooLoginProvider>
    {
        public const string YahooUrlUserGuid = "https://social.yahooapis.com/v1/me/guid";
        public const string YahooUrlContactsFormat = "https://social.yahooapis.com/v1/user/{0}/contacts";

        public override string CodeUrl { get { return "https://api.login.yahoo.com/oauth2/request_auth"; } }
        public override string AccessTokenUrl { get { return "https://api.login.yahoo.com/oauth2/get_token"; } }
        public override string RedirectUri { get { return this["yahooRedirectUrl"]; } }
        public override string ClientID { get { return this["yahooClientId"]; } }
        public override string ClientSecret { get { return this["yahooClientSecret"]; } }
        public override string Scopes { get { return "sdct-r"; } }

        public YahooLoginProvider() { }
        public YahooLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null) : base(name, order, props, additional) { }

        public OAuth20Token Auth(HttpContext context)
        {
            return Auth(context, Scopes);
        }

        public override LoginProfile GetLoginProfile(string accessToken)
        {
            throw new NotImplementedException();
        }
    }
}
