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