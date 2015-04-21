/*
 *
 * (c) Copyright Ascensio System Limited 2010-2015
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
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace ASC.Web.Studio.Controls.Common
{
    public enum HighlightStyle
    {
        Ascetic,
        Dark,
        Default,
        Far,
        Idea,
        Magula,
        Sunburst,
        VS,
        Zenburn
    }

    [ToolboxData("<{0}:CodeHighlighter runat=server></{0}:CodeHighlighter>")]
    public class CodeHighlighter : Control
    {
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(HighlightStyle), "VS")]
        [Localizable(true)]
        public HighlightStyle HighlightStyle
        {
            get
            {
                var hs = ViewState["HighlightStyle"];
                return (hs == null ? HighlightStyle.VS : (HighlightStyle)hs);
            }
            set { ViewState["HighlightStyle"] = value; }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Page.RegisterBodyScripts(ResolveUrl("~/js/third-party/highlight.pack.js"));
            Page.RegisterInlineScript("hljs.initHighlightingOnLoad();");

            var cssFileName = HighlightStyle.ToString().ToLowerInvariant();
            Page.RegisterStyleControl(VirtualPathUtility.ToAbsolute("~/app_themes/codehighlighter/" + cssFileName + ".css"));
        }

        public static string GetJavaScriptLiveHighlight()
        {
            return GetJavaScriptLiveHighlight(false);
        }

        public static string GetJavaScriptLiveHighlight(bool addScriptTags)
        {
            const string script = "jq(document).ready(function(){jq('code').each(function(){ hljs.highlightBlock(jq(this).get(0));});});";
            return
                addScriptTags
                    ? string.Format("<script language='javascript' type='text/javascript'>{0}</script>", script)
                    : script;
        }
    }
}