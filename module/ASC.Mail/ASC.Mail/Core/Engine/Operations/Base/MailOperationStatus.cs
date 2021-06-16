/*
 *
 * (c) Copyright Ascensio System Limited 2010-2021
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
        [DataMember]
        public bool Completed { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Error { get; set; }

        [DataMember]
        public int Percents { get; set; }

        [DataMember]
        public string Source { get; set; }

        [DataMember]
        public int OperationType { get; set; }

        [DataMember]
        public string Operation { get; set; }
    }
}
