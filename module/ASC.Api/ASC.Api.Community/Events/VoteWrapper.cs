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

namespace ASC.Api.Events
{
    [DataContract(Name = "vote", Namespace = "")]
    public class VoteWrapper
    {
        ///<example type="int">133</example>
        ///<order>1</order>
        [DataMember(Order = 1, EmitDefaultValue = true)]
        public long Id { get; set; }

        ///<example>Variant 1</example>
        ///<order>10</order>
        [DataMember(Order = 10, EmitDefaultValue = true)]
        public string Name { get; set; }

        ///<example type="int">100</example>
        ///<order>20</order>
        [DataMember(Order = 20, EmitDefaultValue = true)]
        public int Votes { get; set; }

        public static VoteWrapper GetSample()
        {
            return new VoteWrapper
            {
                Votes = 100,
                Name = "Variant 1",
                Id = 133
            };
        }
    }
}