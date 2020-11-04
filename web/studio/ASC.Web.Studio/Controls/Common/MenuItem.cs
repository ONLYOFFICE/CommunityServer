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