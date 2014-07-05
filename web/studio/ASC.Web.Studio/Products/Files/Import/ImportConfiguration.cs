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
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using ASC.Thrdparty;
using ASC.Thrdparty.Configuration;
using ASC.Thrdparty.TokenManagers;

namespace ASC.Web.Files.Import
{
    public static class ImportConfiguration
    {
        private static IEnumerable<string> ImportProviders
        {
            get { return (WebConfigurationManager.AppSettings["files.import.enable"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        private static IEnumerable<string> ThirdPartyProviders
        {
            get { return (WebConfigurationManager.AppSettings["files.thirdparty.enable"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public static bool SupportImport
        {
            get { return SupportBoxNetImport || SupportGoogleImport || SupportZohoImport; }
        }

        public static bool SupportBoxNetImport
        {
            get { return ImportProviders.Contains("boxnet") && !string.IsNullOrEmpty(BoxNetApiKey); }
        }

        public static bool SupportGoogleImport
        {
            get { return ImportProviders.Contains("google") && !string.IsNullOrEmpty(KeyStorage.Get("googleConsumerKey")); }
        }

        public static bool SupportZohoImport
        {
            get { return ImportProviders.Contains("zoho") && !string.IsNullOrEmpty(ZohoApiKey); }
        }

        public static bool SupportInclusion
        {
            get { return SupportBoxNetInclusion || SupportDropboxInclusion || SupportGoogleInclusion || SupportGoogleDriveInclusion || SupportSkyDriveInclusion || SupportSharePointInclusion; }
        }

        public static bool SupportBoxNetInclusion
        {
            get { return ThirdPartyProviders.Contains("boxnet") && !string.IsNullOrEmpty(BoxNetApiKey); }
        }

        public static bool SupportDropboxInclusion
        {
            get { return ThirdPartyProviders.Contains("dropbox") && !string.IsNullOrEmpty(DropboxAppKey) && !string.IsNullOrEmpty(DropboxAppSecret); }
        }

        public static bool SupportGoogleInclusion
        {
            get { return ThirdPartyProviders.Contains("google") && !string.IsNullOrEmpty(KeyStorage.Get("googleConsumerKey")); }
        }

        public static bool SupportSkyDriveInclusion
        {
            get { return ThirdPartyProviders.Contains("skydrive") && !string.IsNullOrEmpty(SkyDriveAppKey) && !string.IsNullOrEmpty(SkyDriveAppSecret); }
        }

        public static bool SupportSharePointInclusion
        {
            get { return ThirdPartyProviders.Contains("sharepoint"); }
        }

        public static string BoxNetApiKey
        {
            get { return KeyStorage.Get("box.net"); }
        }

        public static string BoxNetIFrameAddress
        {
            get { return KeyStorage.Get("box.net.framehandler"); }
        }

        public static IAssociatedTokenManager GoogleTokenManager
        {
            get { return TokenManagerHolder.Get("google", "googleConsumerKey", "googleConsumerSecret"); }
        }

        public static string ZohoApiKey
        {
            get { return KeyStorage.Get("zoho"); }
        }

        public static string DropboxAppKey
        {
            get { return KeyStorage.Get("dropboxappkey"); }
        }

        public static string DropboxAppSecret
        {
            get { return KeyStorage.Get("dropboxappsecret"); }
        }

        public static string SkyDriveAppKey
        {
            get { return KeyStorage.Get("skydriveappkey"); }
        }

        public static string SkyDriveAppSecret
        {
            get { return KeyStorage.Get("skydriveappsecret"); }
        }


        public static bool SupportGoogleDriveInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("googledrive") &&
                       !(string.IsNullOrEmpty(GoogleOAuth20ClientId) || string.IsNullOrEmpty(GoogleOAuth20ClientSecret) || string.IsNullOrEmpty(GoogleOAuth20RedirectUrl));
            }
        }

        public static string GoogleOAuth20Url
        {
            get
            {
                return SupportGoogleDriveInclusion
                           ? string.Format("https://accounts.google.com/o/oauth2/auth?response_type=code&client_id={0}&redirect_uri={1}&scope={2}&state=%2Fprofile&approval_prompt=force",
                                           HttpUtility.UrlEncode(GoogleOAuth20ClientId),
                                           GoogleOAuth20RedirectUrl,
                                           "https://www.googleapis.com/auth/drive")
                           : string.Empty;
            }
        }

        public static string GoogleOAuth20ClientId
        {
            get { return KeyStorage.Get("googleClientId"); }
        }

        public static string GoogleOAuth20ClientSecret
        {
            get { return KeyStorage.Get("googleClientSecret"); }
        }

        public static string GoogleOAuth20RedirectUrl
        {
            get { return KeyStorage.Get("googleRedirectUrl"); }
        }
    }
}