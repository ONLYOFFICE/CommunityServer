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
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core.ModuleManagement.Common
{
    public class ItemSearchControl : WebControl, IItemControl
    {
        public List<SearchResultItem> Items { get; set; }

        public string Text { get; set; }

        public int MaxCount { get; set; }

        public string SpanClass { get; set; }


        public ItemSearchControl()
            : base(HtmlTextWriterTag.Div)
        {
            MaxCount = 5;
            SpanClass = "describe-text";
        }

        public virtual void RenderContent(HtmlTextWriter writer)
        {
            base.RenderContents(writer);
        }

        /// <summary>
        /// This method needs to keep item height
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string CheckEmptyValue(string value)
        {
            return String.IsNullOrEmpty(value) ? "&nbsp;" : value;
        }
    }

    public interface IItemControl
    {
        List<SearchResultItem> Items { get; set; }

        string Text { get; set; }
    }


    public class SearchResultItem
    {
        /// <summary>
        /// Absolute URL
        /// </summary>
        public string URL { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime? Date { get; set; }
        public Dictionary<string, object> Additional { get; set; }
    }

    public interface ISearchHandlerEx
    {
        Guid ProductID { get; }

        Guid ModuleID { get; }

        /// <summary>
        /// Interface log 
        /// </summary>
        ImageOptions Logo { get; }

        /// <summary>
        /// Search display name
        /// <remarks>Ex: "forum search"</remarks>
        /// </summary>
        string SearchName { get; }

        IItemControl Control { get; }

        /// <summary>
        /// Do search
        /// </summary>
        /// <param name="text">Search text</param>
        /// <returns>If nothing found - empty array</returns>
        SearchResultItem[] Search(string text);
    }
}
