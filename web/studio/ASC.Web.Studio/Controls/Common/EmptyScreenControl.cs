/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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