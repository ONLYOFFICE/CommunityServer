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

                writer.WriteBreak();

                if ((EntityType)(Enum.Parse(typeof(EntityType), (srGroup.Additional["Type"]).ToString())) == EntityType.Project)
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "describe-text");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(CheckEmptyValue(HttpUtility.HtmlEncode(HtmlUtil.GetText(srGroup.Description, 100))));
                    writer.RenderEndTag();
                }
                else
                {
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "describe-text");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(Resources.ProjectsCommonResource.InProject);
                    writer.RenderEndTag();
                    writer.Write("&nbsp;");
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                    writer.Write(srGroup.Additional["ProjectName"].ToString().HtmlEncode());
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