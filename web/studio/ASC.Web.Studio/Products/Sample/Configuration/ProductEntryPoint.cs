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
using ASC.Web.Sample.Classes;
using ASC.Web.Sample.Masters.ClientScripts;
using ASC.Web.Sample.Resources;
using ASC.Web.Studio.Utility;

namespace ASC.Web.Sample.Configuration
{
    public class ProductEntryPoint : Product
    {
        public static readonly Guid Id = new Guid("{314B5C27-631B-4C6C-8B11-C6400491ABEF}");

        private ProductContext context;

        public override Guid ProductID
        {
            get { return Id; }
        }

        public override string Name
        {
            get { return SampleResource.ProductName; }
        }

        public override string Description
        {
            get
            {
                var id = SecurityContext.CurrentAccount.ID;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupAdmin.ID) || CoreContext.UserManager.IsUserInGroup(id, Id))
                    return SampleResource.ExtendedProductDescription;

                return SampleResource.ProductDescription;
            }
        }

        public override string StartURL
        {
            get { return PathProvider.BaseVirtualPath; }
        }

        public override string HelpURL
        {
            get { return string.Concat(PathProvider.BaseVirtualPath, "Help.aspx"); }
        }

        public override string ProductClassName
        {
            get { return "sample"; }
        }

        public override bool Visible
        {
            get { return !TenantExtra.Saas && !CoreContext.Configuration.CustomMode; }
        }

        public override ProductContext Context
        {
            get { return context; }
        }

        public string ModuleSysName { get; set; }


        public override void Init()
        {
            context = new ProductContext
                {
                    MasterPageFile = String.Concat(PathProvider.BaseVirtualPath, "Masters/BasicTemplate.Master"),
                    DisabledIconFileName = "product_logo_disabled.png",
                    IconFileName = "product_logo.png",
                    LargeIconFileName = "product_logo_large.svg",
                    DefaultSortOrder = 100,
                    SubscriptionManager = null,
                    SpaceUsageStatManager = null,
                    AdminOpportunities = () => SampleResource.ProductAdminOpportunities.Split('|').ToList(),
                    UserOpportunities = () => SampleResource.ProductUserOpportunities.Split('|').ToList(),
                };

            SearchHandlerManager.Registry(new SampleSearchHandler());

            ClientScriptLocalization = new ClientLocalizationResources();
        }
    }
}