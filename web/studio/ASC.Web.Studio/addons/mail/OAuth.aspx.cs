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


using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using ASC.Web.Core;

namespace ASC.Web.Mail
{
    public partial class OAuth : BasePage
    {
        public static string Location
        {
            get { return MailAddon.BaseVirtualPath + "oauth.aspx"; }
        }

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
                                                   string.Concat(GoogleLoginProvider.GoogleScopeMail, " ", GoogleLoginProvider.GoogleScopeProfile),
                                                   new Dictionary<string, string>
                                                   {
                                                       { "access_type", "offline" },
                                                       { "approval_prompt", "force" }
                                                   });
                }
                else
                {
                    SubmitData(code);
                }
            }
            catch (ThreadAbortException)
            {

            }
            catch (Exception ex)
            {
                SubmitData(null, ex.Message);
            }
        }

        private const string CallbackJavascript =
            @"window.onload = function(){{try {{window.opener.accountsModal.onGetOAuthInfo(""{0}"", {1});}}catch(ex){{}}window.close();}};";

        private void SubmitData(string token, string errorMessage = null)
        {
            ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                                                   string.Format(CallbackJavascript, token, EncodeJsString(errorMessage)),
                                                   true);
        }

        private static string EncodeJsString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "null";
            var sb = new StringBuilder();
            sb.Append("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        var i = (int)c;
                        if (i < 32 || i > 127)
                        {
                            sb.AppendFormat("\\u{0:X04}", i);
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }
            sb.Append("\"");

            return sb.ToString();
        }
    }
}