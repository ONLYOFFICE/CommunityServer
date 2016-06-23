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


using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;
using ASC.Web.Core.Files;
using System;
using System.Web;

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
        private const string AuthorizationNotApproved = "not_approved";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session.IsReadOnly)
            {
                SubmitError("No session is availible.", Source);
                return;
            }

            if (Boolean.TrueString.Equals(Request.QueryString[AuthorizationNotApproved] ?? "false", StringComparison.InvariantCultureIgnoreCase))
            {
                SubmitError("Canceled at provider", Source);
                return;
            }

            if (!string.IsNullOrEmpty(Request.QueryString[AuthorizationUrlKey]) && Session[RequestTokenSessionKey] != null)
            {
                //Authorization callback
                try
                {
                    var dropboxToken = Session[RequestTokenSessionKey] as DropBoxRequestToken;
                    SubmitToken(dropboxToken.ToString(), Source);
                }
                catch
                {
                    SubmitError("Failed to open storage with token", Source);
                }
            }
            else
            {
                var callbackUri = new UriBuilder(Request.GetUrlRewriter());
                callbackUri.Query += string.Format("&{0}=1", AuthorizationUrlKey);

                var config = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox) as DropBoxConfiguration;
                config.AuthorizationCallBack = callbackUri.Uri;

                // create a request token
                var requestToken = DropBoxStorageProviderTools.GetDropBoxRequestToken(config,
                                                                                      ImportConfiguration.DropboxAppKey,
                                                                                      ImportConfiguration.DropboxAppSecret);
                if (requestToken == null)
                {
                    SubmitError("Failed to open storage with this credentials", Source);
                    return;
                }

                Session[RequestTokenSessionKey] = requestToken; //Store token into session!!!

                var authorizationUrl = DropBoxStorageProviderTools.GetDropBoxAuthorizationUrl(config, requestToken);
                Response.Redirect(authorizationUrl);
            }
        }
    }
}