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
using ASC.CRM.Core;
using ASC.CRM.Core.Entities;
using ASC.Specific;

namespace ASC.Api.CRM.Wrappers
{
    [DataContract(Name = "customField", Namespace = "")]
    public class CustomFieldWrapper : CustomFieldBaseWrapper
    {
        public CustomFieldWrapper(int id)
            : base(id)
        {
        }

        public CustomFieldWrapper(CustomField customField)
            : base(customField)
        {
        }

        [DataMember]
        public int RelativeItemsCount { get; set; }

        public new static CustomFieldWrapper GetSample()
        {
            return new CustomFieldWrapper(0)
                {
                    Position = 10,
                    EntityId = 14523423,
                    FieldType = CustomFieldType.Date,
                    FieldValue = ApiDateTime.GetSample().ToString(),
                    Label = "Birthdate",
                    Mask = "",
                    RelativeItemsCount = 0
                };
        }
    }

    /// <summary>
    ///  User custom fields
    /// </summary>
    [DataContract(Name = "customField", Namespace = "")]
    public class CustomFieldBaseWrapper : ObjectWrapperBase
    {
        public CustomFieldBaseWrapper(int id) : base(id)
        {
        }

        public CustomFieldBaseWrapper(CustomField customField)
            : base(customField.ID)
        {
            EntityId = customField.EntityID;
            Label = customField.Label;
            FieldValue = customField.Value;
            FieldType = customField.FieldType;
            Position = customField.Position;
            Mask = customField.Mask;
        }

        [DataMember]
        public int EntityId { get; set; }

        [DataMember]
        public String Label { get; set; }

        [DataMember]
        public String FieldValue { get; set; }

        [DataMember]
        public CustomFieldType FieldType { get; set; }

        [DataMember]
        public int Position { get; set; }

        [DataMember]
        public String Mask { get; set; }

        public static CustomFieldBaseWrapper GetSample()
        {
            return new CustomFieldBaseWrapper(0)
                {
                    Position = 10,
                    EntityId = 14523423,
                    FieldType = CustomFieldType.Date,
                    FieldValue = ApiDateTime.GetSample().ToString(),
                    Label = "Birthdate",
                    Mask = ""
                };
        }
    }
}