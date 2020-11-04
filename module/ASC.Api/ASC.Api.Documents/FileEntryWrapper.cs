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
using System.Runtime.Serialization;
using ASC.Api.Employee;
using ASC.Files.Core;
using ASC.Files.Core.Security;
using ASC.Specific;

namespace ASC.Api.Documents
{
    /// <summary>
    /// </summary>
    [DataContract(Namespace = "")]
    public abstract class FileEntryWrapper
    {
        /// <summary>
        /// </summary>
        [DataMember]
        public object Id { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Title { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public FileShare Access { get; set; }

        /// <summary>
        /// </summary>
        [DataMember]
        public bool Shared { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Order = 50)]
        public ApiDateTime Created { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Order = 51, EmitDefaultValue = false)]
        public EmployeeWraper CreatedBy { get; set; }

        private ApiDateTime _updated;

        /// <summary>
        /// </summary>
        [DataMember(Order = 52, EmitDefaultValue = false)]
        public ApiDateTime Updated
        {
            get
            {
                return _updated < Created ? Created : _updated;
            }
            set { _updated = value; }
        }

        /// <summary>
        /// </summary>
        [DataMember(Order = 41, EmitDefaultValue = false)]
        public FolderType RootFolderType { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Order = 41, EmitDefaultValue = false)]
        public EmployeeWraper UpdatedBy { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Order = 55, EmitDefaultValue = false)]
        public bool ProviderItem { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Order = 56, EmitDefaultValue = false)]
        public string ProviderKey { get; set; }

        /// <summary>
        /// </summary>
        [DataMember(Order = 57, EmitDefaultValue = false)]
        public int ProviderId { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        protected FileEntryWrapper(FileEntry entry)
        {
            Id = entry.ID;
            Title = entry.Title;
            Access = entry.Access;
            Shared = entry.Shared;
            Created = (ApiDateTime)entry.CreateOn;
            CreatedBy = EmployeeWraper.Get(entry.CreateBy);
            Updated = (ApiDateTime)entry.ModifiedOn;
            UpdatedBy = EmployeeWraper.Get(entry.ModifiedBy);
            RootFolderType = entry.RootFolderType;
            ProviderItem = entry.ProviderEntry;
            ProviderKey = entry.ProviderKey;
            ProviderId = entry.ProviderId;
        }

        /// <summary>
        /// 
        /// </summary>
        protected FileEntryWrapper()
        {

        }
    }
}