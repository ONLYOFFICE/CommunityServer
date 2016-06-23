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
using System.Web;
using System.Web.UI;
using ASC.Common.Utils;
using ASC.Projects.Core.Domain;
using ASC.Web.Core.ModuleManagement.Common;

namespace ASC.Web.Projects.Classes
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

            foreach (var srGroup in Items.GetRange(0, (MaxCount < Items.Count) ? MaxCount : Items.Count))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "search-result-item");
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase left-column gray-text");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                writer.AddAttribute(HtmlTextWriterAttribute.Title, srGroup.Additional["Hint"].ToString());
                writer.RenderBeginTag(HtmlTextWriterTag.Div);
                writer.Write(srGroup.Additional["Hint"].ToString());
                writer.RenderEndTag();
                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase center-column");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);

                writer.AddAttribute(HtmlTextWriterAttribute.Href, srGroup.URL);
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "link bold");
                writer.RenderBeginTag(HtmlTextWriterTag.A);
                writer.Write(HtmlUtil.SearchTextHighlight(Text, srGroup.Name.HtmlEncode()));
                writer.RenderEndTag();

                if ((EntityType)(Enum.Parse(typeof(EntityType), (srGroup.Additional["Type"]).ToString())) == EntityType.Project)
                {
                    if (!string.IsNullOrEmpty(srGroup.Description))
                    {
                        writer.WriteBreak();
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "describe-text");
                        writer.RenderBeginTag(HtmlTextWriterTag.Span);
                        writer.Write(CheckEmptyValue(HttpUtility.HtmlEncode(HtmlUtil.GetText(srGroup.Description, 100))));
                        writer.RenderEndTag();
                    }
                }
                else
                {
                    writer.WriteBreak();
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "describe-text");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(srGroup.Additional["ContainerTitle"].ToString());
                    writer.RenderEndTag();
                    writer.Write("&nbsp;");

                    writer.AddAttribute(HtmlTextWriterAttribute.Href, srGroup.Additional["ContainerPath"].ToString());
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "link");
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write(srGroup.Additional["ContainerValue"].ToString().HtmlEncode());
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();

                writer.AddAttribute(HtmlTextWriterAttribute.Class, "borderBase right-column gray-text");
                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                if (srGroup.Date.HasValue)
                {
                    var srGroupDate = srGroup.Date.Value;
                    writer.AddAttribute(HtmlTextWriterAttribute.Title, srGroupDate.ToShortDateString());
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                    writer.Write(srGroupDate.ToShortDateString());
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