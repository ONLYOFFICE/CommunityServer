/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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