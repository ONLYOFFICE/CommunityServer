/*
 *
 * (c) Copyright Ascensio System Limited 2010-2016
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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ASC.Files.Core;
using ASC.Web.Files.Services.WCFService;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class FolderContentWrapper
    {
        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<FileWrapper> Files { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = false)]
        public List<FolderWrapper> Folders { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public FolderWrapper Current { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public object PathParts { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public int StartIndex { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public int Count { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public int Total { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="folderItems"></param>
        /// <param name="startIndex"></param>
        public FolderContentWrapper(DataWrapper folderItems, int startIndex)
        {
            Files = folderItems.Entries.OfType<File>().Select(x => new FileWrapper(x)).ToList();
            Folders = folderItems.Entries.OfType<Folder>().Select(x => new FolderWrapper(x)).ToList();
            Current = new FolderWrapper(folderItems.FolderInfo);
            PathParts = folderItems.FolderPathParts;

            StartIndex = startIndex;
            Count = Files.Count + Folders.Count;
            Total = folderItems.Total;
        }

        private FolderContentWrapper()
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static FolderContentWrapper GetSample()
        {
            return new FolderContentWrapper
                {
                    Current = FolderWrapper.GetSample(),
                    Files = new List<FileWrapper>(new[] {FileWrapper.GetSample(), FileWrapper.GetSample()}),
                    Folders = new List<FolderWrapper>(new[] {FolderWrapper.GetSample(), FolderWrapper.GetSample()}),
                    PathParts = new
                        {
                            key = "Key",
                            path = "//path//to//folder"
                        },

                    StartIndex = 0,
                    Count = 4,
                    Total = 4,
                };
        }
    }
}