/*
 *
 * (c) Copyright Ascensio System Limited 2010-2020
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