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


using System.Text;
using System.Web.Optimization;
using ASC.Web.Core.Client.HttpHandlers;
using Microsoft.Ajax.Utilities;

namespace ASC.Web.Core.Client.Bundling
{
    class JsTransform : IItemTransform, IBundleTransform
    {
        public string Process(string includedVirtualPath, string input)
        {
            if (!BundleTable.EnableOptimizations) return input;

            var stringBuilder = new StringBuilder();
            if (includedVirtualPath.EndsWith("html"))
            {
                ClientScript.GetTemplateData(input, stringBuilder);
                return new Minifier().MinifyJavaScript(stringBuilder.ToString());
            }

            return new Minifier().MinifyJavaScript(input);
        }

        public void Process(BundleContext context, BundleResponse response)
        {
            response.ContentType = "application/javascript";
        }
    }
}