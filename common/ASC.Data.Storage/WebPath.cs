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


using ASC.Data.Storage.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using ASC.Core;

namespace ASC.Data.Storage
{
    public static class WebPath
    {
        private static readonly IEnumerable<AppenderConfigurationElement> Appenders;
        private static readonly IDictionary<string, bool> Existing = new ConcurrentDictionary<string, bool>();


        static WebPath()
        {
            var section = (StorageConfigurationSection)ConfigurationManagerExtension.GetSection(Schema.SECTION_NAME);
            if (section != null)
            {
                Appenders = section.Appenders.Cast<AppenderConfigurationElement>();
            }
        }

        public static string GetRelativePath(string absolutePath)
        {
            if (!Uri.IsWellFormedUriString(absolutePath, UriKind.Absolute))
            {
                throw new ArgumentException(string.Format("bad path format {0} is not absolute", absolutePath));
            }

            var appender = Appenders.FirstOrDefault(x => absolutePath.Contains(x.Append) || (absolutePath.Contains(x.AppendSecure) && !string.IsNullOrEmpty(x.AppendSecure)));
            if (appender == null)
            {
                return absolutePath;
            }
            return SecureHelper.IsSecure() && !string.IsNullOrEmpty(appender.AppendSecure) ?
                absolutePath.Remove(0, appender.AppendSecure.Length) :
                absolutePath.Remove(0, appender.Append.Length);
        }

        public static string GetPath(string relativePath)
        {
            if (relativePath.StartsWith("~"))
            {
                throw new ArgumentException(string.Format("bad path format {0} remove '~'", relativePath), "relativePath");
            }

            var result = relativePath;
            var ext = Path.GetExtension(relativePath).ToLowerInvariant();

            if (CoreContext.Configuration.Standalone && StaticUploader.CanUpload())
            {
                try
                {
                    result = CdnStorageSettings.Load().DataStore.GetInternalUri("", relativePath, TimeSpan.Zero, null).AbsoluteUri.ToLower();
                    if (!string.IsNullOrEmpty(result)) return result;
                }
                catch (Exception)
                {
                    
                }
            }
            
            if (Appenders.Any()) {
                var avaliableAppenders = Appenders.Where(x => x.Extensions.Split('|').Contains(ext) || String.IsNullOrEmpty(ext)).ToList();
                var avaliableAppendersCount = avaliableAppenders.LongCount();

                AppenderConfigurationElement appender;
                if (avaliableAppendersCount > 1)
                {
                    appender = avaliableAppenders[(int)(relativePath.Length % avaliableAppendersCount)];
                }
                else if (avaliableAppendersCount == 1)
                {
                    appender = avaliableAppenders.First();
                }
                else
                {
                    appender = Appenders.First();
                }

                if (appender.Append.StartsWith("~"))
                {
                    var query = string.Empty;
                    //Rel path
                    if (relativePath.IndexOfAny(new[] { '?', '=', '&' }) != -1)
                    {
                        //Cut it
                        query = relativePath.Substring(relativePath.IndexOf('?'));
                        relativePath = relativePath.Substring(0, relativePath.IndexOf('?'));
                    }
                    if (HostingEnvironment.IsHosted)
                    {
                        result = VirtualPathUtility.ToAbsolute(string.Format("{0}/{1}{2}", appender.Append.TrimEnd('/'), relativePath.TrimStart('/'), query));
                    }
                    else
                    {
                        result = string.Format("{0}/{1}{2}", appender.Append.TrimEnd('/').TrimStart('~'), relativePath.TrimStart('/'), query);
                    }
                }
                else
                {
                    if (HostingEnvironment.IsHosted && SecureHelper.IsSecure() && !string.IsNullOrEmpty(appender.AppendSecure))
                    {
                        result = string.Format("{0}/{1}", appender.AppendSecure.TrimEnd('/'), relativePath.TrimStart('/'));
                    }
                    else
                    {
                        //Append directly
                        result = string.Format("{0}/{1}", appender.Append.TrimEnd('/'), relativePath.TrimStart('/'));
                    }
                }
            }

            return result;
        }

        public static bool Exists(string relativePath)
        {
            var path = GetPath(relativePath);
            if (!Existing.ContainsKey(path))
            {
                if (Uri.IsWellFormedUriString(path, UriKind.Relative) && HttpContext.Current != null)
                {
                    //Local
                    Existing[path] = File.Exists(HttpContext.Current.Server.MapPath(path));
                }
                if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                {
                    //Make request
                    Existing[path] = CheckWebPath(path);
                }
            }
            return Existing[path];
        }

        private static bool CheckWebPath(string path)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(path);
                request.Method = "HEAD";
                using (var resp = (HttpWebResponse)request.GetResponse())
                {
                    return resp.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}