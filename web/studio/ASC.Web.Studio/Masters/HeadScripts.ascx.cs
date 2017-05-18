using System;
using ASC.Web.Core.Client.Bundling;

namespace ASC.Web.Studio.Masters
{
    public class HeadScripts : ResourceScriptBundleControl, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetData(GetStaticJavaScript());
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return (ScriptBundleData)
                new ScriptBundleData("head", "common")
                    .AddSource(ResolveUrl,
                        "~/js/third-party/jquery/jquery.core.js",
                        "~/js/third-party/jquery/jquery.ui.js",
                        "~/js/third-party/jquery/jquery.tmpl.js",
                        "~/js/third-party/jquery/jquery.json.js",
                        "~/js/third-party/jquery/jquery.cookies.js",
                        "~/js/asc/plugins/jquery.tlcombobox.js",
                        "~/js/asc/plugins/jquery.browser.js",
                        "~/js/third-party/jquery/jquery.blockUI.js",
                        "~/js/third-party/jquery/jquery.mask.js",
                        "~/js/third-party/jquery/jquery.scrollTo.js",
                        "~/js/third-party/jquery/jquery.colors.js",
                        "~/js/third-party/jquery/jquery.magnific-popup.js",
                        "~/js/third-party/jquery/toastr.js",
                        "~/js/third-party/jquery/jquery.caret.1.02.min.js",
                        "~/js/asc/plugins/jquery.helper.js",
                        "~/js/asc/core/asc.anchorcontroller.js",
                        "~/js/asc/core/common.js",
                        "~/js/asc/plugins/jquery.dropdownToggle.js",
                        "~/js/asc/core/asc.topstudiopanel.js",
                        "~/js/asc/core/asc.pagenavigator.js",
                        "~/js/asc/core/asc.tabsnavigator.js",
                        "~/js/asc/core/localstorage.js",
                        "~/js/third-party/ajaxpro.core.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return null;
        }
    }
}