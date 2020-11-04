/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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