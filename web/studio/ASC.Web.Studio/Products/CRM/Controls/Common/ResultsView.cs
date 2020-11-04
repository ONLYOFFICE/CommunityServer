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
using System.Web.UI;
using ASC.Common.Utils;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.CRM.Resources;

namespace ASC.Web.CRM.Controls.Common
{
    public sealed class ResultsView : ItemSearchControl
    {

        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "tableBase");
            writer.AddAttribute("cellspacing", "0");
            writer.AddAttribute("cellpadding", "8");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

            foreach (var searchItemResult in Items.GetRange(0, (MaxCount < Items.Count) ? MaxCount : Items.Count))
            {
                var relativeInfo = searchItemResult.Additional["relativeInfo"].ToString();

                if (String.IsNullOrEmpty(relativeInfo))
                    relativeInfo = searchItemResult.Description.HtmlEncode();
                else
                    relativeInfo = String.Format("<span class='describe-text'>{0}</span> {1}", CRMCommonResource.RelativeTo, relativeInfo.HtmlEncode());

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "search-result-item");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase left-column gray-text");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, searchItemResult.Additional["typeInfo"].ToString());
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(searchItemResult.Additional["typeInfo"].ToString());
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase center-column");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                if (!string.IsNullOrEmpty(searchItemResult.URL))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Href, searchItemResult.URL);
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "link bold");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write(HtmlUtil.SearchTextHighlight(Text, searchItemResult.Name.HtmlEncode()));
                    writer.RenderEndTag();
                }
                else
                {
                    writer.Write(HtmlUtil.SearchTextHighlight(Text, searchItemResult.Name.HtmlEncode(), true));
                }
                if (!String.IsNullOrEmpty(relativeInfo))
                {
                    writer.WriteBreak();
                    writer.Write(relativeInfo);
                }

                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase right-column gray-text");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (searchItemResult.Date.HasValue)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, searchItemResult.Date.Value.ToShortDateString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(searchItemResult.Date.Value.ToShortDateString());
                    writer.RenderEndTag();
                }
                writer.RenderEndTag();

                writer.RenderEndTag();
            }

            writer.RenderEndTag();
            writer.RenderEndTag();
        }
    }
}