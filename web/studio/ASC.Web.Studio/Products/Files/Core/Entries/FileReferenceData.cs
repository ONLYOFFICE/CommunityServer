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


using System.Runtime.Serialization;

namespace ASC.Web.Files.Core
{
    [DataContract(Name = "FileReference", Namespace = "")]
    public class FileReference
    {
        [DataMember(Name = "error")] public string Error;
        [DataMember(Name = "fileType")] public string FileType;
        [DataMember(Name = "key")] public string Key;
        [DataMember(Name = "link")] public string Link;
        [DataMember(Name = "path")] public string Path;
        [DataMember(Name = "referenceData")] public FileReferenceData ReferenceData;
        [DataMember(Name = "token", EmitDefaultValue = false)] public string Token;
        [DataMember(Name = "url")] public string Url;

        [DataContract(Name = "ReferenceData", Namespace = "")]
        public class FileReferenceData
        {
            [DataMember(Name = "fileKey")] public string FileKey;
            [DataMember(Name = "instanceId")] public string InstanceId;
        }
    }
}