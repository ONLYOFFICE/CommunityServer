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
using ASC.Web.Community.Resources;
using ASC.Web.Core;

namespace ASC.Web.Community.Product
{
    public class CommunityProduct : Core.Product
    {
        private static ProductContext ctx;


        public static Guid ID
        {
            get { return WebItemManager.CommunityProductID; }
        }

        public override Guid ProductID
        {
            get { return ID; }
        }

        public override string Name
        {
            get { return CommunityResource.ProductName; }
        }

        public override string Description
        {
            get
            {
                var id = SecurityContext.CurrentAccount.ID;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupVisitor.ID))
                    return CommunityResource.ProductDescriptionShort;

                if (CoreContext.UserManager.IsUserInGroup(id, ASC.Core.Users.Constants.GroupAdmin.ID) || CoreContext.UserManager.IsUserInGroup(id, ID))
                    return CommunityResource.ProductDescriptionExt;

                return CommunityResource.ProductDescription;
            }
        }

        public override string StartURL
        {
            get { return "~/Products/Community/"; }
        }

        public override string HelpURL
        {
            get { return "~/Products/Community/Help.aspx"; }
        }

        public override string ProductClassName
        {
            get { return "community"; }
        }

        public override bool Visible { get { return true; } }

        public override ProductContext Context
        {
            get { return ctx; }
        }


        public override void Init()
        {
            ctx = new ProductContext
            {
                MasterPageFile = "~/Products/Community/Master/Community.Master",
                DisabledIconFileName = "product_disabled_logo.png",
                IconFileName = "product_logo.png",
                LargeIconFileName = "product_logolarge.svg",
                DefaultSortOrder = 40,

                SubscriptionManager = new CommunitySubscriptionManager(),

                SpaceUsageStatManager = new CommunitySpaceUsageStatManager(),

                AdminOpportunities = () => CommunityResource.ProductAdminOpportunities.Split('|').ToList(),
                UserOpportunities = () => CommunityResource.ProductUserOpportunities.Split('|').ToList(),
            };
        }
    }
}
