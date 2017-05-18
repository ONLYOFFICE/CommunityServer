/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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
using ASC.Web.Core.Utility.Skins;

namespace ASC.Web.Core
{
    public static class WebItemExtension
    {
        public static string GetSysName(this IWebItem webitem)
        {
            if (string.IsNullOrEmpty(webitem.StartURL)) return string.Empty;

            var sysname = string.Empty;
            var parts = webitem.StartURL.ToLower().Split('/', '\\').ToList();

            var index = parts.FindIndex(s => "products".Equals(s));
            if (0 <= index && index < parts.Count - 1)
            {
                sysname = parts[index + 1];
                index = parts.FindIndex(s => "modules".Equals(s));
                if (0 <= index && index < parts.Count - 1)
                {
                    sysname += "-" + parts[index + 1];
                }
                else if (index == parts.Count - 1)
                {
                    sysname = parts[index].Split('.')[0];
                }
                return sysname;
            }

            index = parts.FindIndex(s => "addons".Equals(s));
            if (0 <= index && index < parts.Count - 1)
            {
                sysname = parts[index + 1];
            }

            return sysname;
        }

        public static string GetDisabledIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.DisabledIconFileName)) return string.Empty;
            return WebImageSupplier.GetAbsoluteWebPath(item.Context.DisabledIconFileName, item.ID);
        }

        public static string GetSmallIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.SmallIconFileName)) return string.Empty;
            return WebImageSupplier.GetAbsoluteWebPath(item.Context.SmallIconFileName, item.ID);
        }

        public static string GetIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.IconFileName)) return string.Empty;
            return WebImageSupplier.GetAbsoluteWebPath(item.Context.IconFileName, item.ID);
        }

        public static string GetLargeIconAbsoluteURL(this IWebItem item)
        {
            if (item == null || item.Context == null || String.IsNullOrEmpty(item.Context.LargeIconFileName)) return string.Empty;
            return WebImageSupplier.GetAbsoluteWebPath(item.Context.LargeIconFileName, item.ID);
        }

        public static List<string> GetUserOpportunities(this IWebItem item)
        {
            return item.Context.UserOpportunities != null ? item.Context.UserOpportunities() : new List<string>();
        }

        public static List<string> GetAdminOpportunities(this IWebItem item)
        {
            return item.Context.AdminOpportunities != null ? item.Context.AdminOpportunities() : new List<string>();
        }

        public static bool HasComplexHierarchyOfAccessRights(this IWebItem item)
        {
            return item.Context.HasComplexHierarchyOfAccessRights;
        }

        public static bool CanNotBeDisabled(this IWebItem item)
        {
            return item.Context.CanNotBeDisabled;
        }


        public static bool IsDisabled(this IWebItem item)
        {
            return IsDisabled(item, SecurityContext.CurrentAccount.ID);
        }

        public static bool IsDisabled(this IWebItem item, Guid userID)
        {
            return item != null && (!WebItemSecurity.IsAvailableForUser(item.ID.ToString("N"), userID) || !item.Visible);
        }

        public static bool IsSubItem(this IWebItem item)
        {
            return item is IModule && !(item is IProduct);
        }
    }
}
