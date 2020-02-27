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
