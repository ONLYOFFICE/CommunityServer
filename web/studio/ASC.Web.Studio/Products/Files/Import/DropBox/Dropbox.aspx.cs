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
                if (requestToken == null)
                {
                    SubmitError("Failed to open storage with this credentials", Source);
                    return;
                }

                var authorizationUrl = DropBoxStorageProviderTools.GetDropBoxAuthorizationUrl(config, requestToken);
                Session[RequestTokenSessionKey] = requestToken; //Store token into session!!!
                Response.Redirect(authorizationUrl);
            }
        }
    }
}