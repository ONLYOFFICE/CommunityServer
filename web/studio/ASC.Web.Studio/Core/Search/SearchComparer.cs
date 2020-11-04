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


using System.Collections.Generic;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Studio.Core.Users;
using Resources;

namespace ASC.Web.Studio.Core.Search
{
    public sealed class SearchComparer : IComparer<SearchResult>
    {
        public int Compare(SearchResult x, SearchResult y)
        {
            if (x.Name == y.Name)
                return 0;
            if(y.Name == CustomNamingPeople.Substitute<Resource>("Employees"))
                return 1;
            return -1;
        }
    }

    public sealed class DateSearchComparer : IComparer<SearchResultItem>
    {
        public int Compare(SearchResultItem x, SearchResultItem y)
        {
            if (x.Date == y.Date)
                return 0;
            if (x.Date > y.Date)
                return -1;
            return 1;
        }
    }
}
