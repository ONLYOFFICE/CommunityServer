/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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

namespace ASC.Web.Studio.UserControls.Common.ViewSwitcher
{
    public class ViewSwitcherTabItem : Control
    {
        public string OnClickText { get; set; }

        public string TabName { get; set; }

        public string TabAnchorName { get; set; }

        public bool IsSelected { get; set; }

        public bool SkipRender { get; set; }

        public string SortItemsDivID { get; set; }

        public bool HideSortItems { get; set; }

        public string DivID = Guid.NewGuid().ToString();

        public string GetSortLink(bool renderAllTabs)
        {
            var tabCssName = String.IsNullOrEmpty(TabAnchorName)
                                 ? (IsSelected ? "viewSwitcherTabSelected" : "viewSwitcherTab")
                                 : String.Format("{0} viewSwitcherTab_{1}",
                                                 IsSelected ? "viewSwitcherTabSelected" : "viewSwitcherTab",
                                                 TabAnchorName);

            var javascriptText = string.Format(@" onclick=""{0} {1} viewSwitcherToggleTabs(this.id);"" ",
                                               String.IsNullOrEmpty(OnClickText) ? String.Empty : OnClickText + ";",
                                               renderAllTabs && !String.IsNullOrEmpty(TabAnchorName)
                                                   ? String.Format("ASC.Controls.AnchorController.move('{0}');", TabAnchorName)
                                                   : "");

            var sb = new StringBuilder();
            sb.AppendFormat(@"<li id='{0}_ViewSwitcherTab' class='{1}' {2}>{3}</li>", DivID, tabCssName, javascriptText, TabName.HtmlEncode());
            return sb.ToString();
        }

        public void RenderTabContent(HtmlTextWriter writer)
        {
            if (SkipRender) return;
            writer.Write("<div id='{0}'{1}>", DivID, IsSelected ? string.Empty : " style='display: none;'");
            RenderControl(writer);
            writer.Write("</div>");
        }
    }
}