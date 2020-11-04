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