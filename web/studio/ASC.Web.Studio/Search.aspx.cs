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


using AjaxPro;
using ASC.Web.Core;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Core.Search;
using ASC.Web.Studio.UserControls.Common.Search;
using ASC.Web.Studio.Utility;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;

namespace ASC.Web.Studio
{
    [AjaxNamespace("SearchController")]
    public partial class Search : MainPage
    {
        protected string SearchText;

        protected void Page_Load(object sender, EventArgs e)
        {
            AjaxPro.Utility.RegisterTypeForAjax(GetType());

            Master.DisabledSidePanel = true;

            Title = HeaderStringHelper.GetPageTitle(Resource.Search);

            var productID = !string.IsNullOrEmpty(Request["productID"]) ? new Guid(Request["productID"]) : Guid.Empty;
            var moduleID = !string.IsNullOrEmpty(Request["moduleID"]) ? new Guid(Request["moduleID"]) : Guid.Empty;

            SearchText = (Request["search"] ?? "").Trim();

            var searchResultsData = new List<SearchResult>();
            if (!string.IsNullOrEmpty(SearchText))
            {
                List<ISearchHandlerEx> handlers = null;

                var products = !string.IsNullOrEmpty(Request["products"]) ? Request["products"] : string.Empty;
                if (!string.IsNullOrEmpty(products))
                {
                    try
                    {
                        var productsStr = products.Split(new[] { ',' });
                        var productsGuid = productsStr.Select(p => new Guid(p)).ToArray();

                        handlers = SearchHandlerManager.GetHandlersExForProductModule(productsGuid);
                    }
                    catch(Exception err)
                    {
                        Log.Error(err);
                    }
                }

                if (handlers == null)
                {
                    handlers = SearchHandlerManager.GetHandlersExForProductModule(productID, moduleID);
                }

                searchResultsData = GetSearchresultByHandlers(handlers, SearchText);
            }

            if (searchResultsData.Count <= 0)
            {
                var emptyScreenControl = new EmptyScreenControl
                    {
                        ImgSrc = WebImageSupplier.GetAbsoluteWebPath("empty_search.png"),
                        Header = Resource.SearchNotFoundMessage,
                        Describe = Resource.SearchNotFoundDescript
                    };
                SearchContent.Controls.Add(emptyScreenControl);
            }
            else
            {
                searchResultsData = GroupSearchresult(productID, searchResultsData);
                var oSearchView = (SearchResults)LoadControl(SearchResults.Location);
                oSearchView.SearchResultsData = searchResultsData;
                SearchContent.Controls.Add(oSearchView);
            }
        }

        private List<SearchResult> GroupSearchresult(Guid productID, List<SearchResult> searchResult)
        {
            if (!productID.Equals(Guid.Empty) && productID != WebItemManager.CommunityProductID) return searchResult;

            var groupedData = GroupDataModules(searchResult);

            groupedData.Sort(new SearchComparer());

            return groupedData;
        }

        private List<SearchResult> GroupDataModules(List<SearchResult> data)
        {
            var guids = data.Select(searchResult => searchResult.ProductID).ToList();

            if (!guids.Any())
                return data;

            guids = guids.Distinct().ToList();

            var groupedData = new List<SearchResult>();
            foreach (var productID in guids)
            {
                foreach (var searchResult in data)
                {
                    if (searchResult.ProductID != productID) continue;

                    var item = GetContainer(groupedData, productID);
                    foreach (var searchResultItem in searchResult.Items)
                    {
                        if (searchResultItem.Additional == null)
                            searchResultItem.Additional = new Dictionary<string, object>();

                        if (!searchResultItem.Additional.ContainsKey("imageRef"))
                            searchResultItem.Additional.Add("imageRef", searchResult.LogoURL);
                        if (!searchResultItem.Additional.ContainsKey("Hint"))
                            searchResultItem.Additional.Add("Hint", searchResult.Name);
                    }
                    item.Items.AddRange(searchResult.Items);

                    if (item.PresentationControl == null) item.PresentationControl = searchResult.PresentationControl;
                    if (string.IsNullOrEmpty(item.LogoURL)) item.LogoURL = searchResult.LogoURL;
                    if (string.IsNullOrEmpty(item.Name)) item.Name = searchResult.Name;
                }
            }

            return groupedData;
        }

        private SearchResult GetContainer(ICollection<SearchResult> newData, Guid productID)
        {
            foreach (var searchResult in newData.Where(searchResult => searchResult.ProductID == productID))
                return searchResult;

            var newResult = CreateCertainContainer(productID);
            newData.Add(newResult);
            return newResult;
        }

        private SearchResult CreateCertainContainer(Guid productID)
        {
            var certainProduct = WebItemManager.Instance[productID];
            var container = new SearchResult
                {
                    ProductID = productID,
                    Name = (certainProduct != null) ? certainProduct.Name : string.Empty,
                    LogoURL = (certainProduct != null) ? certainProduct.GetIconAbsoluteURL() : string.Empty
                };

            if (productID == WebItemManager.CommunityProductID || productID == Guid.Empty)
                container.PresentationControl = new CommonResultsView { MaxCount = 7, Text = SearchText };

            return container;
        }

        private static List<SearchResult> GetSearchresultByHandlers(IEnumerable<ISearchHandlerEx> handlers, string searchText)
        {
            var searchResults = new List<SearchResult>();
            foreach (var sh in handlers)
            {
                var module = WebItemManager.Instance[sh.ModuleID];
                if (module != null && module.IsDisabled())
                    continue;

                var items = sh.Search(searchText).OrderByDescending(item => item.Date).ToArray();

                if (items.Length == 0)
                    continue;

                var searchResult = new SearchResult
                    {
                        ProductID = sh.ProductID,
                        PresentationControl = (ItemSearchControl)sh.Control,
                        Name = module != null ? module.Name : sh.SearchName,
                        LogoURL = module != null
                                      ? module.GetIconAbsoluteURL()
                                      : WebImageSupplier.GetAbsoluteWebPath(sh.Logo.ImageFileName, sh.Logo.PartID)
                    };

                searchResult.PresentationControl.Text = searchText;
                searchResult.PresentationControl.MaxCount = 7;
                searchResult.Items.AddRange(items);

                searchResults.Add(searchResult);
            }
            return searchResults;
        }

        [AjaxMethod]
        public string GetAllData(string product, string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var productID = new Guid(product);

            var handlers = SearchHandlerManager.GetHandlersExForProductModule(productID, Guid.Empty);
            var searchResultsData = GetSearchresultByHandlers(handlers, text);

            searchResultsData = GroupSearchresult(productID, searchResultsData);
            if (searchResultsData.Count <= 0) return string.Empty;

            var control = searchResultsData[0].PresentationControl ?? new CommonResultsView();

            control.Items = new List<SearchResultItem>();
            foreach (var searchResult in searchResultsData)
            {
                control.Items.AddRange(searchResult.Items);
            }
            control.MaxCount = int.MaxValue;
            control.Text = control.Text ?? text;
            var stringWriter = new StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            control.RenderControl(htmlWriter);

            return stringWriter.ToString();
        }
    }
}