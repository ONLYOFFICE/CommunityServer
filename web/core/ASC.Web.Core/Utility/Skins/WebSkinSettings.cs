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
using System.Globalization;
using System.IO;
using System.Web;
using ASC.Data.Storage;

namespace ASC.Web.Core.Utility.Skins
{
    public class WebSkin
    {
        public static string BaseCSSFileAbsoluteWebPath
        {
            get { return WebPath.GetPath("/skins/default/common_style.css"); }
        }


        private static readonly HashSet<string> BaseCultureCss = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static bool HasCurrentCultureCssFile
        {
            get { return BaseCultureCss.Contains(CultureInfo.CurrentCulture.Name); }
        }

        static WebSkin()
        {
            if (HttpContext.Current == null) return;

            try
            {
                var dir = HttpContext.Current.Server.MapPath("~/skins/default/");
                if (!Directory.Exists(dir)) return;

                foreach (var f in Directory.GetFiles(dir, "common_style.*.css"))
                {
                    BaseCultureCss.Add(Path.GetFileName(f).Split('.')[1]);
                }
            }
            catch
            {
            }
        }
    }
}