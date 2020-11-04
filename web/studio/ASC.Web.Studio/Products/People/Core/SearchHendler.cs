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
using System.Linq;
using ASC.Core;
using ASC.Core.Users;
using ASC.Web.Core.ModuleManagement.Common;
using ASC.Web.Core.Utility;
using ASC.Web.Core.Utility.Skins;
using ASC.Web.Studio.Controls.Common;
using ASC.Web.Studio.Utility;
using Resources;

namespace ASC.Web.People.Core
{
    public class SearchHandler : BaseSearchHandlerEx
    {
        public override Guid ProductID
        {
            get { return PeopleProduct.ID; }
        }

        public override Guid ModuleID
        {
            get { return ProductID; }
        }

        public override ImageOptions Logo
        {
            get { return new ImageOptions { ImageFileName = "product_search_icon.png", PartID = ProductID }; }
        }

        public override string SearchName
        {
            get { return Resource.EmployeesSearch; }
        }

        public override IItemControl Control
        {
            get { return new CommonResultsView(); }
        }

        public override SearchResultItem[] Search(string text)
        {
            var users = new List<UserInfo>();

            users.AddRange(CoreContext.UserManager.Search(text, EmployeeStatus.Active));

            return users.Select(user =>
                                    {
                                        var description = string.Empty;
                                        if (!string.IsNullOrEmpty(user.Title))
                                            description = user.Title;

                                        var department = string.Join(", ", CoreContext.UserManager.GetUserGroups(user.ID).Select(d => d.Name).ToArray());
                                        if (!string.IsNullOrEmpty(department))
                                        {
                                            if (!string.IsNullOrEmpty(description))
                                                description += ", ";
                                            description += department;
                                        }

                                        return new SearchResultItem
                                            {
                                                Name = user.DisplayUserName(false),
                                                Description = description,
                                                URL = CommonLinkUtility.GetUserProfile(user.ID),
                                                Date = user.WorkFromDate,
                                                Additional = new Dictionary<string, object> { { "imageRef", user.GetSmallPhotoURL() }, { "showIcon", true }, { "Hint", Resources.PeopleResource.ProductName } }
                                            };
                                    }).ToArray();
        }
    }
}