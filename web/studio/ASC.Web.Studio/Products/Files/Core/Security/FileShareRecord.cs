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

namespace ASC.Files.Core.Security
{
    public class FileShareRecord
    {
        public int Tenant { get; set; }

        public object EntryId { get; set; }

        public FileEntryType EntryType { get; set; }

        public Guid Subject { get; set; }

        public Guid Owner { get; set; }

        public FileShare Share { get; set; }

        public int Level { get; set; }


        public class ShareComparer : IComparer<FileShare>
        {
            private static readonly int[] ShareOrder = new[]
                {
                    (int)FileShare.None,
                    (int)FileShare.ReadWrite,
                    (int)FileShare.CustomFilter,
                    (int)FileShare.Review,
                    (int)FileShare.FillForms,
                    (int)FileShare.Comment,
                    (int)FileShare.Read,
                    (int)FileShare.Restrict,
                    (int)FileShare.Varies
                };

            public int Compare(FileShare x, FileShare y)
            {
                return Array.IndexOf(ShareOrder, (int)x).CompareTo(Array.IndexOf(ShareOrder, (int)y));
            }
        }
    }

    public class SmallShareRecord
    {
        public Guid ShareTo { get; set; }
        public Guid ShareParentTo { get; set; }
        public Guid ShareBy { get; set; }
        public DateTime ShareOn { get; set; }
        public FileShare Share { get; set; }
    }
}
