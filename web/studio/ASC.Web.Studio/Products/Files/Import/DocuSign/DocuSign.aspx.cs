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
using System.Security;
using System.Threading;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.FederatedLogin;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.Web.Core.Files;
using ASC.Web.Files.Classes;
using ASC.Web.Files.Resources;
using SecurityContext = ASC.Core.SecurityContext;

namespace ASC.Web.Files.Import.DocuSign
{
    public partial class DocuSign : OAuthBase
    {
        public static string Location
        {
            get { return FilesLinkUtility.FilesBaseAbsolutePath + "import/docusign/docusign.aspx"; }
        }

        private const string Source = "docusign";

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID).IsVisitor()
                    || !FilesSettings.EnableThirdParty || !ImportConfiguration.SupportDocuSignInclusion) throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException_Create);

                var error = Request["error"];
                if (!string.IsNullOrEmpty(error))
                {
                    if (error == "access_denied")
                    {
                        error = "Canceled at provider";
                    }
                    throw new Exception(error);
                }

                OAuth20Token token;

                var code = Request["code"];
                if (!string.IsNullOrEmpty(code))
                {
                    token = DocuSignLoginProvider.GetAccessToken(code);
                    DocuSignHelper.ValidateToken(token);
                    DocuSignToken.SaveToken(token);
                }
                else
                {
                    token = DocuSignToken.GetToken();
                    if (token == null)
                    {
                        OAuth20TokenHelper.RequestCode(HttpContext.Current,
                                                       DocuSignLoginProvider.DocuSignOauthCodeUrl,
                                                       DocuSignLoginProvider.DocuSignOAuth20ClientId,
                                                       DocuSignLoginProvider.DocuSignOAuth20RedirectUrl,
                                                       DocuSignLoginProvider.DocuSignScope);
                        return;
                    }
                }

                SubmitToken("", Source);
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                Global.Logger.Error("DocuSign", ex);
                SubmitError(ex.Message, Source);
            }
        }
    }
}