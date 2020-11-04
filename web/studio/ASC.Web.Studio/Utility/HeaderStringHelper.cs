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
using ASC.Web.Core;
using Resources;

namespace ASC.Web.Studio.Utility
{
    public static class HeaderStringHelper
    {
        public static string GetHTMLSearchHeader(string searchString)
        {
            return String.Format("{0}: \"{1}\"", Resource.SearchResult, searchString.HtmlEncode());
        }

        public static string GetPageTitle(string pageTitle)
        {
            var productName = "";
            var product = WebItemManager.Instance[CommonLinkUtility.GetProductID()];
            if (product != null)
                productName = product.Name;

            productName = String.IsNullOrEmpty(productName) ? Resource.WebStudioName : productName;

            return
                string.IsNullOrEmpty(pageTitle)
                    ? productName
                    : String.Format("{0} - {1}", pageTitle, productName);
        }
    }
}