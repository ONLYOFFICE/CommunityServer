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


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Client;

namespace ASC.Web.Studio.HttpHandlers
{
    public class TemplatingHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var path = context.Request["id"];
            var name = context.Request["name"];
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(name))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            var key = string.Join("_", new[] { path, name, Thread.CurrentThread.CurrentCulture.Name, ClientSettings.ResetCacheKey });
            var hashToken = HttpServerUtility.UrlTokenEncode(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(key)));

            if (hashToken.Equals(context.Request.Headers["If-None-Match"]))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotModified;
            }
            else
            {
                var template = RenderDocument(context, path, name);
                context.Response.ContentType = "text/xml";
                context.Response.Write(template.ToString());
            }

            // set cache
            context.Response.Cache.SetVaryByCustom("*");
            context.Response.Cache.SetAllowResponseInBrowserHistory(true);
            context.Response.Cache.SetETag(hashToken);
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
        }

        private static XDocument RenderDocument(HttpContext context, string templatePath, string templateName)
        {
            try
            {
                var path = string.Format("~{0}{1}{2}", templatePath, System.IO.Path.GetFileNameWithoutExtension(templateName), ".xsl");
                var template = XDocument.Load(context.Server.MapPath(path));
                var ns = template.Root.Name.Namespace;

                if (!template.Elements(ns + "stylesheet").Any())
                {
                    return template;
                }

                var includes = template.Root.Elements(ns + "include").ToList();
                foreach (var include in includes)
                {
                    try
                    {
                        var xml = RenderDocument(context, templatePath, include.Attribute("href").Value);
                        include.ReplaceWith(xml.Root.Elements(ns + "template"));
                    }
                    catch (Exception e)
                    {
                        include.ReplaceWith(new XComment(e.Message));
                    }
                }

                var aliases = new Dictionary<string, string>();
                var registers = template.Root.Elements("register").ToList();
                foreach (var register in registers)
                {
                    aliases[register.Attribute("alias").Value] = register.Attribute("type").Value;
                    register.Remove();
                }

                var resources = template.Descendants("resource").ToList();
                foreach (var resource in resources)
                {
                    var typeName = resource.Attribute("name").Value.Split('.');
                    if (typeName.Length == 2 && aliases.ContainsKey(typeName[0]))
                    {
                        var value = GetModuleResource(aliases[typeName[0]], typeName[1]);
                        resource.ReplaceWith(new XText(value));
                    }
                }

                return template;
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Web.Template").Error(err);
                throw;
            }
        }

        private static String GetModuleResource(string typeName, string key)
        {
            try
            {
                var type = Type.GetType(typeName, true);
                var manager = (ResourceManager)type.InvokeMember(
                    "resourceMan",
                    BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public, null, type, null);

                //custom
                if (!SecurityContext.IsAuthenticated)
                {
                    SecurityContext.AuthenticateMe(CookiesManager.GetCookies(CookiesType.AuthKey));
                }

                var u = CoreContext.UserManager.GetUsers(SecurityContext.CurrentAccount.ID);
                var culture = !string.IsNullOrEmpty(u.CultureName) ? CultureInfo.GetCultureInfo(u.CultureName) : CoreContext.TenantManager.GetCurrentTenant().GetCulture();
                return manager.GetString(key, culture);
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Web.Template").Error(err);
                return string.Empty;
            }
        }
    }
}