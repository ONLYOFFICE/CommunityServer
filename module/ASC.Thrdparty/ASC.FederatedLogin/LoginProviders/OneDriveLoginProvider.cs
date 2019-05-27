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


using System.Collections.Generic;
using ASC.Core.Common.Configuration;

namespace ASC.FederatedLogin.LoginProviders
{
    public class OneDriveLoginProvider : Consumer, IOAuthProvider
    {
        private const string OneDriveOauthUrl = "https://login.live.com/";
        public const string OneDriveApiUrl = "https://api.onedrive.com";

        public static OneDriveLoginProvider Instance
        {
            get { return ConsumerFactory.Get<OneDriveLoginProvider>(); }
        }

        public string Scopes { get { return "wl.signin wl.skydrive_update wl.offline_access"; } }
        public string CodeUrl { get { return OneDriveOauthUrl + "oauth20_authorize.srf"; } }
        public string AccessTokenUrl { get { return OneDriveOauthUrl + "oauth20_token.srf"; } }
        public string RedirectUri { get { return this["skydriveRedirectUrl"]; } }
        public string ClientID { get { return this["skydriveappkey"]; } }
        public string ClientSecret { get { return this["skydriveappsecret"]; } }

        public bool IsEnabled
        {
            get
            {
                return !string.IsNullOrEmpty(ClientID) &&
                       !string.IsNullOrEmpty(ClientSecret) &&
                       !string.IsNullOrEmpty(RedirectUri);
            }
        }

        public OneDriveLoginProvider() { }

        public OneDriveLoginProvider(string name, int order, Dictionary<string, string> props, Dictionary<string, string> additional = null)
            : base(name, order, props, additional)
        {
        }
    }
}