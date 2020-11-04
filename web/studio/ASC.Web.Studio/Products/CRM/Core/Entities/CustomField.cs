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


#region Import

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

#endregion

namespace ASC.CRM.Core.Entities
{
    
    [DataContract]
    public class CustomField : DomainObject
    {
        [DataMember(Name = "entity_type")]
        public EntityType EntityType { get; set; }
        
        [DataMember(Name = "entity_id")]
        public int EntityID { get; set; }

        [DataMember(Name = "label")]
        public String Label { get; set; }

        [DataMember(Name = "value")]
        public String Value { get; set; }

        [DataMember(Name = "fieldType")]
        public CustomFieldType FieldType { get; set; }

        [DataMember(Name = "position")]
        public int Position { get; set; }

        [DataMember(Name = "mask")]
        public String Mask { get; set; }

        public override int GetHashCode()
        {
            return string.Format("{0}|{1}|{2}", GetType().FullName, Label, (int)FieldType).GetHashCode();
        }
    }
}
