/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
*/

using System;
using System.Linq;
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

        public override string ExtendedDescription
        {
            get { return string.Format(CommunityResource.ProductDescriptionExt, "<span style='display:none'>", "</span>"); }
        }

        public override string Description
        {
            get { return CommunityResource.ProductDescription; }
        }

        public override string StartURL
        {
            get { return "~/products/community"; }
        }

        public override string ProductClassName
        {
            get { return "community"; }
        }

        public override ProductContext Context
        {
            get { return ctx; }
        }


        public override void Init()
        {
            ctx = new ProductContext
            {
                MasterPageFile = "~/products/community/community.master",
                DisabledIconFileName = "product_disabled_logo.png",
                IconFileName = "product_logo.png",
                LargeIconFileName = "product_logolarge.png",
                DefaultSortOrder = 40,

                SubscriptionManager = new CommunitySubscriptionManager(),

                SpaceUsageStatManager = new CommunitySpaceUsageStatManager(),

                AdminOpportunities = () => CommunityResource.ProductAdminOpportunities.Split('|').ToList(),
                UserOpportunities = () => CommunityResource.ProductUserOpportunities.Split('|').ToList(),
            };
        }
    }
}
