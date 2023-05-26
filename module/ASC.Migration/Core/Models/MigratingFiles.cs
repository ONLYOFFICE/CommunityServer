/*
 *
 * (c) Copyright Ascensio System Limited 2010-2023
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

using ASC.Migration.Core.Models.Api;

namespace ASC.Migration.Core.Models
{
    public abstract class MigratingFiles : ImportableEntity
    {
        public abstract int FoldersCount { get; }
        public abstract int FilesCount { get; }
        public abstract long BytesTotal { get; }
        public abstract string ModuleName { get; }

        public virtual MigratingApiFiles ToApiInfo()
        {
            return new MigratingApiFiles()
            {
                BytesTotal = BytesTotal,
                FilesCount = FilesCount,
                FoldersCount = FoldersCount,
                ModuleName = ModuleName
            };
        }

        protected MigratingFiles(Action<string, Exception> log) : base(log) { }
    }
}
