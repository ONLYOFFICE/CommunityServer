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
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Sample.Classes;
using ASC.Web.Sample.Resources;

namespace ASC.Web.Sample.Configuration
{
    public class SampleSearchHandler : BaseSearchHandlerEx
    {
        public override Guid ProductID
        {
            get { return ProductEntryPoint.Id; }
        }

        public override Guid ModuleID
        {
            get { return ProductID; }
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "common_search_icon.png" }; }
        }

        public override string SearchName
        {
            get { return SampleResource.Search; }
        }

        public override IItemControl Control
        {
            get { return new SearchResultsView(); }
        }

        public override SearchResultItem[] Search(string searchText)
        {
            //TODO: search by text implementation
            return new[]
                {
                    new SearchResultItem
                        {
                            Name = "SearchResultItem " + searchText + " Name",
                            Description = "SearchResultItem " + searchText + " Description",
                            URL = PathProvider.BaseAbsolutePath,
                            Date = DateTime.Now
                        }
                };
        }
    }
}