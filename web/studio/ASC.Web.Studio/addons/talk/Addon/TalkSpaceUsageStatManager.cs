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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using ASC.Core;
using ASC.Core.Users;
using ASC.Data.Storage;
using ASC.Web.Core;
using ASC.Web.Studio.UserControls.Statistics;

namespace ASC.Web.Talk.Addon
{
    public class TalkSpaceUsageStatManager : SpaceUsageStatManager, IUserSpaceUsage
    {
        public override List<UsageSpaceStatItem> GetStatData()
        {
            var storage = GetStorage();

            if (storage == null)
            {
                return WebItemManager.Instance.GetItemsAll()
                                     .Where(webItem => webItem.ID == WebItemManager.TalkProductID)
                                     .Select(webItem => new UsageSpaceStatItem
                                         {
                                             Name = webItem.Name,
                                             SpaceUsage = TenantStatisticsProvider.GetUsedSize(webItem.ID),
                                             Url = VirtualPathUtility.ToAbsolute(webItem.StartURL)
                                         })
                                     .ToList();
            }

            return CoreContext.UserManager.GetUsers(EmployeeStatus.All, EmployeeType.All)
                              .Select(userInfo => ToUsageSpaceStatItem(storage, userInfo))
                              .Where(item => item != null)
                              .OrderByDescending(item => item.SpaceUsage)
                              .ToList();
        }

        public long GetUserSpaceUsage(Guid userId)
        {
            return GetSpaceUsage(userId);
        }

        private static IDataStore GetStorage()
        {
            var tenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId;

            return StorageFactory.GetStorage(tenantId.ToString(CultureInfo.InvariantCulture), "talk");
        }

        private static UsageSpaceStatItem ToUsageSpaceStatItem(IDataStore storage, UserInfo userInfo)
        {
            if (userInfo.Equals(Constants.LostUser)) return null;
            
            var md5Hash = GetUserMd5Hash(userInfo.ID);

            if (!storage.IsDirectory(md5Hash)) return null;

            return new UsageSpaceStatItem
                {
                    SpaceUsage = storage.GetDirectorySize(md5Hash),
                    Name = userInfo.DisplayUserName(false),
                    ImgUrl = userInfo.GetSmallPhotoURL(),
                    Url = userInfo.GetUserProfilePageURL(),
                    Disabled = userInfo.Status == EmployeeStatus.Terminated
                };
        }

        public static string GetUserMd5Hash(Guid userId)
        {
            var data = MD5.Create().ComputeHash(Encoding.Default.GetBytes(userId.ToString()));

            var sBuilder = new StringBuilder();

            for (Int32 i = 0, n = data.Length; i < n; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static long GetSpaceUsage()
        {
            return GetSpaceUsage(SecurityContext.CurrentAccount.ID);
        }

        private static long GetSpaceUsage(Guid userId)
        {
            var storage = GetStorage();

            if (storage == null) throw new Exception("storage not found");

            var hash = GetUserMd5Hash(userId);

            return storage.IsDirectory(hash) ? storage.GetDirectorySize(hash) : 0;
        }

        public static void ClearSpaceUsage(ClearType type)
        {
            var storage = GetStorage();

            if (storage == null) throw new Exception("storage not found");

            var hash = GetUserMd5Hash(SecurityContext.CurrentAccount.ID);

            switch (type)
            {
                case ClearType.All:
                    storage.DeleteDirectory(string.Empty, hash);
                    break;
                case ClearType.Month:
                    storage.DeleteFiles(string.Empty, hash, DateTime.MinValue, DateTime.Now.AddMonths(-1));
                    break;
                case ClearType.Year:
                    storage.DeleteFiles(string.Empty, hash, DateTime.MinValue, DateTime.Now.AddYears(-1));
                    break;
            }
        }

        public enum ClearType
        {
            All,
            Month,
            Year
        }
    }
}
