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
using System.Collections.Concurrent;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Web;

using ASC.Common.Logging;
using ASC.Core;

namespace TMResourceData
{
    public class DBResourceManager : ResourceManager
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Resources");

        public DBResourceManager(string filename, Assembly assembly)
                    : base(filename, assembly, typeof(DBResourceSet))
        {
        }

        public static void PatchAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (_, a) => PatchAssembly(a.LoadedAssembly);
            Array.ForEach(AppDomain.CurrentDomain.GetAssemblies(), a => PatchAssembly(a));
        }

        public static void PatchAssembly(Assembly a, bool onlyAsc = true)
        {
            if (!onlyAsc || Accept(a))
            {
                var types = new Type[0];
                try
                {
                    types = a.GetTypes();
                }
                catch (ReflectionTypeLoadException rtle)
                {
                    log.WarnFormat("Can not GetTypes() from assembly {0}, try GetExportedTypes(), error: {1}", a.FullName, rtle.Message);
                    foreach (var e in rtle.LoaderExceptions)
                    {
                        log.Info(e.Message);
                    }

                    try
                    {
                        types = a.GetExportedTypes();
                    }
                    catch (Exception err)
                    {
                        log.ErrorFormat("Can not GetExportedTypes() from assembly {0}: {1}", a.FullName, err.Message);
                    }
                }
                foreach (var type in types)
                {
                    var prop = type.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
                    if (prop != null)
                    {
                        var rm = (ResourceManager)prop.GetValue(type);
                        if (!(rm is DBResourceManager))
                        {
                            var dbrm = new DBResourceManager(rm.BaseName, a);
                            type.InvokeMember("resourceMan", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.SetField, null, type, new object[] { dbrm });
                        }
                    }
                }
            }
        }

        private static bool Accept(Assembly a)
        {
            var n = a.GetName().Name;
            return (n.StartsWith("ASC.") || n.StartsWith("App_GlobalResources")) && a.GetManifestResourceNames().Any();
        }

        class DBResourceSet : ResourceSet
        {
            public DBResourceSet() : base()
            {
            }

            public DBResourceSet(string fileName) : base(fileName)
            {
            }

            public DBResourceSet(IResourceReader reader) : base(reader)
            {
            }

            public DBResourceSet(Stream stream) : base(stream)
            {
            }
        

            public override string GetString(string name, bool ignoreCase)
            {
                var result = base.GetString(name, ignoreCase);
                result = WhiteLabelHelper.ReplaceLogo(name, result);
                return result;
            }
        }
    }

    public class WhiteLabelHelper
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Resources");
        private static readonly ConcurrentDictionary<int, string> whiteLabelDictionary = new ConcurrentDictionary<int, string>();
        private static readonly string replPattern = ConfigurationManagerExtension.AppSettings["resources.whitelabel-text.replacement.pattern"] ?? "(?<=[^@/\\\\]|^)({0})(?!\\.com)";
        public static string DefaultLogoText = "";


        public static void SetNewText(int tenantId, string newText)
        {
            try
            {
                whiteLabelDictionary.AddOrUpdate(tenantId, r => newText, (i, s) => newText);
            }
            catch (Exception e)
            {
                log.Error("SetNewText", e);
            }
        }

        public static void RestoreOldText(int tenantId)
        {
            try
            {
                string text;
                whiteLabelDictionary.TryRemove(tenantId, out text);
            }
            catch (Exception e)
            {
                log.Error("RestoreOldText", e);
            }
        }

        internal static string ReplaceLogo(string resourceName, string resourceValue)
        {
            if (string.IsNullOrEmpty(resourceValue))
            {
                return resourceValue;
            }

            if (HttpContext.Current != null) //if in Notify Service or other process without HttpContext
            {
                try
                {
                    var tenant = CoreContext.TenantManager.GetCurrentTenant(false);
                    if (tenant == null) return resourceValue;

                    string newText;
                    if (whiteLabelDictionary.TryGetValue(tenant.TenantId, out newText))
                    {
                        var newTextReplacement = newText;

                        if (resourceValue.Contains("<") && resourceValue.Contains(">") || resourceName.StartsWith("pattern_"))
                        {
                            newTextReplacement = HttpUtility.HtmlEncode(newTextReplacement);
                        }
                        if (resourceValue.Contains("{0"))
                        {
                            //Hack for string which used in string.Format
                            newTextReplacement = newTextReplacement.Replace("{", "{{").Replace("}", "}}");
                        }

                        var pattern = string.Format(replPattern, DefaultLogoText);
                        //Hack for resource strings with mails looked like ...@onlyoffice... or with website http://www.onlyoffice.com link or with the https://www.facebook.com/pages/OnlyOffice/833032526736775

                        return Regex.Replace(resourceValue, pattern, newTextReplacement, RegexOptions.IgnoreCase).Replace("™", "");
                    }
                }
                catch (Exception e)
                {
                    log.Error("ReplaceLogo", e);
                }
            }

            return resourceValue;
        }
    }
}
