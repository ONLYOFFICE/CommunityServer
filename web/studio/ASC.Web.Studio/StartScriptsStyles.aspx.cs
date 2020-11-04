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
using System.Collections.Generic;
using System.Linq;
using System.Web.Optimization;
using ASC.Web.Core;

namespace ASC.Web.Studio.UserControls.FirstTime
{
    public partial class StartScriptsStyles : BasePage
    {
        protected List<String> ListUri { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            ListUri = new List<string>();
            var module = Request["module"];
            foreach (var bundle in BundleTable.Bundles.ToArray())
            {
                if (0 <= bundle.Path.IndexOf("/common", StringComparison.InvariantCultureIgnoreCase) ||
                    (!string.IsNullOrWhiteSpace(module) && 0 <= bundle.Path.IndexOf("/" + module, StringComparison.InvariantCultureIgnoreCase)))
                {
                    ListUri.Add(BundleTable.Bundles.ResolveBundleUrl(bundle.Path));
                }
            }
        }
    }
}