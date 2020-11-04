/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/


using ASC.FederatedLogin.Helpers;
using ASC.FederatedLogin.LoginProviders;
using ASC.FederatedLogin.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Web.Services;
using System.Web.SessionState;

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
                _params = ((Dictionary<string, object>) new JavaScriptSerializer().DeserializeObject(
                    Encoding.UTF8.GetString(HttpServerUtility.UrlTokenDecode(context.Request["p"])))).ToDictionary(x => x.Key, y => (string) y.Value);
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
            get { return _params.Get("callback") ?? "loginCallback"; }
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
                    return (LoginMode) Enum.Parse(typeof (LoginMode), _params.Get("mode"), true);
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
            switch (Mode)
            {
                case LoginMode.Redirect:
                    RedirectToReturnUrl(context, profile);
                    break;
                case LoginMode.Popup:
                    SendJsCallback(context, profile);
                    break;
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
                profile = profile.GetMinimalProfile(); //Only id and provider

            if (context.Session != null && !useMinimalProfile)
            {
                //Store in session
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfileSession(profile, context).ToString(), true);
            }
            else if (HttpRuntime.Cache != null && !useMinimalProfile)
            {
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfileCache(profile).ToString(), true);
            }
            else
            {
                context.Response.Redirect(new Uri(ReturnUrl, UriKind.Absolute).AddProfile(profile).ToString(), true);
            }
        }
    }
}