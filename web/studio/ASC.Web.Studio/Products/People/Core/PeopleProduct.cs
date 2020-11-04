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
using System.Linq;
using ASC.Core;
using ASC.Web.Core;
using ASC.Web.Core.Utility;
using ASC.Web.People.Resources;

namespace ASC.Web.People.Core
{
    public class PeopleProduct : Product
    {
        internal const string ProductPath = "~/Products/People/";

        private ProductContext _context;

        public static Guid ID
        {
            get { return new Guid("{F4D98AFD-D336-4332-8778-3C6945C81EA0}"); }
        }

        public override bool Visible { get { return true; } }

        public override ProductContext Context
        {
            get { return _context; }
        }

        public override string Name
        {
            get { return PeopleResource.ProductName; }
        }

        public override string Description
        {
            get
            {
                var id = SecurityContext.CurrentAccount.ID;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupAdmin.ID) || CoreContext.UserManager.IsUserInGroup(id, ID))
                    return PeopleResource.ProductDescriptionEx;

                return PeopleResource.ProductDescription;
            }
        }

        public override Guid ProductID
        {
            get { return ID; }
        }

        public override string StartURL
        {
            get { return ProductPath; }
        }

        public override string HelpURL
        {
            get { return string.Concat(ProductPath, "Help.aspx"); }
        }

        public static string GetStartURL()
        {
            return ProductPath;
        }

        public override string ProductClassName
        {
            get { return "people"; }
        }

        public override void Init()
        {
            _context = new ProductContext
                {
                    MasterPageFile = "~/Products/People/PeopleBaseTemplate.Master",
                    DisabledIconFileName = "product_disabled_logo.png",
                    IconFileName = "product_logo.png",
                    LargeIconFileName = "product_logolarge.svg",
                    DefaultSortOrder = 50,
                    AdminOpportunities = () => PeopleResource.ProductAdminOpportunities.Split('|').ToList(),
                    UserOpportunities = () => PeopleResource.ProductUserOpportunities.Split('|').ToList()
                };

            SearchHandlerManager.Registry(new SearchHandler());
        }
    }
}