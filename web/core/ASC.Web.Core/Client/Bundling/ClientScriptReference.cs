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


using ASC.Common.Caching;
using ASC.Core;
using ASC.Web.Core.Client.HttpHandlers;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASC.Web.Core.Client.Bundling
{
    [ToolboxData("<{0}:ClientScriptReference runat=server></{0}:ClientScriptReference>")]
    public class ClientScriptReference : WebControl
    {
        private static readonly ICache cache = AscCache.Default;
        public virtual ICollection<Type> Includes { get; private set; }


        public ClientScriptReference()
        {
            Includes = new HashSet<Type>();
        }

        public override void RenderBeginTag(HtmlTextWriter writer) { }
        public override void RenderEndTag(HtmlTextWriter writer) { }

        protected override void RenderContents(HtmlTextWriter output)
        {
            var link = GetLink();
            output.Write(BundleHelper.HtmlScript(VirtualPathUtility.ToAbsolute(link), false, false));
        }

        public string GetLink()
        {
            var filename = string.Empty;
            foreach (var type in Includes)
            {
                filename += type.FullName.ToLowerInvariant();
            }
            var filenameHash = GetHash(filename) + "_" + CultureInfo.CurrentCulture.Name.ToLowerInvariant();

            var scripts = cache.Get<List<string>>(filenameHash);
            if (scripts == null)
            {

                scripts = Includes.Select(type => type.AssemblyQualifiedName).ToList();
                cache.Insert(filenameHash, scripts, DateTime.MaxValue);
            }

            return string.Format("~{0}{1}.js?ver={2}", BundleHelper.CLIENT_SCRIPT_VPATH, filenameHash, GetContentHash(filenameHash));
        }

        public static string GetContent(string uri)
        {
            var log = LogManager.GetLogger("ASC.Web.Bundle");
            CultureInfo oldCulture = null;
            try
            {
                if (0 < uri.IndexOf('_'))
                {
                    var cultureName = uri.Split('_').Last().Split('.').FirstOrDefault();
                    var culture = CultureInfo.GetCultureInfo(cultureName);
                    if (!Equals(culture, CultureInfo.CurrentCulture))
                    {
                        oldCulture = CultureInfo.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                        log.DebugFormat("GetContent uri:{0}, oldCulture:{1}, newCulture:{2}", uri, oldCulture.Name, culture.Name);
                    }
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }

            var content = new StringBuilder();
            try
            {
                var fileName = uri.Split('.').FirstOrDefault();
                fileName = Path.GetFileNameWithoutExtension(fileName ?? uri);
                foreach (var script in GetClientScriptListFromCache(fileName))
                {
                    content.Append(script.GetData(HttpContext.Current));
                }

            }
            catch (Exception e)
            {
                log.Error("GetContent uri: " + uri, e);
            }

            if (oldCulture != null)
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
                Thread.CurrentThread.CurrentUICulture = oldCulture;
            }
            
            return new JsTransform(true).Process("", content.ToString());
        }

        public static string GetContentHash(string uri)
        {
            var version = string.Empty;
            var types = new List<Type>();
            var fileName = uri.Split('.').FirstOrDefault();
            fileName = Path.GetFileNameWithoutExtension(fileName ?? uri);

            foreach (var s in GetClientScriptListFromCache(fileName))
            {
                version += s.GetCacheHash();
                types.Add(s.GetType());
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null && types.All(r => r.BaseType != typeof(ClientScriptLocalization)))
            {
                version = string.Join(string.Empty, ToString(tenant.TenantId), ToString(tenant.Version), ToString(tenant.LastModified.Ticks), version);
            }

            return GetHash(version);
        }

        private static string ToString(long value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string GetHash(string s)
        {
            return HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(s)));
        }

        private static IEnumerable<ClientScript> GetClientScriptListFromCache(string fileName)
        {
            return cache.Get<List<string>>(fileName).Select(r =>
            {
                var rSplit = r.Split(',');
                return (ClientScript)Activator.CreateInstance(rSplit[1].Trim(), rSplit[0].Trim()).Unwrap();
            });
        } 
    }
}
