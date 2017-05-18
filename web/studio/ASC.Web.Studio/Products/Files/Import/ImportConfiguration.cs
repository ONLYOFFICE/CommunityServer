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
using System.Linq;
using System.Web.Configuration;
using ASC.FederatedLogin.LoginProviders;
using ASC.Thrdparty.Configuration;

namespace ASC.Web.Files.Import
{
    public static class ImportConfiguration
    {
        private static IEnumerable<string> ThirdPartyProviders
        {
            get { return (WebConfigurationManager.AppSettings["files.thirdparty.enable"] ?? "").Split(new char[] { '|', ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public static bool SupportInclusion
        {
            get { return SupportBoxInclusion || SupportDropboxInclusion || SupportDocuSignInclusion || SupportGoogleDriveInclusion || SupportOneDriveInclusion || SupportSharePointInclusion || SupportWebDavInclusion; }
        }

        public static bool SupportBoxInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("box") &&
                       !(string.IsNullOrEmpty(BoxLoginProvider.BoxOAuth20ClientId) || string.IsNullOrEmpty(BoxLoginProvider.BoxOAuth20ClientSecret) || string.IsNullOrEmpty(BoxLoginProvider.BoxOAuth20RedirectUrl));
            }
        }

        public static bool SupportDropboxInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("dropboxv2") &&
                       !(string.IsNullOrEmpty(DropboxLoginProvider.DropboxOAuth20ClientId) || string.IsNullOrEmpty(DropboxLoginProvider.DropboxOAuth20ClientSecret) || string.IsNullOrEmpty(DropboxLoginProvider.DropboxOAuth20RedirectUrl));
            }
        }

        public static bool SupportOneDriveInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("skydrive") &&
                       !(string.IsNullOrEmpty(OneDriveLoginProvider.OneDriveOAuth20ClientId) || string.IsNullOrEmpty(OneDriveLoginProvider.OneDriveOAuth20ClientSecret) || string.IsNullOrEmpty(OneDriveLoginProvider.OneDriveOAuth20RedirectUrl));
            }
        }

        public static bool SupportSharePointInclusion
        {
            get { return ThirdPartyProviders.Contains("sharepoint"); }
        }

        public static bool SupportWebDavInclusion
        {
            get { return ThirdPartyProviders.Contains("webdav"); }
        }

        public static bool SupportYandexInclusion
        {
            get { return ThirdPartyProviders.Contains("yandex"); }
        }

        public static string DropboxAppKey
        {
            get { return KeyStorage.Get("dropboxappkey"); }
        }

        public static string DropboxAppSecret
        {
            get { return KeyStorage.Get("dropboxappsecret"); }
        }

        public static bool SupportDocuSignInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("docusign") &&
                       !(string.IsNullOrEmpty(DocuSignLoginProvider.DocuSignOAuth20ClientId) || string.IsNullOrEmpty(DocuSignLoginProvider.DocuSignOAuth20ClientSecret) || string.IsNullOrEmpty(DocuSignLoginProvider.DocuSignOAuth20RedirectUrl));
            }
        }

        public static bool SupportGoogleDriveInclusion
        {
            get
            {
                return ThirdPartyProviders.Contains("google") &&
                       !(string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientId) || string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20ClientSecret) || string.IsNullOrEmpty(GoogleLoginProvider.GoogleOAuth20RedirectUrl));
            }
        }
    }
}