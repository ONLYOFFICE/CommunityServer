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

using ASC.Core;
using ASC.ElasticSearch;
using ASC.Files.Core;

namespace ASC.Web.Files.Core.Search
{
    public sealed class FoldersWrapper : Wrapper
    {
        [Column("title", 1)]
        public string Title { get; set; }

        [ColumnLastModified("modified_on")]
        public override DateTime LastModifiedOn { get; set; }

        [ColumnCondition("folder_type", 1, FolderType.DEFAULT, FolderType.BUNCH)]
        public FolderType FolderType { get; set; }

        protected override string Table { get { return "files_folder"; } }

        public static implicit operator FoldersWrapper(Folder d)
        {
            return new FoldersWrapper
            {
                Id = (int)d.ID,
                Title = d.Title,
                TenantId = CoreContext.TenantManager.GetCurrentTenant().TenantId
            };
        }
    }
}