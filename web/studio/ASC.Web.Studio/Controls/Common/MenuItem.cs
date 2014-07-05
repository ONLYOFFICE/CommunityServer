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
using System.Web.UI;

namespace ASC.Web.Studio.Controls.Common
{
    [ToolboxData("<{0}:MenuItem runat=server />"),
     DefaultProperty("Name"), PersistChildren(false)]
    public class MenuItem : Control
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageURL { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write("<div style='margin-top:10px;'>");
            RenderContents(writer);
            writer.Write("</div>");
        }

        protected virtual void RenderContents(HtmlTextWriter writer)
        {
            writer.Write(Name.HtmlEncode());
        }

    }

    [ToolboxData("<{0}:HtmlMenuItem runat=server />")]
    public class HtmlMenuItem : MenuItem
    {
        public HtmlMenuItem(string html)
        {
            Html = html;
        }

        public string Html { get; set; }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (String.IsNullOrEmpty(Html))
                base.RenderContents(writer);
            else
                writer.Write(Html);
        }
    }

    [ToolboxData("<{0}:NavigationItem runat=server />")]
    public class NavigationItem : MenuItem
    {
        public string URL { get; set; }

        public bool Selected { get; set; }

        public bool RightAlign { get; set; }

        public List<NavigationItem> SubItems { get; set; }

        public int Width { get; set; }

        public string ModuleStatusIconFileName { get; set; }

        public string LinkId { get; set; }

        public NavigationItem()
        {
            RightAlign = false;
            SubItems = new List<NavigationItem>();
        }

        public NavigationItem(string name, string url)
            : this()
        {
            Name = name;
            URL = url;
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            if (!String.IsNullOrEmpty(Name) && !String.IsNullOrEmpty(URL))
            {
                writer.Write(@"<a href=""{0}"" title=""{1}"" class=""linkAction"" {2}>{3}</a>",
                             ResolveUrl(URL),
                             Description.HtmlEncode(),
                             string.IsNullOrEmpty(LinkId) ? string.Empty : string.Format(@"id=""{0}""", LinkId),
                             Name.HtmlEncode()
                    );
            }
            else
                base.RenderContents(writer);
        }
    }
}