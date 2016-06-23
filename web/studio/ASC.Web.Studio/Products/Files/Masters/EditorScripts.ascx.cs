using System;
using System.Linq;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Masters
{
    public class EditorScripts : ResourceBundleControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Scripts.AddRange(new[]
                             {
                                 "~/js/third-party/jquery/jquery.core.js",
                                 "~/js/asc/core/localstorage.js"
                             }.Select(ResolveUrl));

            Scripts.AddRange(new[]
                             {
                                 "common.js",
                                 "servicemanager.js",
                                 "editor.js"
                             }.Select(PathProvider.GetFileStaticRelativePath));
        }
    }
}