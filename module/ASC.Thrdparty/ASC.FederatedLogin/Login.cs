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
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services;
using System.Web.SessionState;
using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.FederatedLogin.Profile;
using DotNetOpenAuth.OpenId.RelyingParty;

namespace ASC.FederatedLogin
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class Login : IHttpHandler, IRequiresSessionState
    {

        private Dictionary<string, string> _params;

        public void ProcessRequest(HttpContext context)
        {
            context.PushRewritenUri();

            if (string.IsNullOrEmpty(context.Request["p"]))
            {
                _params = new Dictionary<string, string>(context.Request.QueryString.Count);
                //Form params and redirect
                foreach (var key in context.Request.QueryString.AllKeys)
                {
                    _params.Add(key, context.Request.QueryString[key]);
                }

                //Pack and redirect
                var uriBuilder = new UriBuilder(context.Request.GetUrlRewriter());
                var token = HttpServerUtility.UrlTokenEncode(Encoding.UTF8.GetBytes(new JavaScriptSerializer().Serialize(_params)));
                uriBuilder.Query = "p=" + token;
                context.Response.Redirect(uriBuilder.Uri.ToString(), true);
            }
            else
            {
                _params = ((Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(
                    Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(context.Request["p"])))).ToDictionary(x => x.Key, y => (string)y.Value);

            }


            if (!string.IsNullOrEmpty(Auth))
            {
                try
                {
                    var profile = ProviderManager.Process(Auth, context, _params);
                    if (profile != null)
                    {
                        SendClientData(context, profile);
                    }
                }
                catch (ThreadAbortException)
                {
                    //Thats is responce ending
                }
                catch (Exception ex)
                {
                    SendClientData(context, LoginProfile.FromError(ex));
                }
            }
            else
            {
                //Render xrds
                RenderXrds(context);
            }
            context.PopRewritenUri();
        }

        protected bool Minimal
        {
            get
            {
                if (_params.ContainsKey("min"))
                {
                    bool result;
                    bool.TryParse(_params.Get("min"), out result);
                    return result;
                }
                return false;
            }
        }

        protected string Callback
        {
            get
            {
                return _params.Get("callback") ?? "loginCallback";
            }
        }

        private void RenderXrds(HttpContext context)
        {
            var xrdsloginuri = new Uri(context.Request.GetUrlRewriter(),
                                       new Uri(context.Request.GetUrlRewriter().AbsolutePath, UriKind.Relative)) + "?auth=openid&returnurl=" + ReturnUrl;
            var xrdsimageuri = new Uri(context.Request.GetUrlRewriter(),
                                       new Uri(context.Request.ApplicationPath, UriKind.Relative)) + "openid.gif";
            XrdsHelper.RenderXrds(context.Response, xrdsloginuri, xrdsimageuri);
        }

        protected LoginMode Mode
        {
            get
            {
                if (!string.IsNullOrEmpty(_params.Get("mode")))
                {
                    return (LoginMode)Enum.Parse(typeof(LoginMode), _params.Get("mode"), true);
                }
                return LoginMode.Popup;
            }
        }

        protected string ReturnUrl
        {
            get { return _params.Get("returnurl") ?? FormsAuthentication.LoginUrl; }
        }

        protected string Auth
        {
            get { return _params.Get("auth"); }
        }

        public bool IsReusable
        {
            get { return false; }
        }


        private void SendClientData(HttpContext context, LoginProfile profile)
        {
            if (Mode == LoginMode.Redirect)
            {
                RedirectToReturnUrl(context, profile);
            }
            else if (Mode == LoginMode.Popup)
            {
                SendJsCallback(context, profile);
            }
        }

        private void SendJsCallback(HttpContext context, LoginProfile profile)
        {
            //Render a page
            context.Response.ContentType = "text/html";
            context.Response.Write(JsCallbackHelper.GetCallbackPage().Replace("%PROFILE%", profile.ToJson()).Replace("%CALLBACK%", Callback));
        }

        private void RedirectToReturnUrl(HttpContext context, LoginProfile profile)
        {
            var useMinimalProfile = Minimal;
            if (useMinimalProfile)
                profile = profile.GetMinimalProfile();//Only id and provider


            if (context.Session != null && !useMinimalProfile)
            {
                //Store in session
                context.Response.Redirect(
                    new Uri(ReturnUrl, UriKind.Absolute).AddProfileSession(profile, context).ToString(), true);
            }
            else if (HttpRuntime.Cache != null && !useMinimalProfile)
            {
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfileCache(profile).ToString(),
                                          true);
            }
            else
            {
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfile(profile).ToString(), true);
            }
        }
    }
}
