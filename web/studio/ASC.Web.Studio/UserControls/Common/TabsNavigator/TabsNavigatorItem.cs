/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using System.Text;
using System.Web.UI;

namespace ASC.Web.Studio.UserControls.Common.TabsNavigator
{
    public class TabsNavigatorItem : Control
    {
        public string OnClickText { get; set; }

        public string TabName { get; set; }

        public string TabAnchorName { get; set; }

        public string TabHref { get; set; }

        public bool IsSelected { get; set; }

        public string DivID = Guid.NewGuid().ToString();

        public bool SkipRender { get; set; }

        public string GetTabLink(bool isLast)
        {
            var tabCssName = String.IsNullOrEmpty(TabAnchorName)
                                 ? (IsSelected ? "tabsNavigationLink selectedTab" : "tabsNavigationLink")
                                 : String.Format("{0} tabsNavigationLink_{1}",
                                                 IsSelected ? "tabsNavigationLink selectedTab" : "tabsNavigationLink", TabAnchorName);

            var href = String.IsNullOrEmpty(TabHref) || IsSelected ? "" : String.Format(" href='{0}'", TabHref);

            var javascriptText = String.IsNullOrEmpty(TabHref)
                                     ? String.Format(" onclick=\"{0} ASC.Controls.TabsNavigator.toggleTabs(this.id, '{1}');\"",
                                                     String.IsNullOrEmpty(OnClickText) ? String.Empty : OnClickText + ";",
                                                     TabAnchorName)
                                     : String.Empty;

            var sb = new StringBuilder();
            sb.AppendFormat("<a id='{0}_tab' class='{1}'{2} {3}>{4}</a>",
                            DivID, tabCssName, href, javascriptText, TabName);
            if (!isLast)
            {
                sb.AppendFormat("<span class=\"splitter\"></span>");
            }

            return sb.ToString();
        }

        public void RenderTabContent(HtmlTextWriter writer)
        {
            writer.Write("<div id='{0}'{1}>", DivID, IsSelected ? string.Empty : " class='display-none'");
            RenderControl(writer);
            writer.Write("</div>");
        }
    }
}