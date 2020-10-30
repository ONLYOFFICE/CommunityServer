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
using System.Text;
using System.Web.Optimization;
using ASC.Common.Logging;
using ASC.Data.Storage;

namespace ASC.Web.Core.Client.Bundling
{
    class StorageTransform : IBundleTransform
    {
        private static readonly ILog Log= LogManager.GetLogger("ASC.Web.Bundle.StorageTransform");

        public void Process(BundleContext context, BundleResponse response)
        {
            if (!BundleTable.Bundles.UseCdn) return;

            try
            {
                var bundle = context.BundleCollection.GetBundleFor(context.BundleVirtualPath);
                if (bundle != null)
                {
                    var fileName = Path.GetFileName(bundle.Path);
                    if (string.IsNullOrEmpty(fileName)) return;
                    var path = Path.Combine("App_Data", fileName);
                    var relativePath = Path.GetTempFileName();
                    
                    File.WriteAllText(relativePath, response.Content, Encoding.UTF8);

                    StaticUploader.UploadFileAsync(path, relativePath, r =>
                    {
                        try
                        {
                            bundle.CdnPath = r;
                            File.Delete(relativePath);
                        }
                        catch (Exception e)
                        {
                            Log.Error("StorageTransform", e);
                        }
                    });
                }
            }
            catch (Exception fatal)
            {
                Log.Fatal(fatal);
                throw;
            }
        }
    }
}
