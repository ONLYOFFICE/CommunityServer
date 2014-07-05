/*
(c) Copyright Ascensio System SIA 2010-2014

This program is a free software product.
You can redistribute it and/or modify it under the terms 
of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of 
any third-party rights.

This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty 
of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see 
the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html

You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.

The  interactive user interfaces in modified source and object code versions of the Program must 
display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
 
Pursuant to Section 7(b) of the License you must retain the original Product logo when 
distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under 
trademark law for use of our trademarks.
 
All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode
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
                    FieldValue = ((ApiDateTime)DateTime.UtcNow).ToString(),
                    Label = "Birthdate",
                    Mask = ""
                };
        }
    }
}