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


using System;
using System.Runtime.Serialization;

namespace ASC.Mail.Data.Contracts
{
    [Serializable]
    public class ContactInfo
    {
        ///<example name="id" type="int">1234</example>
        [DataMember(IsRequired = false, Name = "id")]
        public int Id { get; set; }

        ///<example name="value">value</example>
        [DataMember(IsRequired = true, Name = "value")]
        public string Value { get; set; }

        ///<example name="isPrimary">true</example>
        [DataMember(IsRequired = true, Name = "isPrimary")]
        public bool IsPrimary { get; set; }
    }
}