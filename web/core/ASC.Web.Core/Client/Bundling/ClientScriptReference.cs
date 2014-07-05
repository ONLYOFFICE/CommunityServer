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

using ASC.Core;
using ASC.Web.Core.Client.HttpHandlers;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASC.Web.Core.Client.Bundling
{
    [ToolboxData("<{0}:ClientScriptReference runat=server></{0}:ClientScriptReference>")]
    public class ClientScriptReference : WebControl
    {
        private static readonly ConcurrentDictionary<string, List<ClientScript>> cache = new ConcurrentDictionary<string, List<ClientScript>>(StringComparer.InvariantCultureIgnoreCase);


        public virtual ICollection<Type> Includes
        {
            get;
            private set;
        }


        public ClientScriptReference()
        {
            Includes = new HashSet<Type>();
        }


        public override void RenderBeginTag(HtmlTextWriter writer)
        {
        }

        public override void RenderEndTag(HtmlTextWriter writer)
        {
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            var link = GetLink(false);
            if (ClientSettings.BundlingEnabled)
            {
                var bundle = BundleHelper.GetJsBundle(link);
                if (bundle == null)
                {
                    bundle = BundleHelper.JsBundle(link).Include(link, true);
                    bundle.UseCache = false;
                    BundleHelper.AddBundle(bundle);
                }
                output.Write(BundleHelper.HtmlScript(link));
            }
            else
            {
                output.Write(BundleHelper.HtmlScript(VirtualPathUtility.ToAbsolute(link), false, false));
            }
        }

        public string GetLink(bool onlyLocalization)
        {
            var filename = string.Empty;
            foreach (var type in Includes)
            {
                filename += type.FullName.ToLowerInvariant();
            }
            var filenameHash = GetHash(filename) + "_" + CultureInfo.CurrentCulture.Name.ToLowerInvariant();

            var scripts = new List<ClientScript>();
            if (!cache.TryGetValue(filenameHash, out scripts))
            {
                scripts = Includes.Select(type => (ClientScript)Activator.CreateInstance(type)).ToList();
                cache.TryAdd(filenameHash, scripts);
                if (onlyLocalization && scripts.Any(s => !(s is ClientScriptLocalization)))
                {
                    throw new InvalidOperationException("Only localization script available.");
                }
            }

            return string.Format("~{0}{1}.js?ver={2}", BundleHelper.CLIENT_SCRIPT_VPATH, filenameHash, GetContentHash(filenameHash));
        }

        public static string GetContent(string uri)
        {
            CultureInfo oldCulture = null;
            try
            {
                if (0 < uri.IndexOf('_'))
                {
                    var cultureName = uri.Split('_').Last().Split('.').FirstOrDefault();
                    var culture = CultureInfo.GetCultureInfo(cultureName);
                    if (culture != CultureInfo.CurrentCulture)
                    {
                        oldCulture = CultureInfo.CurrentCulture;
                        Thread.CurrentThread.CurrentCulture = culture;
                        Thread.CurrentThread.CurrentUICulture = culture;
                    }
                }
            }
            catch (Exception err)
            {
                LogManager.GetLogger("ASC.Web.Bundle").Error(err);
            }

            var content = new StringBuilder();
            foreach (var script in cache[Path.GetFileNameWithoutExtension(uri)])
            {
                content.Append(script.GetData(HttpContext.Current));
            }

            if (oldCulture != null)
            {
                Thread.CurrentThread.CurrentCulture = oldCulture;
                Thread.CurrentThread.CurrentUICulture = oldCulture;
            }
            
            return content.ToString();
        }

        public static string GetContentHash(string uri)
        {
            var version = string.Empty;
            var types = new List<Type>();
            foreach (var s in cache[Path.GetFileNameWithoutExtension(uri)])
            {
                version += s.GetCacheHash();
                types.Add(s.GetType());
            }

            var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
            if (tenant != null && types.All(r => r.BaseType != typeof(ClientScriptLocalization)))
            {
                version = String.Join(string.Empty, new[] { ToString(tenant.TenantId), ToString(tenant.Version), ToString(tenant.LastModified.Ticks), version });
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
    }
}
