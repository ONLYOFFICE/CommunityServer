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
