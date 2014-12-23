/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
 * 
*/

using ASC.Collections;
using ASC.Data.Storage.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace ASC.Data.Storage
{
    public static class WebPath
    {
        private static readonly IEnumerable<AppenderConfigurationElement> Appenders;
        private static readonly IDictionary<string, bool> Existing = new SynchronizedDictionary<string, bool>();


        static WebPath()
        {
            var section = (StorageConfigurationSection)ConfigurationManager.GetSection(Schema.SECTION_NAME);
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

            if (Appenders.Any())
            {
                var avaliableAppenders = Appenders.Where(x => x.Extensions.Split('|').Contains(ext) || String.IsNullOrEmpty(ext));
                var avaliableAppendersCount = avaliableAppenders.LongCount();

                AppenderConfigurationElement appender;
                if (avaliableAppendersCount > 1)
                {
                    appender = avaliableAppenders.ToList()[(int)(relativePath.Length % avaliableAppendersCount)];
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
                    result = VirtualPathUtility.ToAbsolute(string.Format("{0}/{1}{2}", appender.Append.TrimEnd('/'), relativePath.TrimStart('/'), query));
                }
                else
                {
                    if (SecureHelper.IsSecure() && !string.IsNullOrEmpty(appender.AppendSecure))
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
            //To LOWER! cause Amazon is CASE SENSITIVE!
            return result.ToLowerInvariant();
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