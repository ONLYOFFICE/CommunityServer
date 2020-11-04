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
    public class ResultsView : ItemSearchControl
    {
        protected override void RenderContents(HtmlTextWriter writer)
        {
            foreach (var srGroup in Items.GetRange(0, (MaxCount < Items.Count) ? MaxCount : Items.Count))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "clearFix item");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);

                writer.AddAttribute(HtmlTextWriterAttribute.Class, String.IsNullOrEmpty(srGroup.Description) ? "widebody" : "body");
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
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

                if (srGroup.Date.HasValue)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "date");
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);

                    writer.AddAttribute(HtmlTextWriterAttribute.Style, "height:100%");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);

                    writer.Write(srGroup.Date.Value.ToShortDateString());

                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    writer.RenderEndTag();
                }
                writer.RenderEndTag();
            }
        }
    }
}