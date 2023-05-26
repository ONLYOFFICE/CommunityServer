/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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
using System.IO;
using System.Linq;
using System.Web;

using ASC.Common.Logging;

namespace ASC.Web.Core
{
    public abstract class WarmupPage : BasePage
    {
        protected virtual List<string> Pages { get; }
        protected virtual List<string> Exclude { get; }
        protected void Page_Load(object sender, EventArgs e)
        {
            var path = Path.GetDirectoryName(HttpContext.Current.Request.PhysicalPath);

            var files = Directory.GetFiles(path, "*.aspx", SearchOption.TopDirectoryOnly)
                .Select(r => Path.GetFileName(r))
                .Where(r => !r.EndsWith("warmup.aspx", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (Pages != null && Pages.Any())
            {
                files.AddRange(Pages);
            }

            if (Exclude != null && Exclude.Any())
            {
                files = files.Except(Exclude).ToList();
            }

            foreach (var file in files.Distinct())
            {
                var page = Path.GetFileName(file);
                try
                {
                    HttpContext.Current.Server.Execute(page);
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("ASC").Error("Warmup " + page, ex);
                }
            }

            HttpContext.Current.Response.Clear();
        }
    }
}
