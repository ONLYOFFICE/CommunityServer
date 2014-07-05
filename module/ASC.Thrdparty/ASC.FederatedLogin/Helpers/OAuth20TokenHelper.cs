/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;

namespace ASC.FederatedLogin.Helpers
{
    public static class OAuth20TokenHelper
    {
        public static OAuth20Token GetAccessToken(string requestUrl, string clientID, string clientSecret, string redirectUri, string authCode)
        {
            if (String.IsNullOrEmpty(clientID)) throw new ArgumentNullException("clientID");
            if (String.IsNullOrEmpty(clientSecret)) throw new ArgumentNullException("clientSecret");
            if (String.IsNullOrEmpty(redirectUri)) throw new ArgumentNullException("redirectUri");
            if (String.IsNullOrEmpty(authCode)) throw new ArgumentNullException("authCode");

            var data = string.Format("code={0}&client_id={1}&client_secret={2}&redirect_uri={3}&grant_type=authorization_code",
                                     authCode, clientID, clientSecret, redirectUri);

            var json = RequestHelper.PerformRequest(requestUrl, "application/x-www-form-urlencoded", "POST", data);
            if (json != null)
            {
                var token = OAuth20Token.FromJson(json);
                if (token == null) return null;

                token.ClientID = clientID;
                token.ClientSecret = clientSecret;
                token.RedirectUri = redirectUri;
                return token;
            }

            return null;
        }

        public static OAuth20Token RefreshToken(string requestUrl, OAuth20Token token)
        {
            if (token == null || !CanRefresh(token)) throw new ArgumentException("Can not refresh given token", "token");

            var data = String.Format("client_id={0}&client_secret={1}&redirect_uri={2}&grant_type=refresh_token&refresh_token={3}",
                                     token.ClientID, token.ClientSecret, token.RedirectUri, token.RefreshToken);

            var json = RequestHelper.PerformRequest(requestUrl, "application/x-www-form-urlencoded", "POST", data);
            if (json != null)
            {
                var refreshed = OAuth20Token.FromJson(json);
                refreshed.ClientID = token.ClientID;
                refreshed.ClientSecret = token.ClientSecret;
                refreshed.RedirectUri = token.RedirectUri;
                return refreshed;
            }

            return token;
        }

        private static bool CanRefresh(OAuth20Token token)
        {
            return !String.IsNullOrEmpty(token.ClientID) && !String.IsNullOrEmpty(token.ClientSecret) && !String.IsNullOrEmpty(token.RedirectUri);
        }
    }
}
