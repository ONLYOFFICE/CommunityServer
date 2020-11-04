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
using ASC.Web.Core.ModuleManagement.Common;

namespace ASC.Web.Studio.Core.Search
{
    public class SearchResult
    {
        public string LogoURL { get; set; }
        public string Name { get; set; }
        public Guid ProductID { get; set; }
        public List<SearchResultItem> Items { get; set; }

        public ItemSearchControl PresentationControl { get; set; }

        public SearchResult()
        {
            Items = new List<SearchResultItem>();
        }
    }
}