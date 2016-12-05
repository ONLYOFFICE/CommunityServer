/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using log4net;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Caching;
using System.Text.RegularExpressions;
using ASC.Core;
using System.Web;
using ASC.Common.Data;
using ASC.Common.Data.Sql;
using ASC.Common.Data.Sql.Expressions;

namespace TMResourceData
{
    public class DBResourceManager : ResourceManager
    {
        public static bool WhiteLableEnabled = false;
        public static bool ResourcesFromDataBase { get; private set; }
        private static readonly ILog log = LogManager.GetLogger("ASC.Resources");
        private readonly ConcurrentDictionary<string, ResourceSet> resourceSets = new ConcurrentDictionary<string, ResourceSet>();

        static DBResourceManager()
        {
            ResourcesFromDataBase = string.Equals(ConfigurationManager.AppSettings["resources.from-db"], "true");
        }

        public DBResourceManager(string filename, Assembly assembly)
                    : base(filename, assembly)
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


        public override Type ResourceSetType
        {
            get { return typeof(DBResourceSet); }
        }


        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            ResourceSet set;
            resourceSets.TryGetValue(culture.Name, out set);
            if (set == null)
            {
                var invariant = culture == CultureInfo.InvariantCulture ? base.InternalGetResourceSet(CultureInfo.InvariantCulture, true, true) : null;
                set = new DBResourceSet(invariant, culture, BaseName);
                resourceSets.AddOrUpdate(culture.Name, set, (k, v) => set);
            }
            return set;
        }


        class DBResourceSet : ResourceSet
        {
            private const string NEUTRAL_CULTURE = "Neutral";

            private static readonly TimeSpan cacheTimeout = TimeSpan.FromMinutes(120); // for performance
            private readonly object locker = new object();
            private readonly MemoryCache cache;
            private readonly ResourceSet invariant;
            private readonly string culture;
            private readonly string filename;


            static DBResourceSet()
            {
                try
                {
                    var defaultValue = ((int)cacheTimeout.TotalMinutes).ToString();
                    cacheTimeout = TimeSpan.FromMinutes(Convert.ToInt32(ConfigurationManager.AppSettings["resources.cache-timeout"] ?? defaultValue));
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
            }


            public DBResourceSet(ResourceSet invariant, CultureInfo culture, string filename)
            {
                if (culture == null)
                {
                    throw new ArgumentNullException("culture");
                }
                if (string.IsNullOrEmpty(filename))
                {
                    throw new ArgumentNullException("filename");
                }

                this.invariant = invariant;
                this.culture = invariant != null ? NEUTRAL_CULTURE : culture.Name;
                this.filename = filename.Split('.').Last() + ".resx";
                cache = MemoryCache.Default;
            }

            public override string GetString(string name, bool ignoreCase)
            {
                var result = (string)null;
                try
                {
                    var dic = GetResources();
                    dic.TryGetValue(name, out result);
                }
                catch (Exception err)
                {
                    log.ErrorFormat("Can not get resource from {0} for {1}: GetString({2}), {3}", filename, culture, name, err);
                }

                if (invariant != null && result == null)
                {
                    result = invariant.GetString(name, ignoreCase);
                }

                if (WhiteLableEnabled)
                {
                    result = WhiteLabelHelper.ReplaceLogo(name, result);
                }

                return result;
            }

            public override IDictionaryEnumerator GetEnumerator()
            {
                var result = new Hashtable();
                if (invariant != null)
                {
                    foreach (DictionaryEntry e in invariant)
                    {
                        result[e.Key] = e.Value;
                    }
                }
                try
                {
                    var dic = GetResources();
                    foreach (var e in dic)
                    {
                        result[e.Key] = e.Value;
                    }
                }
                catch (Exception err)
                {
                    log.Error(err);
                }
                return result.GetEnumerator();
            }

            private Dictionary<string, string> GetResources()
            {
                var key = string.Format("{0}/{1}", filename, culture);
                var dic = cache.Get(key) as Dictionary<string, string>;
                if (dic == null)
                {
                    lock (locker)
                    {
                        dic = cache.Get(key) as Dictionary<string, string>;
                        if (dic == null)
                        {
                            var policy = cacheTimeout == TimeSpan.Zero ? null : new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.Add(cacheTimeout) };
                            cache.Set(key, dic = LoadResourceSet(filename, culture), policy);
                        }
                    }
                }
                return dic;
            }

            private static Dictionary<string, string> LoadResourceSet(string filename, string culture)
            {
                using (var db = new DbManager("tmresource"))
                {
                    var q = new SqlQuery("res_data d")
                        .Select("d.title", "d.textvalue")
                        .InnerJoin("res_files f", Exp.EqColumns("f.id", "d.fileid"))
                        .Where("f.resname", filename)
                        .Where("d.culturetitle", culture);
                    return db.ExecuteList(q)
                        .ToDictionary(r => (string)r[0], r => (string)r[1], StringComparer.InvariantCultureIgnoreCase);
                }
            }
        }
    }

    public class WhiteLabelHelper
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Resources");
        private static readonly ConcurrentDictionary<int, string> whiteLabelDictionary = new ConcurrentDictionary<int, string>();
        private static readonly string replPattern = ConfigurationManager.AppSettings["resources.whitelabel-text.replacement.pattern"] ?? "(?<=[^@/\\\\]|^)({0})(?!\\.com)";
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
