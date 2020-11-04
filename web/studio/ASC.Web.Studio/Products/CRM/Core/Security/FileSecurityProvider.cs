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
using ASC.CRM.Core.Dao;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Web.CRM.Core;
using ASC.Web.Files.Api;
using Autofac;

namespace ASC.CRM.Core
{
    public class FileSecurity : IFileSecurity
    {
        public bool CanCreate(FileEntry entry, Guid userId)
        {
            return true;
        }

        public bool CanComment(FileEntry entry, Guid userId)
        {
            return CanEdit(entry, userId);
        }

        public bool CanCustomFilterEdit(FileEntry file, Guid userId)
        {
            return CanEdit(file, userId);
        }

        public bool CanFillForms(FileEntry entry, Guid userId)
        {
            return CanEdit(entry, userId);
        }

        public bool CanReview(FileEntry entry, Guid userId)
        {
            return CanEdit(entry, userId);
        }

        public bool CanDelete(FileEntry entry, Guid userId)
        {
            return CanEdit(entry, userId);
        }

        public bool CanEdit(FileEntry entry, Guid userId)
        {
            return
                CanRead(entry, userId) &&
                entry.CreateBy == userId || entry.ModifiedBy == userId || CRMSecurity.IsAdministrator(userId);
        }

        public bool CanRead(FileEntry entry, Guid userId)
        {
            if (entry.FileEntryType == FileEntryType.Folder) return false;

            if (!CRMSecurity.IsAvailableForUser(userId)) return false;

            using (var scope = DIHelper.Resolve())
            {
                var daoFactory = scope.Resolve<DaoFactory>();
                var invoice = daoFactory.InvoiceDao.GetByFileId(Convert.ToInt32(entry.ID));
                if (invoice != null)
                    return CRMSecurity.CanAccessTo(invoice, userId);

                var reportFile = daoFactory.ReportDao.GetFile(Convert.ToInt32(entry.ID), userId);
                if (reportFile != null)
                    return CRMSecurity.IsAdministrator(userId);

                using (var tagDao = FilesIntegration.GetTagDao())
                {
                    var eventIds = tagDao.GetTags(entry.ID, FileEntryType.File, TagType.System)
                        .Where(x => x.TagName.StartsWith("RelationshipEvent_"))
                        .Select(x => Convert.ToInt32(x.TagName.Split(new[] { '_' })[1]))
                        .ToList();

                    if (!eventIds.Any()) return false;

                    var eventItem = daoFactory.RelationshipEventDao.GetByID(eventIds.First());
                    return CRMSecurity.CanAccessTo(eventItem, userId);
                }
            }
        }

        public IEnumerable<Guid> WhoCanRead(FileEntry entry)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSecurityProvider : IFileSecurityProvider
    {
        public IFileSecurity GetFileSecurity(string data)
        {
            return new FileSecurity();
        }

        public Dictionary<object, IFileSecurity> GetFileSecurity(Dictionary<string, string> data)
        {
            return data.ToDictionary<KeyValuePair<string, string>, object, IFileSecurity>(d => d.Key, d => new FileSecurity());
        }
    }
}