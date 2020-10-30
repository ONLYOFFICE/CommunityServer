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

namespace ASC.Mail.Data.Contracts
{
    public class CrmContactData
    {
        public int Id { get; set; }
        public EntityTypes Type { get; set; }

        public string EntityTypeName {
            get
            {
                switch (Type)
                {
                    case EntityTypes.Contact:
                        return CrmEntityTypeNames.CONTACT;
                    case EntityTypes.Case:
                        return CrmEntityTypeNames.CASE;
                    case EntityTypes.Opportunity:
                        return CrmEntityTypeNames.OPPORTUNITY;
                    default:
                        throw new ArgumentException(string.Format("Invalid CrmEntityType: {0}", Type));
                }
            }
        }

        public enum EntityTypes
        {
            Contact = 1,
            Case = 2,
            Opportunity = 3
        }

        public static class CrmEntityTypeNames
        {
            public const string CONTACT = "contact";
            public const string CASE = "case";
            public const string OPPORTUNITY = "opportunity";
        }
    }
}