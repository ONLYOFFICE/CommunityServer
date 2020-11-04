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


using System.Runtime.Serialization;

namespace ASC.Web.Files.Services.WCFService.FileOperations
{
    [DataContract(Name = "operation_result", Namespace = "")]
    public class FileOperationResult
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "operation")]
        public FileOperationType OperationType { get; set; }

        [DataMember(Name = "progress")]
        public int Progress { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "result")]
        public string Result { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "processed")]
        public string Processed { get; set; }

        [DataMember(Name = "finished")]
        public bool Finished { get; set; }
    }
}