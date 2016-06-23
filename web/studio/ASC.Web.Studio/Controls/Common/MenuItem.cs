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