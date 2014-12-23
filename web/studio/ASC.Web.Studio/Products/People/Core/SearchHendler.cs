/*
 * 
 * (c) Copyright Ascensio System SIA 2010-2014
 * 
 * This program is a free software product.
 * You can redistribute it and/or modify it under the terms of the GNU Affero General Public License
 * (AGPL) version 3 as published by the Free Software Foundation. 
 * In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended to the effect 
 * that Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 * 
 * This program is distributed WITHOUT ANY WARRANTY; 
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 * For details, see the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
 * 
 * You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
 * 
 * The interactive user interfaces in modified source and object code versions of the Program 
 * must display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 * 
 * Pursuant to Section 7(b) of the License you must retain the original Product logo when distributing the program. 
 * Pursuant to Section 7(e) we decline to grant you any rights under trademark law for use of our trademarks.
 * 
 * All the Product's GUI elements, including illustrations and icon sets, as well as technical 
 * writing content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0 International. 
 * See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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