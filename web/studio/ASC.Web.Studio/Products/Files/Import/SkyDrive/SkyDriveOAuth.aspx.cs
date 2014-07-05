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
using System.Web;
using ASC.Web.Core.Files;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.SkyDrive.Authorization;

namespace ASC.Web.Files.Import.SkyDrive
{
    public partial class SkyDriveOAuth : OAuthBase
    {
        public static string Location { get { return FilesLinkUtility.FilesBaseAbsolutePath + "import/skydrive/skydriveoauth.aspx"; } }

        private const string Source = "skydrive";

        private const string AuthorizationCodeUrlKey = "code";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session.IsReadOnly)
            {
                SubmitError("No session is availible.", Source);
                return;
            }

            var redirectUri = Request.GetUrlRewriter().GetLeftPart(UriPartial.Path);

            if (!string.IsNullOrEmpty(Request[AuthorizationCodeUrlKey]))
            {
                //we ready to obtain and store token
                var authCode = Request[AuthorizationCodeUrlKey];
                var accessToken = SkyDriveAuthorizationHelper.GetAccessToken(ImportConfiguration.SkyDriveAppKey,
                                                                             ImportConfiguration.SkyDriveAppSecret,
                                                                             redirectUri,
                                                                             authCode);
                
                //serialize token
                var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.SkyDrive);
                var storage = new CloudStorage();
                var base64AccessToken = storage.SerializeSecurityTokenToBase64Ex(accessToken, config.GetType(), new Dictionary<string, string>());

                //check and submit
                storage.Open(config, accessToken);
                var root = storage.GetRoot();
                if (root != null)
                {
                    SubmitToken(base64AccessToken, Source);
                }
                else
                {
                    SubmitError("Failed to open storage with token", Source);
                }
            }
            else
            {
                var authCodeUri = SkyDriveAuthorizationHelper.BuildAuthCodeUrl(ImportConfiguration.SkyDriveAppKey, null, redirectUri);
                Response.Redirect(authCodeUri);
            }
        }
    }
}
