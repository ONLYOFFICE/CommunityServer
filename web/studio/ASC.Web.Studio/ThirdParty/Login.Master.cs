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


using System;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.ThirdParty
{
    public partial class Login : MasterPage
    {
        private const string CallbackSuccessJavascript =
            "function snd(){{try{{window.opener.OAuthCallback(\"{0}\");}}catch(err){{alert(err);}}window.close();}} window.onload = snd;";

        private const string CallbackErrorJavascript =
            "function snd(){{try{{window.opener.OAuthError(\"{0}\");}}catch(err){{alert(err);}}window.close();}} window.onload = snd;";

        protected override void OnInit(EventArgs e)
        {
            HttpContext.Current.PushRewritenUri();
            base.OnInit(e);
        }

        public void SubmitCode(string code)
        {
            var urlRedirect = Request["redirect"];
            if (!string.IsNullOrEmpty(urlRedirect))
            {
                Response.Redirect(AppendCode(urlRedirect, code), true);
            }

            Page.ClientScript.RegisterClientScriptBlock(GetType(), "posttoparent",
                                                        String.Format(CallbackSuccessJavascript, code.Replace("\"", "\\\"")), true);
        }

        public void SubmitError(string error)
        {
            var urlRedirect = Request["redirect"];
            if (!string.IsNullOrEmpty(urlRedirect))
            {
                Response.Redirect(AppendCode(urlRedirect, null, error), true);
            }

            Page.ClientScript.RegisterClientScriptBlock(GetType(), "posterrortoparent",
                                                        String.Format(CallbackErrorJavascript, error.Replace("\"", "\\\"")), true);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private static string AppendCode(string url, string code = null, string error = null)
        {
            url += (url.Contains("#") ? "&" : "#")
                   + (string.IsNullOrEmpty(error)
                          ? (string.IsNullOrEmpty(code)
                                 ? string.Empty
                                 : "code=" + HttpUtility.UrlEncode(code))
                          : ("error/" + HttpUtility.UrlEncode(error)));

            return url;
        }
    }
}