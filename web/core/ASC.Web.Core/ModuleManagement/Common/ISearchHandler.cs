/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
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
