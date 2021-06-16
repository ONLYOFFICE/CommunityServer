/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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


using ASC.Core;
using ASC.Files.Core.Security;

namespace ASC.Files.Core.Data
{
    public class DaoFactory : IDaoFactory
    {
        public DaoFactory()
        {

        }

        public IFileDao GetFileDao()
        {
            return new FileDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId);
        }

        public IFolderDao GetFolderDao()
        {
            return new FolderDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId);
        }

        public ITagDao GetTagDao()
        {
            return new TagDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId);
        }

        public ISecurityDao GetSecurityDao()
        {
            return new SecurityDao(CoreContext.TenantManager.GetCurrentTenant().TenantId, FileConstant.DatabaseId);
        }

        public IProviderDao GetProviderDao()
        {
            return null;
        }
    }
}