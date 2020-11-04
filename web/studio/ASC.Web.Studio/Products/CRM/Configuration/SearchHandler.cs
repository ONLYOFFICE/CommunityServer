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
using ASC.CRM.Core.Dao;
using ASC.Web.CRM.Classes;
using ASC.Web.CRM.Controls.Common;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.CRM.Core;
using ASC.Web.CRM.Resources;
using Autofac;

namespace ASC.Web.CRM.Configuration
{
    public class SearchHandler : BaseSearchHandlerEx
    {
        public override Guid ProductID
        {
            get { return ProductEntryPoint.ID; }
        }

        public override Guid ModuleID
        {
            get { return ProductID; }
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "search_in_module.png", PartID = ProductID }; }
        }

        public override string SearchName
        {
            get { return CRMContactResource.Search; }
        }

        public override IItemControl Control
        {
            get { return new ResultsView(); }
        }

        public override SearchResultItem[] Search(string searchText)
        {
            using (var scope = DIHelper.Resolve())
            {
                return scope.Resolve<DaoFactory>().SearchDao.Search(searchText);
            }
        }
    }
}