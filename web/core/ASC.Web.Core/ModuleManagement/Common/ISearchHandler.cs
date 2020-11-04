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
