/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using System;
using System.Collections.Generic;
using System.Web;
using ASC.Web.Core.Files;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;

namespace ASC.Web.Files.Import.DropBox
{
    public partial class Dropbox : OAuthBase
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "import/dropbox/dropbox.aspx"; }
        }

        private const string Source = "dropbox";

        private const string RequestTokenSessionKey = "requestToken";
        private const string AuthorizationUrlKey = "authorization";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session.IsReadOnly)
            {
                SubmitError("No session is availible.", Source);
                return;
            }

            var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox) as DropBoxConfiguration;
            var callbackUri = new UriBuilder(Request.GetUrlRewriter());
            if (!string.IsNullOrEmpty(Request.QueryString[AuthorizationUrlKey]) && Session[RequestTokenSessionKey] != null)
            {
                //Authorization callback
                try
                {
                    var accessToken = DropBoxStorageProviderTools.ExchangeDropBoxRequestTokenIntoAccessToken(config,
                                                                                                             ImportConfiguration.DropboxAppKey,
                                                                                                             ImportConfiguration.DropboxAppSecret,
                                                                                                             Session[RequestTokenSessionKey] as DropBoxRequestToken);

                    Session[RequestTokenSessionKey] = null; //Exchanged
                    var storage = new CloudStorage();
                    var base64token = storage.SerializeSecurityTokenToBase64Ex(accessToken, config.GetType(), new Dictionary<string, string>());
                    storage.Open(config, accessToken); //Try open storage!
                    var root = storage.GetRoot();
                    if (root == null) throw new Exception();

                    SubmitToken(base64token, Source);
                }
                catch
                {
                    SubmitError("Failed to open storage with token", Source);
                }
            }
            else
            {
                callbackUri.Query += string.Format("&{0}=1", AuthorizationUrlKey);
                config.AuthorizationCallBack = callbackUri.Uri;
                // create a request token
                var requestToken = DropBoxStorageProviderTools.GetDropBoxRequestToken(config, ImportConfiguration.DropboxAppKey,
                                                                                      ImportConfiguration.DropboxAppSecret);
                var authorizationUrl = DropBoxStorageProviderTools.GetDropBoxAuthorizationUrl(config, requestToken);
                Session[RequestTokenSessionKey] = requestToken; //Store token into session!!!
                Response.Redirect(authorizationUrl);
            }
        }
    }
}