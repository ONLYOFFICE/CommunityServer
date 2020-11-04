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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ASC.Web.Studio.Core.Search;

namespace ASC.Web.Studio.UserControls.Common.Search
{
    public partial class SearchResults : UserControl
    {
        public static string Location
        {
            get { return "~/UserControls/Common/Search/SearchResults.ascx"; }
        }

        internal int MaxResultCount = 5;

        public IEnumerable<SearchResult> SearchResultsData { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.RegisterStyle("~/UserControls/Common/Search/css/searchresults.less")
                .RegisterBodyScripts("~/UserControls/Common/Search/js/searchresults.js");

            results.ItemDataBound += ResultsItemDataBound;
            results.DataSource = SearchResultsData;
            results.DataBind();
        }

        private static void ResultsItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var control = ((SearchResult)e.Item.DataItem).PresentationControl;
                if (control == null)
                    return;
                control.Items = ((SearchResult)e.Item.DataItem).Items;
                e.Item.FindControl("resultItems").Controls.Add(control);
            }
        }
    }
}