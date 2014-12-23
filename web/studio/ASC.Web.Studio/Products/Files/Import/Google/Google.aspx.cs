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
using System.Threading;
using System.Web;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core.Files;

namespace ASC.Web.Files.Import.Google
{
    public partial class Google : OAuthBase
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "import/google/google.aspx"; }
        }

        private const string Source = "googledrive";
        private const string GoogleDriveScope = "https://www.googleapis.com/auth/drive";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                var error = Request["error"];
                if (!string.IsNullOrEmpty(error))
                {
                    if (error == "access_denied")
                    {
                        error = "Canceled at provider";
                    }
                    throw new Exception(error);
                }

                var code = Request["code"];
                if (string.IsNullOrEmpty(code))
                {
                    OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                                   GoogleLoginProvider.GoogleOauthCodeUrl,
                                                   GoogleLoginProvider.GoogleOAuth20ClientId,
                                                   GoogleLoginProvider.GoogleOAuth20RedirectUrl,
                                                   GoogleDriveScope,
                                                   new Dictionary<string, string>
                                                   {
                                                       { "access_type", "offline" },
                                                       { "approval_prompt", "force" }
                                                   });
                }
                else
                {
                    SubmitToken(code, Source);
                }
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                SubmitError(ex.Message, Source);
            }
        }
    }
}