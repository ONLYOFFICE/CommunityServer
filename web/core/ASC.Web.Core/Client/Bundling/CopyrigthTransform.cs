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
using System.Linq;
using System.Web.Optimization;
using ASC.Web.Core.WhiteLabel;

namespace ASC.Web.Core.Client.Bundling
{
    class CopyrigthTransform : IBundleTransform
    {
        public static readonly string CopyrigthText = @"/*
    Copyright (c) {0} " + DateTime.UtcNow.Year + @". All rights reserved.
    {1}
*/
";
        public void Process(BundleContext context, BundleResponse response)
        {
            if (response.Files.Any(f => f.VirtualFile.VirtualPath.ToLowerInvariant().Contains("jquery"))) return;

            var settings = CompanyWhiteLabelSettings.Instance;

            if (!settings.IsLicensor)
                settings = (CompanyWhiteLabelSettings) settings.GetDefault();

            var copyrigth = string.Format(CopyrigthText, settings.CompanyName, settings.Site);

            response.Content = copyrigth + response.Content;
        }
    }
}