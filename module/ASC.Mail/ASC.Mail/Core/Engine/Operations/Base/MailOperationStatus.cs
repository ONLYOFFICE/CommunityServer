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

namespace ASC.Mail.Core.Engine.Operations.Base
{
    [DataContract]
    public class MailOperationStatus
    {
        ///<example>true</example>
        [DataMember]
        public bool Completed { get; set; }

        ///<example>Id</example>
        [DataMember]
        public string Id { get; set; }

        ///<example>Status</example>
        [DataMember]
        public string Status { get; set; }

        ///<example>Error</example>
        [DataMember]
        public string Error { get; set; }

        ///<example type="int">100</example>
        [DataMember]
        public int Percents { get; set; }

        ///<example>Source</example>
        [DataMember]
        public string Source { get; set; }

        ///<example type="int">1</example>
        [DataMember]
        public int OperationType { get; set; }

        ///<example>Operation</example>
        [DataMember]
        public string Operation { get; set; }
    }
}
