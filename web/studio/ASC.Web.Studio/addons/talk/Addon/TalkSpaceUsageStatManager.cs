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
