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