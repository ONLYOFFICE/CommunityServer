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
            return item != null && (!WebItemSecurity.IsAvailableForUser(item.ID.ToString("N"), userID) || !item.IsLicensed());
        }

        public static bool IsLicensed(this IWebItem item)
        {
            return WebItemSecurity.IsLicensed(item);
        }

        public static bool IsSubItem(this IWebItem item)
        {
            return item is IModule && !(item is IProduct);
        }
    }
}
