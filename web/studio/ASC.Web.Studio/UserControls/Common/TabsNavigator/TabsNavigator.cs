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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Linq;

namespace ASC.Web.Studio.UserControls.Common.TabsNavigator
{
    [
        ToolboxData("<{0}:TabsNavigator runat=\"server\"/>"),
        ParseChildren(ChildrenAsProperties = true), PersistChildren(true)
    ]
    public class TabsNavigator : Control
    {
        #region Properties

        public string BlockID { get; set; }


        private List<TabsNavigatorItem> _tabItems;

        [Description("List of tabs."),
         Category("Data"), PersistenceMode(PersistenceMode.InnerProperty),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Content), NotifyParentProperty(true)
        ]
        public List<TabsNavigatorItem> TabItems
        {
            get { return _tabItems ?? (_tabItems = new List<TabsNavigatorItem>()); }
            set { _tabItems = value; }
        }

        protected bool HasTabItems
        {
            get { return TabItems.Count > 0; }
        }

        #endregion

        protected override void OnLoad(EventArgs e)
        {
            foreach (var tab in TabItems.Where(tab => tab.Visible))
            {
                Controls.Add(tab);
            }
            base.OnInit(e);
            InitScripts();
        }

        protected void InitScripts()
        {
            Page.RegisterBodyScripts(ResolveUrl("~/usercontrols/common/tabsnavigator/js/tabsnavigator.js"));
        }

        protected override void Render(HtmlTextWriter writer)
        {
            var sb = new StringBuilder();

            sb.Append("<div class=\"clearFix\">");
            sb.Append("  <div id=\"" + (String.IsNullOrEmpty(BlockID) ? ClientID : BlockID) + "\" class=\"tabsNavigationLinkBox\">");

            if (HasTabItems)
            {
                var visibleTabItems = TabItems.Where(tab => tab.Visible).ToList();
                var visibleTabsCount = visibleTabItems.Count;

                for (var i = 0; i < visibleTabsCount; i++)
                {
                    sb.Append(visibleTabItems[i].GetTabLink(i == visibleTabsCount - 1));
                }
            }

            sb.Append("  </div>");
            sb.Append("</div>");

            writer.Write(sb.ToString());

            foreach (var tab in TabItems.Where(tab => tab.Visible && string.IsNullOrEmpty(tab.TabHref) && !tab.SkipRender))
            {
                tab.RenderTabContent(writer);
            }
        }
    }
}