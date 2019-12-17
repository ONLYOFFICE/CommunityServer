/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
 *
*/


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
                        "~/js/third-party/jquery/jquery.scrollto.js",
                        "~/js/third-party/jquery/jquery.colors.js",
                        "~/js/third-party/jquery/toastr.js",
                        "~/js/third-party/jquery/jquery.caret.1.02.min.js",
                        "~/js/asc/plugins/jquery.helper.js",
                        "~/js/asc/core/asc.anchorcontroller.js",
                        "~/js/asc/core/common.js",
                        "~/js/asc/plugins/jquery.dropdowntoggle.js",
                        "~/js/asc/core/asc.topstudiopanel.js",
                        "~/js/asc/core/asc.pagenavigator.js",
                        "~/js/asc/core/asc.tabsnavigator.js",
                        "~/js/asc/core/localstorage.js",
                        "~/js/third-party/ajaxpro.core.js",
                        "~/js/asc/plugins/svg4everybody.js");
        }

        public StyleBundleData GetStaticStyleSheet()
        {
            return null;
        }
    }
}