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
using System.Globalization;
using System.Text.RegularExpressions;
using ASC.Core;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.Files.Classes;

namespace ASC.Files.Thirdparty.SharePoint
{
    internal class SharePointDaoSelector : RegexDaoSelectorBase<string>
    {
        public SharePointDaoSelector()
            : base(new Regex(@"^spoint-(?'id'\d+)(-(?'path'.*)){0,1}$", RegexOptions.Singleline | RegexOptions.Compiled))
        {
        }

        public override IFileDao GetFileDao(object id)
        {
            return new SharePointFileDao(GetInfo(id), this);
        }

        public override IFolderDao GetFolderDao(object id)
        {
            return new SharePointFolderDao(GetInfo(id), this);
        }

        public override ITagDao GetTagDao(object id)
        {
            return new SharePointTagDao(GetInfo(id), this);
        }

        public override ISecurityDao GetSecurityDao(object id)
        {
            return new SharePointSecurityDao(GetInfo(id), this);
        }

        public override object ConvertId(object id)
        {
            if (id != null)
            {
                var match = Selector.Match(Convert.ToString(id, CultureInfo.InvariantCulture));
                if (match.Success)
                {
                    return GetInfo(id).SpRootFolderId + match.Groups["path"].Value.Replace('|', '/');
                }
                throw new ArgumentException("Id is not a sharepoint id");
            }
            return base.ConvertId(null);
        }

        private SharePointProviderInfo GetInfo(object objectId)
        {
            if (objectId == null) throw new ArgumentNullException("objectId");
            var id = Convert.ToString(objectId, CultureInfo.InvariantCulture);
            var match = Selector.Match(id);
            if (match.Success)
            {
                return GetProviderInfo(Convert.ToInt32(match.Groups["id"].Value));
            }
            throw new ArgumentException("Id is not a sharepoint id");
        }

        public override object GetIdCode(object id)
        {
            if (id != null)
            {
                var match = Selector.Match(Convert.ToString(id, CultureInfo.InvariantCulture));
                if (match.Success)
                {
                    return match.Groups["id"].Value;
                }
            }
            return base.GetIdCode(id);
        }

        private static SharePointProviderInfo GetProviderInfo(int linkId)
        {
            SharePointProviderInfo info;

            using (var dbDao = Global.DaoFactory.GetProviderDao())
            {
                try
                {
                    info = (SharePointProviderInfo)dbDao.GetProviderInfo(linkId);
                }
                catch (InvalidOperationException)
                {
                    throw new ArgumentException("Provider id not found or you have no access");
                }
            }
            return info;
        }

        public void RenameProvider(SharePointProviderInfo sharePointProviderInfo, string newTitle)
        {
            using (var dbDao = new CachedProviderAccountDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId))
            {
                dbDao.UpdateProviderInfo(sharePointProviderInfo.ID, newTitle, null, sharePointProviderInfo.RootFolderType);
                sharePointProviderInfo.UpdateTitle(newTitle); //This will update cached version too
            }
        }
    }
}