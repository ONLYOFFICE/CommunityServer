/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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


using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using ASC.Security.Cryptography;
using ASC.Web.Core.Utility;
using ASC.Web.Studio.Masters.MasterManagement;
using ASC.Web.Studio.PublicResources;
using ASC.Web.Studio.UserControls.DeepLink;

using Newtonsoft.Json;

namespace ASC.Web.Studio.UserControls
{
    public partial class DeepLinking : UserControl
    {

        private const string withoutDeeplinkRedirect = "without_redirect";

        protected string FileTitle { get; set; }
        protected string FileName { get; set; }
        protected string FileExtension { get; set; }
        protected string ErrorMsg {  get; set; }

        protected string DeepLinkUrl { get; set; }

        public static string WithoutDeeplinkRedirect
        {
            get { return withoutDeeplinkRedirect; }
        }

        public static string Location
        {
            get { return "~/UserControls/DeepLink/DeepLinking.ascx"; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var deepLink = ConfigurationManagerExtension.AppSettings["deeplink.documents.url"];

            var userAgent = Request.UserAgent.ToString().ToLower();

            var fileDataBase64 = !string.IsNullOrEmpty(Request.QueryString["data"]) ? Request.QueryString["data"] : "";

            var storeLink = "";

            var originalUrl = "";

            if (deepLink == null || fileDataBase64 == "")
            {
                Response.Redirect("/");
            }

            try
            {
                var fileDataString = Encoding.UTF8.GetString(Convert.FromBase64String(fileDataBase64));
                var decryptedData = InstanceCrypto.Decrypt(fileDataString);
                var base64DeeplinkData = Convert.ToBase64String(Encoding.UTF8.GetBytes(decryptedData));
                var fileData = JsonConvert.DeserializeObject<DeepLinkData>(decryptedData);

                if (string.IsNullOrEmpty(fileData.ErrorMsg))
                {
                    FileTitle = fileData.File.Title;
                    FileName = Path.GetFileNameWithoutExtension(FileTitle);
                    FileExtension = fileData.File.Extension;

                    originalUrl = fileData.OriginalUrl + string.Format("&{0}=true", withoutDeeplinkRedirect);
                }
                else
                {
                    ErrorMsg = fileData.ErrorMsg;
                }

                if (userAgent.Contains("android"))
                {
                    if (userAgent.Contains("chrome"))
                    {
                        var uriDeepLink = new Uri(deepLink);
                        var scheme = uriDeepLink.Scheme;
                        var path = uriDeepLink.Host + "?data=" + base64DeeplinkData;
                        DeepLinkUrl = "intent://" + path + "#Intent;scheme=" + scheme + ";package=" + ConfigurationManagerExtension.AppSettings["deeplink.documents.androidpackagename"] + ";end;";
                        storeLink = "https://play.google.com/store/apps/details?id=" + ConfigurationManagerExtension.AppSettings["deeplink.documents.androidpackagename"];
                    }
                    else
                    {
                        DeepLinkUrl = ConfigurationManagerExtension.AppSettings["deeplink.documents.url"] + "?data=" + base64DeeplinkData;
                        storeLink = "https://play.google.com/store/apps/details?id=" + ConfigurationManagerExtension.AppSettings["deeplink.documents.androidpackagename"];
                    }
                }
                else if (userAgent.Contains("iphone;") || userAgent.Contains("ipad;"))
                {
                    DeepLinkUrl = ConfigurationManagerExtension.AppSettings["deeplink.documents.url"] + "?data=" + base64DeeplinkData;
                    storeLink = "https://apps.apple.com/app/id" + ConfigurationManagerExtension.AppSettings["deeplink.documents.iospackageid"];
                }
                else
                {
                    Response.Redirect("/");
                }
            }
            catch (Exception)
            {
                Response.Redirect("/");
            }

            var cookie = "";

            if (HttpContext.Current != null)
            {
                var cookieName = "deeplink";

                if (HttpContext.Current.Request.Cookies[cookieName] != null)
                    cookie = HttpContext.Current.Request.Cookies[cookieName].Value ?? "";
            }

            if (cookie == "app" && string.IsNullOrEmpty(ErrorMsg))
            {
                Response.Redirect(DeepLinkUrl, true);
            }

            Page.Title = Resource.MainPageTitle;

            string mName = "viewport";

            HtmlHead pHtml = Page.Header;

            foreach (HtmlMeta metaTag in pHtml.Controls.OfType<HtmlMeta>())
            {
                if (metaTag.Name.Equals(mName, StringComparison.CurrentCultureIgnoreCase))
                {
                    metaTag.Content = "width=device-width, initial-scale=1, maximum-scale=1, minimum-scale=1, user-scalable=no";
                    break;
                }
            }
            if(ModeThemeSettings.GetModeThemesSettings().ModeThemeName == ModeTheme.dark)
            {
                Page.RegisterStyle("~/UserControls/DeepLink/css/dark-deeplinking.less");
            }
            else
            {
                Page.RegisterStyle("~/UserControls/DeepLink/css/deeplinking.less");
            }
            Page.RegisterBodyScripts("~/UserControls/DeepLink/js/deeplinking.js")
               .RegisterInlineScript(@"ASC.DeepLinking.init( '" + FileTitle + "','" + storeLink + "','" + originalUrl + "');");

        }

    }
}