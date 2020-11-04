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
using ASC.Files.Core;
using ASC.Web.Files.Api;

namespace ASC.CRM.Core.Dao
{
    public class FileDao : AbstractDao
    {
        public FileDao(int tenantID)
            : base(tenantID)
        {
        }

        #region Public Methods

        public File GetFile(int id, int version)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                var file = 0 < version ? dao.GetFile(id, version) : dao.GetFile(id);
                return file;
            }
        }

        public void DeleteFile(int id)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                dao.DeleteFile(id);
            }
        }

        public object GetRoot()
        {
            return FilesIntegration.RegisterBunch("crm", "crm_common", "");
        }

        public object GetMy()
        {
            return FilesIntegration.RegisterBunch("files", "my", SecurityContext.CurrentAccount.ID.ToString());
        }

        public File SaveFile(File file, System.IO.Stream stream)
        {
            using (var dao = FilesIntegration.GetFileDao())
            {
                return dao.SaveFile(file, stream);
            }
        }

        public List<int> GetEventsByFile(int id)
        {
            using (var tagdao = FilesIntegration.GetTagDao())
            {
                var tags = tagdao.GetTags(id, FileEntryType.File, TagType.System).ToList().FindAll(tag => tag.TagName.StartsWith("RelationshipEvent_"));
                return tags.Select(item => Convert.ToInt32(item.TagName.Split(new[] { '_' })[1])).ToList();
            }
        }

        #endregion
    }
}