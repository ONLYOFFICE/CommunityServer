using System;
using System.Linq;
using ASC.Web.Core.Client.Bundling;

namespace ASC.Web.Studio.Masters
{
    public class CommonBodyScripts : ResourceBundleControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Scripts.AddRange(new []
                             {
                                 "~/js/asc/core/asc.customevents.js",
                                 "~/js/asc/api/api.factory.js",
                                 "~/js/asc/api/api.helper.js",
                                 "~/js/asc/api/api.service.js",
                                 "~/js/asc/api/asc.teamlab.js",
                                 "~/js/asc/plugins/jquery.base64.js",
                                 "~/js/asc/plugins/jquery-customcombobox.js",
                                 "~/js/asc/plugins/jquery-advansedfilter.js",
                                 "~/js/asc/plugins/jquery-advansedselector.js",
                                 "~/js/asc/plugins/jquery-useradvansedselector.js",
                                 "~/js/asc/plugins/jquery-projectadvansedselector.js",
                                 "~/js/asc/plugins/jquery-groupadvansedselector.js",
                                 "~/js/asc/plugins/jquery-milestoneadvansedselector.js",
                                 "~/js/asc/plugins/jquery-contactadvansedselector.js",
                                 "~/js/asc/plugins/jquery-emailadvansedselector.js",
                                 "~/js/asc/plugins/jquery.tlblock.js",
                                 "~/js/asc/plugins/popupBox.js",
                                 "~/js/asc/core/asc.files.utility.js",
                                 "~/js/asc/core/basetemplate.master.init.js",
                                 "~/usercontrols/common/helpcenter/js/help-center.js",
                                 "~/js/asc/core/groupselector.js",
                                 "~/js/asc/core/asc.mail.utility.js",
                                 "~/js/third-party/async.js",
                                 "~/js/third-party/modernizr.js",
                                 "~/js/third-party/zeroclipboard.js",
                                 "~/js/asc/core/clipboard.js",
                                 "~/js/third-party/email-addresses.js",
                                 "~/js/third-party/punycode.js"
                             }.Select(ResolveUrl));
        }
    }
}