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
using System.ComponentModel;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ASC.Web.Studio.Controls.Common
{
    /// <summary>
    /// Base side container
    /// </summary>   
    [ToolboxData("<{0}:SideContainer runat=server></{0}:SideContainer>")]
    public class SideContainer : PlaceHolder
    {
        [Category("Title"), PersistenceMode(PersistenceMode.Attribute)]
        public string Title { get; set; }

        [Category("Title"), PersistenceMode(PersistenceMode.Attribute)]
        public string ImageURL { get; set; }

        [Category("Style"), PersistenceMode(PersistenceMode.Attribute)]
        public string HeaderCSSClass { get; set; }

        [Category("Style"), PersistenceMode(PersistenceMode.Attribute)]
        public string BodyCSSClass { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='studioSideBox'>");

            sb.Append("<div class='studioSideBoxContent'>");

            //header
            sb.Append("<div class='" + (String.IsNullOrEmpty(HeaderCSSClass) ? "studioSideBoxHeader" : HeaderCSSClass) + "'>");
            if (!String.IsNullOrEmpty(ImageURL))
                sb.Append("<img alt='' style='margin-right:8px;' align='absmiddle' src='" + ImageURL + "'/>");

            sb.Append((Title ?? "").HtmlEncode());
            sb.Append("</div>");

            sb.Append("<div class='" + (String.IsNullOrEmpty(BodyCSSClass) ? "studioSideBoxBody" : BodyCSSClass) + "'>");

            writer.Write(sb.ToString());
            base.Render(writer);


            sb = new StringBuilder();
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append("</div>");
            writer.Write(sb.ToString());
        }
    }
}