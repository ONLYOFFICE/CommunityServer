using System;
using System.Globalization;
using System.Linq;
using ASC.Web.Core.Client.Bundling;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Studio.Masters
{
    public class CommonStyles : ResourceBundleControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Styles.AddRange(new []
                            {
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
                                "~/skins/default/codestyle.css"
                            }.Select(ResolveUrl).ToArray());

            if (WebSkin.HasCurrentCultureCssFile)
            {
                Styles.Add(ResolveUrl("~/skins/default/common_style.css".Replace("css", CultureInfo.CurrentCulture.Name.ToLower() + ".css")));
            }
        }
    }
}