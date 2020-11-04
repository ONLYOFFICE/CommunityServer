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
using System.Web.UI.WebControls;

namespace ASC.Web.Studio.Controls.Common
{
    public class EmptyScreenControl : WebControl
    {
        public string ImgSrc { get; set; }

        public string Header { get; set; }

        public string HeaderDescribe { get; set; }

        public string Describe { get; set; }

        public string ButtonHTML { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            var html = new StringBuilder();

            html.AppendFormat("<div id='{0}' class='noContentBlock emptyScrCtrl {1}' >", ID, !String.IsNullOrEmpty(CssClass)? CssClass : String.Empty);
            if (!String.IsNullOrEmpty(ImgSrc))
            {
                html.AppendFormat("<table><tr><td><div style=\"background-image: url('{0}');\" class=\"emptyScrImage\" ></div></td>", ImgSrc)
                    .Append("<td><div class='emptyScrTd' >");
            }
            if (!String.IsNullOrEmpty(Header))
            {
                html.AppendFormat("<div class='header-base-big' >{0}</div>", Header);
            }
            if (!String.IsNullOrEmpty(HeaderDescribe))
            {
                html.AppendFormat("<div class='emptyScrHeadDscr' >{0}</div>", HeaderDescribe);
            }
            if (!String.IsNullOrEmpty(Describe))
            {
                html.AppendFormat("<div class='emptyScrDscr' >{0}</div>", Describe);
            }
            if (!String.IsNullOrEmpty(ButtonHTML))
            {
                html.AppendFormat("<div class='emptyScrBttnPnl' >{0}</div>", ButtonHTML);
            }
            if (!String.IsNullOrEmpty(ImgSrc))
            {
                html.Append("</div></td></tr></table>");
            }

            html.Append("</div>");

            writer.WriteLine(html.ToString());
        }
    }
}