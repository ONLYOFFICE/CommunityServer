using System;
using ASC.Web.Core.Client.Bundling;

namespace ASC.Web.Studio.Masters
{
    public class CommonStyles : ResourceStyleBundleControl, IStaticBundle
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SetData(GetStaticStyleSheet());
        }

        public ScriptBundleData GetStaticJavaScript()
        {
            return null;
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return (StyleBundleData)
                new StyleBundleData("studio", "common")
                    .AddSource(ResolveUrl,
                        "~/skins/default/jquery_style.css",
                        "~/skins/default/main-title-icon.less",
                        "~/skins/default/empty-screen-control.less",
                        "~/skins/default/common_style.less",
                        "~/skins/default/page-tabs-navigators.less",
                        "~/skins/default/main-page-container.less",
                        "~/skins/default/wizard.less",
                        "~/skins/default/helper.less",
                        "~/skins/default/comments-container.less",
                        "~/skins/default/filetype_style.css",
                        "~/skins/default/magnific-popup.less",
                        "~/skins/default/toastr.less",
                        "~/skins/default/groupselector.css",
                        "~/skins/default/jquery-advansedfilter.css",
                        "~/skins/default/jquery-advansedfilter-fix.less",
                        "~/skins/default/jquery-advansedselector.less",
                        "~/skins/default/jquery-emailadvansedselector.css",
                        "~/skins/default/codestyle.css");
        }
    }
}