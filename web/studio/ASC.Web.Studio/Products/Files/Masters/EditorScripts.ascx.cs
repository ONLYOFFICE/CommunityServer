using System;
using System.Linq;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Files.Classes;

namespace ASC.Web.Files.Masters
{
    public class EditorScripts : ResourceScriptBundleControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AddSource(ResolveUrl,
                "~/js/third-party/jquery/jquery.core.js",
                "~/js/asc/core/localstorage.js"
                );

            AddSource(PathProvider.GetFileStaticRelativePath,
                "common.js",
                "servicemanager.js",
                "editor.js");
        }
    }
}