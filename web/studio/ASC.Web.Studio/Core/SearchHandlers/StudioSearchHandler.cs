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

namespace ASC.Web.Studio.Core.SearchHandlers
{
    public class StudioSearchHandler : BaseSearchHandlerEx
    {
        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "common_search_icon.png", PartID = Guid.Empty }; }
        }

        public override string SearchName
        {
            get { return Resources.Resource.Search; }
        }

        public override SearchResultItem[] Search(string text)
        {
            return new SearchResultItem[0];
        }
    }
}