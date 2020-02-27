/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using ASC.Data.Storage;

namespace ASC.Web.Core.Client.Bundling
{
    public abstract class ResourceBundleControl : UserControl
    {
        protected BundleData bundleData;

        protected ResourceBundleControl()
        {
            bundleData = CreateNewBundleData("", "");
        }

        public void AddSource(Func<string, string> converter, params string[] src)
        {
            bundleData.AddSource(converter, src);
        }

        protected void SetData(BundleData data)
        {
            bundleData = CreateNewBundleData(data.FileName, data.CategoryName).AddSource(r => r, data.GetSource().ToArray());
        }

        protected abstract BundleData CreateNewBundleData(string path, string categoryName);

        protected override void Render(HtmlTextWriter writer)
        {
            if (!bundleData.GetSource().Any()) return;

            if (ClientSettings.BundlingEnabled)
            {
                writer.Write(GetLink());
            }
            else
            {
                Write(writer);
            }
        }

        private void Write(HtmlTextWriter writer)
        {
            base.Render(writer);

            foreach (var s in bundleData.GetSource().Select(ReplaceLess).Select(WebPath.GetPath))
            {
                writer.WriteLine(bundleData.GetLink(s, false));
            }
        }

        private string ReplaceLess(string path)
        {
            if (Path.GetExtension(path) != ".less" && Path.GetExtension(path) != ".css") return path;

            if (HttpContext.Current != null)
            {
                var filePath = HttpContext.Current.Server.MapPath(path);
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath.Replace(".css", ".min.css").Replace(".less", ".min.css")))
                {
                    path = path.Replace(".css", ".min.css").Replace(".less", ".min.css");
                }
            }

            return path;
        }

        private string GetLink()
        {
            var path = bundleData.GetStorageVirtualPath(ClientSettings.ResetCacheKey);

            if (DiscTransform.SuccessInitialized && DiscTransform.IsFile(path) && !StaticUploader.CanUpload())
            {
                return bundleData.GetLink(DiscTransform.GetUri(path), false);
            }

            return BundleHelper.AddBundle(bundleData);
        }
    }

    public class ResourceStyleBundleControl : ResourceBundleControl
    {
        public void SetData(StyleBundleData data)
        {
            base.SetData(data);
        }

        protected override BundleData CreateNewBundleData(string path, string categoryName)
        {
            return new StyleBundleData(path, categoryName);
        }
    }

    public class ResourceScriptBundleControl : ResourceBundleControl
    {
        public void SetData(ScriptBundleData data)
        {
            base.SetData(data);
        }

        protected override BundleData CreateNewBundleData(string path, string categoryName)
        {
            return new ScriptBundleData(path, categoryName);
        }
    }
}