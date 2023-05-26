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

namespace ASC.Api.Settings.Smtp
{
    [DataContract]
    public class SmtpOperationStatus
    {
        ///<example>true</example>
        [DataMember]
        public bool Completed { get; set; }

        ///<example>{some-random-guid}</example>
        [DataMember]
        public string Id { get; set; }

        ///<example></example>
        [DataMember]
        public string Status { get; set; }

        ///<example></example>
        [DataMember]
        public string Error { get; set; }

        ///<example type="int">0</example>
        [DataMember]
        public int Percents { get; set; }

        ///<example></example>
        [DataMember]
        public string Source { get; set; }

        public static SmtpOperationStatus GetSample()
        {
            return new SmtpOperationStatus
            {
                Id = "{some-random-guid}",
                Error = "",
                Percents = 0,
                Completed = true,
                Status = "",
                Source = ""
            };
        }
    }
}