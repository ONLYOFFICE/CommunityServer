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
using ASC.Files.Core;
using ASC.Files.Core.Security;

namespace ASC.Files.Thirdparty.Box
{
    internal class BoxSecurityDao : BoxDaoBase, ISecurityDao
    {
        public BoxSecurityDao(BoxDaoSelector.BoxInfo providerInfo, BoxDaoSelector boxDaoSelector)
            : base(providerInfo, boxDaoSelector)
        {
        }

        public void SetShare(FileShareRecord r)
        {
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<Guid> subjects)
        {
            return null;
        }

        public IEnumerable<FileShareRecord> GetShares(IEnumerable<FileEntry> entry)
        {
            return null;
        }

        public IEnumerable<FileShareRecord> GetShares(FileEntry entry)
        {
            return null;
        }

        public void RemoveSubject(Guid subject)
        {
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(IEnumerable<FileEntry> entries)
        {
            return null;
        }

        public IEnumerable<FileShareRecord> GetPureShareRecords(FileEntry entry)
        {
            return null;
        }

        public void DeleteShareRecords(IEnumerable<FileShareRecord> records)
        {
        }

        public bool IsShared(object entryId, FileEntryType type)
        {
            throw new NotImplementedException();
        }
    }
}