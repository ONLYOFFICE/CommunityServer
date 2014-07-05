/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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