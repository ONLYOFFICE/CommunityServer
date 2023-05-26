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


using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using ASC.Api.Employee;
using ASC.Specific;
using ASC.VoipService;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "voipCall", Namespace = "")]
    public class VoipCallWrapper
    {
        ///<example>Id</example>
        ///<order>1</order>
        [DataMember(Order = 1)]
        public string Id { get; set; }

        ///<example>From</example>
        ///<order>2</order>
        [DataMember(Order = 2)]
        public string From { get; set; }

        ///<example>To</example>
        ///<order>3</order>
        [DataMember(Order = 3)]
        public string To { get; set; }

        ///<example type="int">1</example>
        ///<order>4</order>
        [DataMember(Order = 4)]
        public VoipCallStatus? Status { get; set; }

        ///<type>ASC.Api.Employee.EmployeeWraper, ASC.Api.Employee</type>
        ///<order>5</order>
        [DataMember(Order = 5)]
        public EmployeeWraper AnsweredBy { get; set; }

        ///<example>2020-12-23T04:11:57.1649297Z</example>
        ///<order>6</order>
        [DataMember(Order = 6)]
        public ApiDateTime DialDate { get; set; }

        ///<example type="int">1</example>
        ///<order>7</order>
        [DataMember(Order = 7)]
        public int DialDuration { get; set; }

        ///<example type="double">5.3</example>
        ///<order>10</order>
        [DataMember(Order = 10)]
        public decimal Cost { get; set; }

        ///<type>ASC.Api.CRM.Wrappers.ContactWrapper, ASC.Api.CRM</type>
        ///<order>11</order>
        [DataMember(Order = 11)]
        public ContactWrapper Contact { get; set; }

        ///<example>null</example>
        ///<order>11</order>
        [DataMember(Order = 11, EmitDefaultValue = false)]
        public IEnumerable<VoipCallWrapper> Calls { get; set; }

        ///<example>RecordUrl</example>
        ///<order>13</order>
        [DataMember(Order = 13)]
        public string RecordUrl { get; set; }

        ///<example type="int">2</example>
        ///<order>14</order>
        [DataMember(Order = 14)]
        public int RecordDuration { get; set; }

        public VoipCallWrapper(VoipCall call, ContactWrapper contact = null)
        {
            Id = call.Id;
            From = call.From;
            To = call.To;
            Status = call.Status;
            AnsweredBy = EmployeeWraper.Get(call.AnsweredBy);
            DialDate = new ApiDateTime(call.DialDate);
            DialDuration = call.DialDuration;
            Cost = call.Price + call.ChildCalls.Sum(r => r.Price) + call.VoipRecord.Price;
            Contact = contact;
            RecordUrl = call.VoipRecord.Uri;
            RecordDuration = call.VoipRecord.Duration;

            if (call.ChildCalls.Any())
            {
                Calls = call.ChildCalls.Select(childCall => new VoipCallWrapper(childCall));
            }
        }
    }
}