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

using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Optimization;
using ASC.Core.Tenants;
using ASC.Data.Storage;
using log4net;

namespace ASC.Web.Core.Client.Bundling
{
    class DiscTransform : IBundleTransform
    {
        private static readonly ILog log = LogManager.GetLogger("ASC.Web.Bundle.DiscTransform");

        private static readonly IDataStore storage = StorageFactory.GetStorage(Tenant.DEFAULT_TENANT.ToString(CultureInfo.InvariantCulture), "bundle");

        public void Process(BundleContext context, BundleResponse response)
        {
            if (!BundleTable.Bundles.UseCdn) return;
            
            try
            {
                var bundle = context.BundleCollection.GetBundleFor(context.BundleVirtualPath);
                if (bundle != null)
                {
                    UploadToDisc(new CdnItem { Bundle = bundle, Response = response });
                }
            }
            catch (Exception fatal)
            {
                log.Fatal(fatal);
                throw;
            }
        }

        private static void UploadToDisc(CdnItem item)
        {
            try
            {
                var filePath = GetFullFileName(item.Bundle.Path, item.Response.ContentType).TrimStart('/');

                using (var mutex = new Mutex(true, filePath))
                {
                    mutex.WaitOne();

                    if (!storage.IsFile("", filePath))
                    {
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(item.Response.Content)))
                        {
                            item.Bundle.CdnPath = storage.Save(filePath, stream).ToString();
                        }
                    }
                    else
                    {
                        item.Bundle.CdnPath = GetUri(item.Bundle.Path, item.Response.ContentType);
                    }

                    mutex.ReleaseMutex();
                }
            }
            catch (Exception err)
            {
                log.Error(err);
            }
        }

        internal static string GetUri(string path, string contentType)
        {
            return storage.GetUri(GetFullFileName(path, contentType)).ToString();
        }

        internal static string GetFullFileName(string path, string contentType)
        {
            var href = Path.GetFileNameWithoutExtension(path);
            var category = GetCategoryFromPath(path);

            var hrefTokenSource = href;

            if (Uri.IsWellFormedUriString(href, UriKind.Relative))
                hrefTokenSource = (SecureHelper.IsSecure() ? "https" : "http") + href;

            var hrefToken = string.Concat(HttpServerUtility.UrlTokenEncode(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(hrefTokenSource))));

            if (string.Compare(contentType, "text/css", StringComparison.OrdinalIgnoreCase) == 0)
                return string.Format("/{0}/css/{1}.css", category, hrefToken);

            return string.Format("/{0}/javascript/{1}.js", category, hrefToken);
        }


        private static string GetCategoryFromPath(string controlPath)
        {
            var result = "common";

            controlPath = controlPath.ToLower();
            var matches = Regex.Match(controlPath, "~/(\\w+)/(\\w+)-(\\w+)/?", RegexOptions.Compiled);

            if (matches.Success && matches.Groups.Count > 3 && matches.Groups[2].Success)
                result = matches.Groups[2].Value;

            return result;
        }
    }
}