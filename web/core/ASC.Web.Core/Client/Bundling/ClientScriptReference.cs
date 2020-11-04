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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using ASC.Common.Caching;
using ASC.Common.Logging;
using ASC.Core;
using ASC.Web.Core.Client.HttpHandlers;

namespace ASC.Web.Core.Client.Bundling
{
    [ToolboxData("<{0}:ClientScriptReference runat=server></{0}:ClientScriptReference>")]
    public class ClientScriptReference : WebControl
    {
        private static readonly ICache Cache = AscCache.Default;
        private List<ClientScript> includes;


        public ClientScriptReference()
        {
            includes = new List<ClientScript>();
        }

        public ClientScriptReference AddScript(ClientScript clientScript)
        {
            if (includes.All(r => r.GetType().FullName != clientScript.GetType().FullName))
            {
                includes.Add(clientScript);
            }

            return this;
        }

        public override void RenderBeginTag(HtmlTextWriter writer) { }
        public override void RenderEndTag(HtmlTextWriter writer) { }

        protected override void RenderContents(HtmlTextWriter output)
        {
            var link = GetLink();
            output.Write(BundleHelper.GetJavascriptLink(VirtualPathUtility.ToAbsolute(link), false));
        }

        public string GetLink()
        {
            var filename = string.Empty;
            foreach (var type in includes)
            {
                filename += (type.GetType().FullName ?? "").ToLowerInvariant();
            }
            var filenameHash = GetHash(filename) + "_" + CultureInfo.CurrentCulture.Name.ToLowerInvariant();

            var scripts = Cache.Get<List<string>>(filenameHash.ToLowerInvariant());
            if (scripts == null)
            {
                scripts = includes.Select(type => type.GetType().AssemblyQualifiedName).ToList();
                Cache.Insert(filenameHash.ToLowerInvariant(), scripts, DateTime.MaxValue);
            }

            return string.Format("~{0}{1}.js?ver={2}", BundleHelper.CLIENT_SCRIPT_VPATH, filenameHash, GetContentHash(filenameHash));
        }

        public async Task<string> GetContentAsync(HttpContext context)
        {
            var log = LogManager.GetLogger("ASC.Web.Bundle");
            CultureInfo oldCulture = null;
            var uri = context.Request.Url.AbsolutePath;

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
                var fileName = Path.GetFileNameWithoutExtension(uri.Split('.').FirstOrDefault() ?? uri);
                await Task.WhenAll(GetClientScriptListFromCache(fileName).Select(script => script.GetDataAsync(context, content)));

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
            
            return content.ToString();
        }

        public string GetContentHash(string uri)
        {
            var version = string.Empty;
            var fileName = uri.Split('.').FirstOrDefault();
            fileName = Path.GetFileNameWithoutExtension(fileName ?? uri);

            includes = GetClientScriptListFromCache(fileName);

            if (includes.Any() && (includes.All(r => r is ClientScriptLocalization) || includes.All(r => r is ClientScriptTemplate)))
            {
                version = includes.First().GetCacheHash();
            }
            else
            {
                foreach (var s in includes)
                {
                    version += s.GetCacheHash();
                }

                var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                if (tenant != null)
                {
                    version = string.Join(string.Empty, ToString(tenant.TenantId), ToString(tenant.Version), ToString(tenant.LastModified.Ticks), version);
                }
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

        private List<ClientScript> GetClientScriptListFromCache(string fileName)
        {
            if (!includes.Any())
            {
                var fromCache = Cache.Get<List<string>>(fileName.ToLowerInvariant());

                if (fromCache != null)
                {
                    includes = fromCache.Select(r =>
                    {
                        var rSplit = r.Split(',');
                        return (ClientScript) Activator.CreateInstance(rSplit[1].Trim(), rSplit[0].Trim()).Unwrap();
                    }).ToList();
                }
            }

            return includes;
        } 
    }
}
