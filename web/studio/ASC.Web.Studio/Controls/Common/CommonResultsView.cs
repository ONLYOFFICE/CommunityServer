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

namespace ASC.Web.Studio.Controls.Common
{
    public sealed class CommonResultsView : ItemSearchControl
    {
        protected override void RenderContents(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Class, "tableBase");
            writer.AddAttribute("cellspacing", "0");
            writer.AddAttribute("cellpadding", "8");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tbody);

            foreach (var srGroup in Items.GetRange(0, (MaxCount < Items.Count) ? MaxCount : Items.Count))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "search-result-item");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                if (srGroup.Additional != null && srGroup.Additional.ContainsKey("imageRef"))
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase left-column gray-text");
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, srGroup.Additional["Hint"].ToString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(srGroup.Additional["Hint"].ToString());
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    if (srGroup.Additional.ContainsKey("showIcon"))
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase");
                        writer.RenderBeginTag(HtmlTextWriterTag.Td);
                        writer.AddAttribute(HtmlTextWriterAttribute.Src, srGroup.Additional["imageRef"].ToString());
                        writer.RenderBeginTag(HtmlTextWriterTag.Img);
                        writer.RenderEndTag();
                        writer.RenderEndTag();
                    }
                }

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase center-column");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.AddAttribute(HtmlTextWriterAttribute.Href, srGroup.URL);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "link bold");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(HtmlUtil.SearchTextHighlight(Text, srGroup.Name.HtmlEncode()));
                writer.RenderEndTag();

                if (!String.IsNullOrEmpty(srGroup.Description))
                {
                    writer.WriteBreak();
                    if (!string.IsNullOrEmpty(SpanClass))
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, SpanClass);
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(CheckEmptyValue(srGroup.Description.HtmlEncode()));
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase right-column gray-text");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (srGroup.Date.HasValue)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, srGroup.Date.Value.ToShortDateString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(srGroup.Date.Value.ToShortDateString());
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