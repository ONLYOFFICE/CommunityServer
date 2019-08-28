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
                MasterPageFile = "~/Products/Community/Master/Community.master",
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
