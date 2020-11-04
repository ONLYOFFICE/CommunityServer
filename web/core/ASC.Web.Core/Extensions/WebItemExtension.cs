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
            return item != null && (!WebItemSecurity.IsAvailableForUser(item.ID, userID) || !item.Visible);
        }

        public static bool IsSubItem(this IWebItem item)
        {
            return item is IModule && !(item is IProduct);
        }
    }
}
