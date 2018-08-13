/*
 *
 * (c) Copyright Ascensio System Limited 2010-2018
 *
 * This program is freeware. You can redistribute it and/or modify it under the terms of the GNU 
 * General Public License (GPL) version 3 as published by the Free Software Foundation (https://www.gnu.org/copyleft/gpl.html). 
 * In accordance with Section 7(a) of the GNU GPL its Section 15 shall be amended to the effect that 
 * Ascensio System SIA expressly excludes the warranty of non-infringement of any third-party rights.
 *
 * THIS PROGRAM IS DISTRIBUTED WITHOUT ANY WARRANTY; WITHOUT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY OR
 * FITNESS FOR A PARTICULAR PURPOSE. For more details, see GNU GPL at https://www.gnu.org/copyleft/gpl.html
 *
 * You can contact Ascensio System SIA by email at sales@onlyoffice.com
 *
 * The interactive user interfaces in modified source and object code versions of ONLYOFFICE must display 
 * Appropriate Legal Notices, as required under Section 5 of the GNU GPL version 3.
 *
 * Pursuant to Section 7 § 3(b) of the GNU GPL you must retain the original ONLYOFFICE logo which contains 
 * relevant author attributions when distributing the software. If the display of the logo in its graphic 
 * form is not reasonably feasible for technical reasons, you must include the words "Powered by ONLYOFFICE" 
 * in every copy of the program you distribute. 
 * Pursuant to Section 7 § 3(e) we decline to grant you any rights under trademark law for use of our trademarks.
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